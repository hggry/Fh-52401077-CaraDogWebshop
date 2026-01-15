using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace CaraDog.Db;

public sealed class CaraDogDbContextFactory : IDesignTimeDbContextFactory<CaraDogDbContext>
{
    public CaraDogDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<CaraDogDbContext>();
        var connectionString =
            Environment.GetEnvironmentVariable("CARADOG_CONNECTION") ??
            "server=localhost;port=3306;database=caradog;user=app;password=apppw;";

        optionsBuilder.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));

        return new CaraDogDbContext(optionsBuilder.Options);
    }
}
