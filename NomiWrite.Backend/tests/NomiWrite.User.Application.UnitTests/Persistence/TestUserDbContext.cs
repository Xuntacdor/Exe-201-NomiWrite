using Microsoft.EntityFrameworkCore;
using NomiWrite.User.Infrastructure.Persistence;

namespace NomiWrite.User.Application.UnitTests.Persistence;

public static class TestUserDbContext
{
    public static UserDbContext Create()
    {
        var options = new DbContextOptionsBuilder<UserDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new UserDbContext(options);
    }
}