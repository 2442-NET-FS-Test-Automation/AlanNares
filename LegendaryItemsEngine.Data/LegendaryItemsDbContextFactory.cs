using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace LegendaryItemsEngine.Data;

//Creamos este LegendaryItemsDbContextFactory para poder lanzar comandos CLI apuntando a la carpeta main (AlanNares)
//Ahora funcionan los siguientes comandos;
//Nueva migración: dotnet ef migrations add InitialCreate --project LegendaryItemsEngine.Data --startup-project LegendaryItemsEngine.Api
//Aplicar migración: dotnet ef database update --project LegendaryItemsEngine.Data --startup-project LegendaryItemsEngine.Api

public class LegendaryItemsDbContextFactory : IDesignTimeDbContextFactory<LegendaryItemsDbContext>
{
    public LegendaryItemsDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<LegendaryItemsDbContext>();
        
        // Colocamos la misma cadena de conexión de Docker para el tiempo de diseño
        var connectionString = "Server=localhost,1433;Database=LegendaryItemsDB;User Id=sa;Password=LegItemsEng@2026!;TrustServerCertificate=True;";
        
        optionsBuilder.UseSqlServer(connectionString);

        return new LegendaryItemsDbContext(optionsBuilder.Options);
    }
}