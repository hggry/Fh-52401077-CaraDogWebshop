using CaraDog.Db;
using Microsoft.EntityFrameworkCore;

namespace CaraDog.Integrationtests.TestUtilities;

internal static class DbContextFactory
{
    public static CaraDogDbContext Create(string databaseName)
    {
        var options = new DbContextOptionsBuilder<CaraDogDbContext>()
            .UseInMemoryDatabase(databaseName)
            .EnableSensitiveDataLogging()
            .Options;

        return new CaraDogDbContext(options);
    }
}
