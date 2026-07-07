using Microsoft.EntityFrameworkCore;
using LegendaryItemsEngine.Data.Models;

namespace LegendaryItemsEngine.Data;

public class LegendaryItemsDbContext : DbContext
{
    // Constructor obligatorio para que la API le pase la configuración de la conexión (Docker)
    public LegendaryItemsDbContext(DbContextOptions<LegendaryItemsDbContext> options) : base(options) {}

    //Declaramos las tablas que EF Core va a crear en la base de datos
    //We need to tell our DbContext what C# classes we are tracking as Entities
    public DbSet<Player> Players { get; set; }
    public DbSet<Item> Items { get; set; }
    public DbSet<ItemInventory> ItemInventories { get; set; }
    public DbSet<ItemOrder> ItemOrders { get; set; }
    public DbSet<OrderLine> OrderLines { get; set; }
    public DbSet<FulfillmentEvent> FulfillmentEvents { get; set; }

    //OnModelCreating() Override method we inherited from DbContext 
    //OnModelCreating() - this is called when EF Core creates a migration
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // --- REGLAS PARA PLAYER ---
        modelBuilder.Entity<Player>(entityPlayer =>
        {
            // El correo debe ser ÚNICO
            entityPlayer.HasIndex(player => player.Email).IsUnique();

            //In the examples Jonathan uses "p" but it means "parameter", just to be more describing im
            //using full words like "player" in my case, not just the initial.
            //Jonathan Example would be: e.HasIndex(p => p.Sku).IsUnique();
            //I use "entityPlayer" too, not just "e"
        });

        // --- REGLAS PARA ITEM ---
        modelBuilder.Entity<Item>(entityItem =>
        {
            // SKU ÚNICO
            entityItem.HasIndex(item => item.SKU).IsUnique(); 

            // Configuramos el precio con el decimal requerido (10 enteros, 2 decimales)
            entityItem.Property(item => item.Price).HasColumnType("decimal(10,2)");
        });

        // --- REGLAS PARA ITEMINVENTORY (Relación 1:1) ---
        modelBuilder.Entity<ItemInventory>(entityInventory =>
        {
            // Configuramos la relación 1 a 1: Un Item tiene un Inventory, un Inventory pertenece a un Item
            // Setting the relationship 1:1 
            entityInventory.HasOne(inv => inv.Item)
                  .WithOne(item => item.Inventory)
                  .HasForeignKey<ItemInventory>(inv => inv.ItemId)
                  .OnDelete(DeleteBehavior.Cascade); // Si se borra un ítem, se borra su inventario automáticamente

            // Row version to check ItemInventory
            entityInventory.Property(inv => inv.RowVersion).IsRowVersion();
        });

        // --- REGLAS PARA ItemOrder ---
        modelBuilder.Entity<ItemOrder>(entityOrder =>
        {
            //Indexamos el estatus para revisiones rapidas
            entityOrder.HasIndex(order => order.Status);

            // Relación 1 a Muchos con Player
            // La Orden TIENE UN Jugador, y ese Jugador puede registrar MUCHAS Órdenes.
            entityOrder.HasOne(order => order.Player)
                  .WithMany(playerOrder => playerOrder.Orders)
                  .HasForeignKey(order => order.PlayerId);
        });

        // --- REGLAS PARA OrderLine 1:1 ---
        modelBuilder.Entity<OrderLine>(entityLine =>
        {
            //Conexion con ItemOrders
            entityLine.HasOne(line => line.Order)
                  .WithOne(order => order.Line)
                  .HasForeignKey<OrderLine>(line => line.ItemOrderId);

            //Conexion con Items
            entityLine.HasOne(line => line.Item)
                  .WithMany()
                  .HasForeignKey(line => line.ItemId);
        });

        // --- REGLAS PARA FulfillmentEvent ---
        modelBuilder.Entity<FulfillmentEvent>(entity =>
        {
            // Una orden tiene muchos eventos de auditoría
            entity.HasOne(e => e.Order)
                  .WithMany(o => o.Events)
                  .HasForeignKey(e => e.ItemOrderId);
        });
    }
}