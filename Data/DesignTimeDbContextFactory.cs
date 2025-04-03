using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using DotNetEnv;
using System.IO;

namespace SmartScheduledApi.DataContext;

public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<SmartScheduleApiContext>
{
    public SmartScheduleApiContext CreateDbContext(string[] args)
    {
        Env.Load();

        var builder = new DbContextOptionsBuilder<SmartScheduleApiContext>();
        var connectionString = Environment.GetEnvironmentVariable("DB_CONNECTION_STRING");

        builder.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));

        return new SmartScheduleApiContext(builder.Options);
    }
}
