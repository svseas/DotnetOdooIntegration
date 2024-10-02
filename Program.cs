using System;
using OdooIntegration.Repositories;
using OdooIntegration.Models;

class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("Starting Odoo Integration...");

        var odooRepo = new OdooRepository("your_odoo_url", "your_db", "your_username", "your_password");

        try
        {
            // Example: Create a new custom model
            var newModel = new CustomModel
            {
                Name = "Test Model",
                Description = "This is a test model"
            };

            var result = await odooRepo.CreateCustomModel(newModel);
            Console.WriteLine($"Created new model with ID: {result}");

            // Add more integration code here...
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred: {ex.Message}");
        }

        Console.WriteLine("Odoo Integration completed.");
    }
}