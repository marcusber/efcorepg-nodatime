using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using PackagRefs;

var defaultBuilder = new NpgsqlConnectionStringBuilder
{
    Host = "localhost",
    Database = "packagrefs",
    Username = "postgres",
    Password = "postgres",
    ApplicationName = "PackagRefs"
};

await using var provider = new ServiceCollection()
    .AddDbContext<MyDbContext>(options =>
    {
        options.UseNpgsql(defaultBuilder.ToString(), npgOptions =>
        {
            // Removing this makes doesn't make the program throw.
            npgOptions.UseNodaTime();
        });
        
    },contextLifetime: ServiceLifetime.Transient)
    .BuildServiceProvider();

await using (var db = provider.GetRequiredService<MyDbContext>())
{
    await db.Database.MigrateAsync();
}

var otherBuilder = new NpgsqlConnectionStringBuilder(defaultBuilder.ToString())
{
    Database = "postgres",
    ApplicationName = "debug"
};

await using(var connection = new NpgsqlConnection(otherBuilder.ToString()))
{
    await connection.OpenAsync();
    await using var command = connection.CreateCommand();
    command.CommandText = $"SELECT pid, application_name FROM pg_stat_activity WHERE datname = '{defaultBuilder.Database}'";
    await using (var reader = await command.ExecuteReaderAsync())
    {
        Console.WriteLine($"Listing connections towards {defaultBuilder.Database}.");
        while (await reader.ReadAsync())
        {
            Console.WriteLine(reader.GetInt32(0) + " - " + reader.GetString(1));
        }
    }

    Console.WriteLine($"Clearing all pools");
    NpgsqlConnection.ClearAllPools();

    int rows = 0;
    await using (var reader = await command.ExecuteReaderAsync())
    {
        Console.WriteLine($"Listing connections towards {defaultBuilder.Database}.");
        while (await reader.ReadAsync())
        {
            Console.WriteLine(reader.GetInt32(0) + " - " + reader.GetString(1));
            rows++;
        }
    }

    if (rows > 0)
    {
        throw new Exception("Failed");
    }
}
