﻿using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace Discount.Grpc.Extensions
{
    public static class HostExtensions
    {
        public static IHost MigrateDatabase<TContext>(this IHost host, int? retry = 0)
        {
            int retryForAvailability = retry.Value;

            using (var scope = host.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                var configuration = services.GetRequiredService<IConfiguration>();
                var logger = services.GetRequiredService<ILogger<TContext>>();

                try
                {
                    logger.LogInformation("Migrationg postgresql database.");

                    using var connection = new NpgsqlConnection(configuration.GetValue<string>("DatabaseSettings:ConnectionString"));
                    
                    connection.Open();

                    using var command = new NpgsqlCommand
                    {
                        Connection = connection
                    };

                    command.CommandText = "DROP TABLE IF EXISTS Coupon";
                    command.ExecuteNonQuery();

                    command.CommandText = @"CREATE TABLE Coupon(
	                                            ID SERIAL PRIMARY KEY NOT NULL,
	                                            ProductName VARCHAR(24) NOT NULL,
	                                            Description TEXT,
	                                            Amount INT)";
                    
                    command.ExecuteNonQuery();

                    command.CommandText = "INSERT INTO Coupon(Productname, Description, Amount) VALUES('IPhone X', 'IPhone Discount', 150)";
                    command.ExecuteNonQuery();

                    command.CommandText = "INSERT INTO Coupon(Productname, Description, Amount) VALUES('Samsung 10', 'Samsung Discount', 100)";
                    command.ExecuteNonQuery();

                    logger.LogInformation("Migrated postgresql database.");
                }
                catch (NpgsqlException ex)
                {
                    logger.LogError(ex, "An error occurred while migrationg the postgresql database.");

                    if (retryForAvailability < 50)
                    {
                        retryForAvailability++;
                        System.Threading.Thread.Sleep(2000);
                        MigrateDatabase<TContext>(host, retryForAvailability);
                    }
                }

                return host;
            }
        }
    }
}
