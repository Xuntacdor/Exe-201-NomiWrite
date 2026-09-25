using FluentAssertions;
using FluentValidation;
using NomiWrite.Auth.Application.DTOs;
using NomiWrite.Auth.Application.Exceptions;
using NomiWrite.Auth.Application.Services;
using NomiWrite.Auth.Application.UnitTests.Persistence;
using NomiWrite.Auth.Application.Validation;
using NomiWrite.Auth.Domain.Entities;
using NomiWrite.Auth.Domain.Enums;

namespace NomiWrite.Auth.Application.UnitTests;

public class AdminUserServiceTests
{
    private static AdminUserService Build(TestAuthDbContext db)
        => new(db, new UpdateUserStatusRequestValidator(), new UpdateUserRoleRequestValidator());

    private static User SeedUser(
        TestAuthDbContext db,
        string email,
        string fullName,
        UserRole role = UserRole.Student,
        AccountStatus status = AccountStatus.Active,
        bool isDeleted = false,
        DateTime? createdAt = null)
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = email,
            FullName = fullName,
            PasswordHash = "hash",
            Role = role,
            AccountStatus = status,
            IsEmailVerified = true,
            IsDeleted = isDeleted,
            CreatedAt = createdAt ?? DateTime.UtcNow,
            UpdatedAt = createdAt ?? DateTime.UtcNow
        };
        db.Users.Add(user);
        db.SaveChanges();
        return user;
    }

    [Fact]
    public async Task GetUsersAsync_SearchRoleStatusAndPaging_WorkTogether()
    {
        var db = TestAuthDbContext.Create();
        SeedUser(db, "alice@example.com", "Alice Student", createdAt: DateTime.UtcNow.AddMinutes(-3));
        var moderator = SeedUser(
            db,
            "moderator@example.com",
            "Case Moderator",
            UserRole.Moderator,
            AccountStatus.Active,
            createdAt: DateTime.UtcNow.AddMinutes(-2));
        SeedUser(db, "banned-moderator@example.com", "Case Moderator", UserRole.Moderator, AccountStatus.Banned);

        var sut = Build(db);
        var result = await sut.GetUsersAsync(
            "moderator",
            UserRole.Moderator,
            AccountStatus.Active,
            page: 1,
            pageSize: 10);

        result.TotalCount.Should().Be(1);
        result.Items.Should().ContainSingle(u => u.Id == moderator.Id);
    }

    [Fact]
    public async Task GetUsersAsync_InvalidPaging_IsClamped()
    {
        var db = TestAuthDbContext.Create();
        for (var i = 0; i < 25; i++)
        {
            SeedUser(
                db,
                $"student{i}@example.com",
                $"Student {i}",
                createdAt: DateTime.UtcNow.AddMinutes(i));
        }

        var sut = Build(db);
        var result = await sut.GetUsersAsync(null, null, null, page: 0, pageSize: 500);

        result.Page.Should().Be(1);
        result.PageSize.Should().Be(20);
        result.TotalCount.Should().Be(25);
        result.TotalPages.Should().Be(2);
        result.Items.Should().HaveCount(20);
    }

    [Fact]
    public async Task UpdateUserStatusAsync_Banned_RevokesActiveRefreshTokens()
    {
        var db = TestAuthDbContext.Create();
        var user = SeedUser(db, "student@example.com", "Student One");
        db.RefreshTokens.AddRange(
            new RefreshToken
            {
                UserId = user.Id,
                Token = "active",
                ExpiresAt = DateTime.UtcNow.AddDays(1),
                IsRevoked = false
            },
            new RefreshToken
            {
                UserId = user.Id,
                Token = "already-revoked",
                ExpiresAt = DateTime.UtcNow.AddDays(1),
                IsRevoked = true
            });
        db.SaveChanges();

        var sut = Build(db);
        var result = await sut.UpdateUserStatusAsync(user.Id, new UpdateUserStatusRequestDto
        {
            Status = AccountStatus.Banned
        });

        result.Success.Should().BeTrue();
        db.Users.Single(u => u.Id == user.Id).AccountStatus.Should().Be(AccountStatus.Banned);
        db.RefreshTokens.Where(t => t.UserId == user.Id).Should().OnlyContain(t => t.IsRevoked);
    }

    [Fact]
    public async Task UpdateUserStatusAsync_InvalidStatus_Throws()
    {
        var db = TestAuthDbContext.Create();
        var user = SeedUser(db, "student@example.com", "Student One");

        var sut = Build(db);
        var act = () => sut.UpdateUserStatusAsync(user.Id, new UpdateUserStatusRequestDto
        {
            Status = (AccountStatus)999
        });

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task UpdateUserRoleAsync_SelfRoleChange_Throws()
    {
        var db = TestAuthDbContext.Create();
        var admin = SeedUser(db, "admin@example.com", "Admin One", UserRole.Admin);

        var sut = Build(db);
        var act = () => sut.UpdateUserRoleAsync(admin.Id, admin.Id, new UpdateUserRoleRequestDto
        {
            Role = UserRole.Student
        });

        await act.Should().ThrowAsync<CannotModifyOwnRoleException>();
    }

    [Fact]
    public async Task UpdateUserRoleAsync_ValidRole_UpdatesUser()
    {
        var db = TestAuthDbContext.Create();
        var admin = SeedUser(db, "admin@example.com", "Admin One", UserRole.Admin);
        var student = SeedUser(db, "student@example.com", "Student One");

        var sut = Build(db);
        var result = await sut.UpdateUserRoleAsync(admin.Id, student.Id, new UpdateUserRoleRequestDto
        {
            Role = UserRole.Moderator
        });

        result.Success.Should().BeTrue();
        db.Users.Single(u => u.Id == student.Id).Role.Should().Be(UserRole.Moderator);
    }

    [Fact]
    public async Task GetUserAnalyticsAsync_ExcludesHardDeletedAccounts()
    {
        var db = TestAuthDbContext.Create();
        SeedUser(db, "active@example.com", "Active User");
        SeedUser(db, "banned@example.com", "Banned User", status: AccountStatus.Banned);
        SeedUser(db, "deleted@example.com", "Deleted User", isDeleted: true);

        var sut = Build(db);
        var result = await sut.GetUserAnalyticsAsync();

        result.TotalUsers.Should().Be(2);
        result.ActiveUsers.Should().Be(1);
        result.BannedUsers.Should().Be(1);
    }
}
