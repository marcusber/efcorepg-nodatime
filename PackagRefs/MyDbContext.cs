using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace PackagRefs;

public class MyDbContext(DbContextOptions<MyDbContext> options) : DbContext(options)
{
    public DbSet<Test> Tests { get; set; } = null!;
}

public class Test
{
    public int Id { get; set; }

    public required string Name { get; set; }
}

public class Context1DesignTimeFactory : IDesignTimeDbContextFactory<MyDbContext>
{
    public MyDbContext CreateDbContext(string[] args)
    {
        // Note that you actually don't need a proper connection string
        // just to add migrations

        var options = new DbContextOptionsBuilder<MyDbContext>()
            //.UseSnakeCaseNamingConvention()
            .UseNpgsql("Host=;",
                builder =>
                {
                    //builder.UseCrossDbFunctionality();
                    //builder.MigrationsAssembly(typeof(Context1DesignTimeFactory).Assembly.FullName);
                })
            .Options;

        return new MyDbContext(options);
    }
}
