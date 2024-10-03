using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OdooIntegration.Services;
using OdooIntegration.Interfaces;
using OdooIntegration.Models;

namespace OdooIntegration
{
    class Program
    {
        static async Task Main(string[] args)
        {
            // Set up dependency injection
            var services = new ServiceCollection();
            ConfigureServices(services);

            using (var serviceProvider = services.BuildServiceProvider())
            {
                var logger = serviceProvider.GetRequiredService<ILogger<Program>>();

                try
                {
                    logger.LogInformation("Starting Odoo Integration...");

                    var odooService = serviceProvider.GetRequiredService<IOdooService>();

                    bool isAuthenticated = await odooService.Authenticate();
                    if (isAuthenticated)
                    {
                        logger.LogInformation("Successfully authenticated with Odoo server.");

                        // Test connection
                        bool isConnected = await odooService.TestConnection();
                        if (isConnected)
                        {
                            logger.LogInformation("Successfully connected to Odoo server.");

                            try
                            {
                                // Get user info
                                var userInfo = await odooService.GetUserInfo();
                                string userName = userInfo.ContainsKey("name") ? userInfo["name"]?.ToString() ?? "N/A" : "N/A";
                                string userLogin = userInfo.ContainsKey("login") ? userInfo["login"]?.ToString() ?? "N/A" : "N/A";
                                logger.LogInformation($"Logged in as: {userName} (Login: {userLogin})");
                            }
                            catch (Exception ex)
                            {
                                logger.LogError(ex, "Error getting user info");
                            }

                            try
                            {
                                var partners = await odooService.SearchRead("res.partner", new object[] { }, new[] { "name", "email" }, 5);
                                logger.LogInformation($"Found {partners.Count} partners:");
                                foreach (var partner in partners)
                                {
                                    string name = partner.ContainsKey("name") ? partner["name"]?.ToString() ?? "N/A" : "N/A";
                                    string email = partner.ContainsKey("email") ? partner["email"]?.ToString() ?? "N/A" : "N/A";
                                    logger.LogInformation($"- Name: {name}, Email: {email}");
                                }
                            }
                            catch (Exception ex)
                            {
                                logger.LogError(ex, "Error searching partners");
                            }
                        }
                        else
                        {
                            logger.LogWarning("Failed to connect to Odoo server.");
                        }
                    }
                    else
                    {
                        logger.LogWarning("Failed to authenticate with Odoo server.");
                    }

                    logger.LogInformation("Odoo Integration completed.");
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "An error occurred during Odoo Integration");
                }
            }
        }

        private static void ConfigureServices(IServiceCollection services)
        {
            services.AddLogging(configure => configure.AddConsole())
                    .AddSingleton<OdooConnectionInfo>(sp => new OdooConnectionInfo
                    {
                        Url = "http://localhost:8059",
                        Database = "odoo",
                        Username = "admin",
                        Password = "admin"
                    })
                    .AddSingleton<IOdooService, OdooService>()
                    .AddSingleton<CustomXmlRpcClient>();
        }
    }
}