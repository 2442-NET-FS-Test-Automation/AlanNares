using LegendaryItemsEngine.Data.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace LegendaryItemsEngine.Data.Seeding;

public static class DbInitializer
{
    public static void SeedData(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();

        var context = scope.ServiceProvider.GetRequiredService<LegendaryItemsDbContext>(); 

        context.Database.Migrate();

        // If no items we add these
        if (!context.Items.Any())
        {
            Log.Information("=== Seed Data: Database is empty. Creating 10 Legendary Items and Inventories... ===");

            // Definimos una lista de 10 ítems RPG con los campos exactos de tu ER (SKU, Name, Price)
            var itemsToSeed = new List<Item>
            {
                new() { SKU = "WEAP-EXCAL-001", Name = "Excalibur", Price = 9999.99m },
                new() { SKU = "WEAP-FROST-002", Name = "Frostmourne", Price = 8500.50m },
                new() { SKU = "WEAP-MURAS-003", Name = "Murasama Blade", Price = 7200.00m },
                new() { SKU = "STAF-REEDM-004", Name = "Staff of Redemption", Price = 4500.25m },
                new() { SKU = "ARM-AEGIS-005", Name = "Aegis Shield", Price = 6100.99m },
                new() { SKU = "RING-CHRST-006", Name = "Chrono Ring", Price = 3200.00m },
                new() { SKU = "POT-ELIXR-007", Name = "Elixir of Immortality", Price = 500.00m },
                new() { SKU = "WEAP-MJOLN-008", Name = "Mjolnir Replica", Price = 9500.00m },
                new() { SKU = "AMUL-DRGNE-009", Name = "Dragon Eye Amulet", Price = 5800.75m },
                new() { SKU = "BOOK-SHADW-010", Name = "Book of Shadows", Price = 4150.00m }
            };

            // add items to the database
            context.Items.AddRange(itemsToSeed);
            
            // we save the changes in our database
            context.SaveChanges(); 

            Log.Information("=== Seed Data: 10 Items created ===");

            // Now we create some fields in our ItemInventories related to the items we created before
            foreach (var item in itemsToSeed)
            {
                context.ItemInventories.Add(new ItemInventory
                {
                    ItemId = item.Id,        // Adding FK from items
                    QuantityOnHand = 100     // Stock
                });
            }

            context.SaveChanges();
            Log.Information("=== Seed Data: ItemInventory now have stock ===");
        }
        else
        {
            Log.Information("=== Seed Data: Database already contains data. By: LegendaryItemsEngine.Data/Seeding/DbInitializer.cs ===");
        }
    }
}