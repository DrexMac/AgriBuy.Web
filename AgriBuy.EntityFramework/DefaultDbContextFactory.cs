using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;

namespace AgriBuy.EntityFramework
{
    public class DefaultDbContextFactory
        : IDesignTimeDbContextFactory<DefaultDbContext>
    {
        public DefaultDbContext CreateDbContext(string[] args)
        {
            var currentDirectory = Directory.GetCurrentDirectory();

            string[] possiblePaths =
            {
                currentDirectory,
                Path.Combine(currentDirectory, "AgriBuy.Web"),
                Path.GetFullPath(
                    Path.Combine(currentDirectory, "..", "AgriBuy.Web"))
            };

            string? configPath = null;

            foreach (var path in possiblePaths)
            {
                if (File.Exists(Path.Combine(path, "appsettings.json")))
                {
                    configPath = path;
                    break;
                }
            }

            if (configPath == null)
            {
                throw new DirectoryNotFoundException(
                    "Could not find AgriBuy.Web/appsettings.json.");
            }

            var builder = new ConfigurationBuilder()
                .SetBasePath(configPath)
                .AddJsonFile("appsettings.json", optional: false);

            var secretsPath = Path.Combine(
                Environment.GetFolderPath(
                    Environment.SpecialFolder.ApplicationData),
                "Microsoft",
                "UserSecrets",
                "5f96dab8-80f4-4925-84d3-524122eec7f1",
                "secrets.json");

            if (File.Exists(secretsPath))
            {
                builder.AddJsonFile(secretsPath, optional: true);
            }

            builder.AddEnvironmentVariables();

            var configuration = builder.Build();

            var connectionString =
                configuration.GetConnectionString("DefaultConnectionString");

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new InvalidOperationException(
                    "Connection string 'DefaultConnectionString' was not found.");
            }

            var optionsBuilder =
                new DbContextOptionsBuilder<DefaultDbContext>();

            optionsBuilder.UseMySql(
                connectionString,
                ServerVersion.AutoDetect(connectionString),
                mySqlOptions =>
                {
                    mySqlOptions.MigrationsAssembly(
                        typeof(DefaultDbContext).Assembly.FullName);
                });

            return new DefaultDbContext(optionsBuilder.Options);
        }
    }
}
