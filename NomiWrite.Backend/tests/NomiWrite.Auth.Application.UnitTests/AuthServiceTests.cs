using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using MassTransit;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using NSubstitute;
using NomiWrite.Auth.Application.DTOs;
using NomiWrite.Auth.Application.Exceptions;
using NomiWrite.Auth.Application.Interfaces;
using NomiWrite.Auth.Application.Options;
using NomiWrite.Auth.Application.Services;
using NomiWrite.Auth.Application.UnitTests.Persistence;
using NomiWrite.Auth.Domain.Entities;
using NomiWrite.Auth.Domain.Enums;
using NomiWrite.Shared.Contracts.Events.Auth;
using OptionsSet = Microsoft.Extensions.Options.Options;

namespace NomiWrite.Auth.Application.UnitTests;

public class AuthServiceTests
{
    private static readonly DateTime Now = DateTime.UtcNow;

    private static AuthService Build(
        TestAuthDbContext dbContext,
        IPasswordHasher? hasher = null,
        IJwtTokenService? jwt = null,
        IPublishEndpoint? publish = null,
        IValidator<RegisterRequestDto>? registerValidator = null,
        IValidator<LoginRequestDto>? loginValidator = null)
    {
        hasher ??= Substitute.For<IPasswordHasher>();
        jwt ??= Substitute.For<IJwtTokenService>();
        publish ??= Substitute.For<IPublishEndpoint>();

        var emailSender = Substitute.For<IEmailSender>();
        var appSettings = OptionsSet.Create(new AppSettings { FrontendBaseUrl = "http://localhost:3000" });
        var googleSettings = OptionsSet.Create(new GoogleAuthSettings { ClientId = "client-id" });
        var logger = NullLogger<AuthService>.Instance;

        return new AuthService(
            dbContext,
            hasher,
            jwt,
            registerValidator ?? Valid.RegisterValidator(),
            loginValidator ?? Valid.LoginValidator(),
            Valid.VerifyEmailValidator(),
            Valid.ResendValidator(),
            Valid.ForgotValidator(),
            Valid.ResetValidator(),
            emailSender,
            appSettings,
            googleSettings,
            publish,
            logger);
    }

    private static User SeedUser(TestAuthDbContext dbContext, string email = "user@example.com",
        AccountStatus status = AccountStatus.Active, bool isDeleted = false)
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = email,
            PasswordHash = "hash",
            FullName = "Test User",
            Role = UserRole.Student,
            AccountStatus = status,
            IsEmailVerified = true,
            IsDeleted = isDeleted,
            CreatedAt = Now
        };
        dbContext.Users.Add(user);
        dbContext.SaveChanges();
        return user;
    }

    private static RefreshToken SeedRefreshToken(TestAuthDbContext dbContext, Guid userId,
        string token, DateTime expiresAt, bool isRevoked = false)
    {
        var refresh = new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Token = token,
            ExpiresAt = expiresAt,
            IsRevoked = isRevoked,
            CreatedAt = Now
        };
        dbContext.RefreshTokens.Add(refresh);
        dbContext.SaveChanges();
        return refresh;
    }

    private static Guid SeedEmailVerificationToken(TestAuthDbContext dbContext, Guid userId, string token,
        DateTime expiresAt, bool isUsed = false)
    {
        var entity = new EmailVerificationToken
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Token = token,
            ExpiresAt = expiresAt,
            IsUsed = isUsed,
            CreatedAt = Now
        };
        dbContext.EmailVerificationTokens.Add(entity);
        dbContext.SaveChanges();
        return entity.Id;
    }

    private static Guid SeedPasswordResetToken(TestAuthDbContext dbContext, Guid userId, string token,
        DateTime expiresAt, bool isUsed = false)
    {
        var entity = new PasswordResetToken
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Token = token,
            ExpiresAt = expiresAt,
            IsUsed = isUsed,
            CreatedAt = Now
        };
        dbContext.PasswordResetTokens.Add(entity);
        dbContext.SaveChanges();
        return entity.Id;
    }

    #region U-A1 / U-A2 — refresh token rotation

    [Fact]
    public async Task RefreshTokenAsync_WithExpiredToken_ThrowsInvalidRefreshToken()
    {
        var db = TestAuthDbContext.Create();
        var user = SeedUser(db);
        SeedRefreshToken(db, user.Id, "expired-token", Now.AddDays(-1));

        var sut = Build(db);
        var act = () => sut.RefreshTokenAsync(new RefreshTokenRequestDto { RefreshToken = "expired-token" });

        await act.Should().ThrowAsync<InvalidRefreshTokenException>();
    }

    [Fact]
    public async Task RefreshTokenAsync_WithUnknownToken_ThrowsInvalidRefreshToken()
    {
        var db = TestAuthDbContext.Create();
        var sut = Build(db);

        var act = () => sut.RefreshTokenAsync(new RefreshTokenRequestDto { RefreshToken = "nope" });

        await act.Should().ThrowAsync<InvalidRefreshTokenException>();
    }

    [Fact]
    public async Task RefreshTokenAsync_WithValidToken_RotatesAndRevokesOld()
    {
        var db = TestAuthDbContext.Create();
        var user = SeedUser(db);
        var old = SeedRefreshToken(db, user.Id, "old-token", Now.AddDays(7));

        var jwt = Substitute.For<IJwtTokenService>();
        jwt.GenerateAccessToken(Arg.Any<User>()).Returns(("new-access", Now.AddMinutes(30)));
        jwt.GenerateRefreshToken().Returns("new-refresh");

        var sut = Build(db, jwt: jwt);
        var result = await sut.RefreshTokenAsync(new RefreshTokenRequestDto { RefreshToken = old.Token });

        result.AccessToken.Should().Be("new-access");
        result.RefreshToken.Should().Be("new-refresh");
        db.RefreshTokens.Single(t => t.Token == "old-token").IsRevoked.Should().BeTrue();
        db.RefreshTokens.Should().Contain(t => t.Token == "new-refresh" && !t.IsRevoked);
    }

    [Fact]
    public async Task RefreshTokenAsync_ReuseOfRevokedToken_RevokesEntireFamily()
    {
        var db = TestAuthDbContext.Create();
        var user = SeedUser(db);
        var stolen = SeedRefreshToken(db, user.Id, "stolen", Now.AddDays(7), isRevoked: true);
        var sibling = SeedRefreshToken(db, user.Id, "sibling", Now.AddDays(7), isRevoked: false);

        var sut = Build(db);
        var act = () => sut.RefreshTokenAsync(new RefreshTokenRequestDto { RefreshToken = stolen.Token });

        await act.Should().ThrowAsync<InvalidRefreshTokenException>();
        db.RefreshTokens.Single(t => t.Token == sibling.Token).IsRevoked.Should().BeTrue();
    }

    #endregion

    #region U-A3 — login account-state machine

    [Fact]
    public async Task LoginAsync_SoftDeletedAccount_ThrowsAccountDeactivated()
    {
        var db = TestAuthDbContext.Create();
        SeedUser(db, isDeleted: true);

        var hasher = Substitute.For<IPasswordHasher>();
        hasher.VerifyPassword(Arg.Any<string>(), Arg.Any<string>()).Returns(true);

        var sut = Build(db, hasher: hasher);
        var act = () => sut.LoginAsync(new LoginRequestDto { Email = "user@example.com", Password = "password" });

        await act.Should().ThrowAsync<AccountDeactivatedException>();
    }

    [Fact]
    public async Task LoginAsync_DeactivatedStatus_ThrowsAccountDeactivated()
    {
        var db = TestAuthDbContext.Create();
        SeedUser(db, status: AccountStatus.Deactivated);

        var hasher = Substitute.For<IPasswordHasher>();
        hasher.VerifyPassword(Arg.Any<string>(), Arg.Any<string>()).Returns(true);

        var sut = Build(db, hasher: hasher);
        var act = () => sut.LoginAsync(new LoginRequestDto { Email = "user@example.com", Password = "password" });

        await act.Should().ThrowAsync<AccountDeactivatedException>();
    }

    [Fact]
    public async Task LoginAsync_BannedStatus_ThrowsAccountBanned()
    {
        var db = TestAuthDbContext.Create();
        SeedUser(db, status: AccountStatus.Banned);

        var hasher = Substitute.For<IPasswordHasher>();
        hasher.VerifyPassword(Arg.Any<string>(), Arg.Any<string>()).Returns(true);

        var sut = Build(db, hasher: hasher);
        var act = () => sut.LoginAsync(new LoginRequestDto { Email = "user@example.com", Password = "password" });

        await act.Should().ThrowAsync<AccountBannedException>();
    }

    [Fact]
    public async Task LoginAsync_WithWrongPassword_ThrowsInvalidCredentials()
    {
        var db = TestAuthDbContext.Create();
        SeedUser(db);

        var hasher = Substitute.For<IPasswordHasher>();
        hasher.VerifyPassword(Arg.Any<string>(), Arg.Any<string>()).Returns(false);

        var sut = Build(db, hasher: hasher);
        var act = () => sut.LoginAsync(new LoginRequestDto { Email = "user@example.com", Password = "wrong" });

        await act.Should().ThrowAsync<InvalidCredentialsException>();
    }

    [Fact]
    public async Task LoginAsync_WithValidCredentials_ReturnsTokens()
    {
        var db = TestAuthDbContext.Create();
        var user = SeedUser(db);

        var hasher = Substitute.For<IPasswordHasher>();
        hasher.VerifyPassword(Arg.Any<string>(), Arg.Any<string>()).Returns(true);

        var jwt = Substitute.For<IJwtTokenService>();
        jwt.GenerateAccessToken(Arg.Any<User>()).Returns(("access-token", Now.AddMinutes(30)));
        jwt.GenerateRefreshToken().Returns("refresh-token");

        var sut = Build(db, hasher: hasher, jwt: jwt);
        var result = await sut.LoginAsync(new LoginRequestDto { Email = "user@example.com", Password = "pw" });

        result.UserId.Should().Be(user.Id);
        result.RefreshToken.Should().Be("refresh-token");
        db.RefreshTokens.Should().HaveCount(1);
    }

    #endregion

    #region U-A5 — no account enumeration

    [Fact]
    public async Task ForgotPasswordAsync_ExistingUser_ReturnsGenericSuccess()
    {
        var db = TestAuthDbContext.Create();
        SeedUser(db);

        var sut = Build(db);
        var result = await sut.ForgotPasswordAsync(new ForgotPasswordRequestDto { Email = "user@example.com" });

        result.Success.Should().BeTrue();
        result.Message.Should().Contain("password reset link has been sent");
    }

    [Fact]
    public async Task ForgotPasswordAsync_MissingUser_ReturnsSameGenericMessage()
    {
        var db = TestAuthDbContext.Create();

        var sut = Build(db);
        var result = await sut.ForgotPasswordAsync(new ForgotPasswordRequestDto { Email = "ghost@example.com" });

        result.Success.Should().BeTrue();
        result.Message.Should().Be(
            "If an account with that email exists, a password reset link has been sent.");
    }

    [Fact]
    public async Task ResendVerificationEmailAsync_MissingEmail_ReturnsGenericMessage()
    {
        var db = TestAuthDbContext.Create();
        db.Users.Add(new User
        {
            Id = Guid.NewGuid(),
            Email = "someone@example.com",
            FullName = "x",
            // no roles here need to resemble a real user; IsVerified false
        });
        db.SaveChanges();

        var sut = Build(db);
        var result = await sut.ResendVerificationEmailAsync(
            new ResendVerificationEmailRequestDto { Email = "nobody@example.com" },
            Guid.NewGuid());

        result.Success.Should().BeTrue();
    }

    #endregion

    #region U-A6 — reset password invalidates sessions

    [Fact]
    public async Task ResetPasswordAsync_RevokesAllRefreshTokens()
    {
        var db = TestAuthDbContext.Create();
        var user = SeedUser(db);
        SeedRefreshToken(db, user.Id, "active-1", Now.AddDays(7));
        SeedRefreshToken(db, user.Id, "active-2", Now.AddDays(7));
        SeedPasswordResetToken(db, user.Id, "reset-token", Now.AddHours(1));

        var hasher = Substitute.For<IPasswordHasher>();
        hasher.HashPassword(Arg.Any<string>()).Returns("new-hash");

        var sut = Build(db, hasher: hasher);
        var result = await sut.ResetPasswordAsync(new ResetPasswordRequestDto
        {
            Token = "reset-token",
            NewPassword = "NewPass123!"
        });

        result.Success.Should().BeTrue();
        db.RefreshTokens.Should().OnlyContain(t => t.IsRevoked);
    }

    [Fact]
    public async Task ResetPasswordAsync_ExpiredToken_ReturnsFailure()
    {
        var db = TestAuthDbContext.Create();
        var user = SeedUser(db);
        SeedPasswordResetToken(db, user.Id, "expired-reset", Now.AddHours(-2));

        var sut = Build(db);
        var result = await sut.ResetPasswordAsync(new ResetPasswordRequestDto
        {
            Token = "expired-reset",
            NewPassword = "NewPass123!"
        });

        result.Success.Should().BeFalse();
    }

    #endregion

    #region U-A7 — email verification idempotency

    [Fact]
    public async Task VerifyEmailAsync_RepeatVerification_ReturnsSuccessForVerifiedAccount()
    {
        var db = TestAuthDbContext.Create();
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "v@example.com",
            IsEmailVerified = true,
            CreatedAt = Now
        };
        db.Users.Add(user);
        SeedEmailVerificationToken(db, user.Id, "used-verify", Now.AddHours(12), isUsed: true);

        var sut = Build(db);
        var result = await sut.VerifyEmailAsync(new VerifyEmailRequestDto { Token = "used-verify" });

        result.Success.Should().BeTrue();
        result.Message.Should().Be("Email has already been verified.");
    }

    [Fact]
    public async Task VerifyEmailAsync_Once_MarksUserVerified_AndTokenUsed()
    {
        var db = TestAuthDbContext.Create();
        var user = SeedUser(db);
        user.IsEmailVerified = false;
        db.SaveChanges();
        SeedEmailVerificationToken(db, user.Id, "verify-1", Now.AddHours(12));

        var sut = Build(db);
        var result = await sut.VerifyEmailAsync(new VerifyEmailRequestDto { Token = "verify-1" });

        result.Success.Should().BeTrue();
        db.Users.Single(u => u.Id == user.Id).IsEmailVerified.Should().BeTrue();
    }

    #endregion

    #region U-A8 — registration

    [Fact]
    public async Task RegisterAsync_NormalizesEmail_AndPublishesEvent()
    {
        var db = TestAuthDbContext.Create();
        var jwt = Substitute.For<IJwtTokenService>();
        jwt.GenerateAccessToken(Arg.Any<User>()).Returns(("access", Now.AddMinutes(30)));
        jwt.GenerateRefreshToken().Returns("refresh");

        var hasher = Substitute.For<IPasswordHasher>();
        hasher.HashPassword(Arg.Any<string>()).Returns("hashed");

        var publish = Substitute.For<IPublishEndpoint>();

        var sut = Build(db, hasher: hasher, jwt: jwt, publish: publish);
        var result = await sut.RegisterAsync(new RegisterRequestDto
        {
            Email = "  User@Example.COM ",
            Password = "Password123!",
            FullName = "New User"
        });

        result.Email.Should().Be("user@example.com");
        var stored = db.Users.Single();
        stored.Email.Should().Be("user@example.com");
        stored.PasswordHash.Should().Be("hashed");
        stored.Role.Should().Be(UserRole.Student);

        var published = publish.ReceivedCalls()
            .SelectMany(c => c.GetArguments())
            .OfType<UserRegisteredEvent>()
            .First();
        published.Should().NotBeNull();
        published.Email.Should().Be("user@example.com");
    }

    [Fact]
    public async Task RegisterAsync_DuplicateEmail_ThrowsUserAlreadyExists()
    {
        var db = TestAuthDbContext.Create();
        SeedUser(db);

        var sut = Build(db);
        var act = () => sut.RegisterAsync(new RegisterRequestDto
        {
            Email = "user@example.com",
            Password = "Password123!",
            FullName = "x"
        });

        await act.Should().ThrowAsync<UserAlreadyExistsException>();
    }

    #endregion

    #region sealed helpers

    private static class Valid
    {
        public static IValidator<RegisterRequestDto> RegisterValidator() => new ValidatorProxy<RegisterRequestDto>();
        public static IValidator<LoginRequestDto> LoginValidator() => new ValidatorProxy<LoginRequestDto>();
        public static IValidator<VerifyEmailRequestDto> VerifyEmailValidator() => new ValidatorProxy<VerifyEmailRequestDto>();
        public static IValidator<ResendVerificationEmailRequestDto> ResendValidator() => new ValidatorProxy<ResendVerificationEmailRequestDto>();
        public static IValidator<ForgotPasswordRequestDto> ForgotValidator() => new ValidatorProxy<ForgotPasswordRequestDto>();
        public static IValidator<ResetPasswordRequestDto> ResetValidator() => new ValidatorProxy<ResetPasswordRequestDto>();

        private sealed class ValidatorProxy<T> : AbstractValidator<T>
        {
        }
    }

    #endregion
}