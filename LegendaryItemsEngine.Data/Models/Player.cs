using System.ComponentModel.DataAnnotations;

namespace LegendaryItemsEngine.Data.Models;

public class Player
{
    public int Id {get; set;}

    [Required]
    [MaxLength(100)]
    public string Name {get; set;} = default!;

    [Required]
    [MaxLength(150)]
    public string Email {get; set;} = string.Empty; //Same aa using " = default! " its an empty string
                                                    //read-only

    // Propiedad de navegación: Un jugador puede tener muchas ordenes
    public ICollection<ItemOrder> Orders { get; set; } = new List<ItemOrder>();
    //Other way to do it (instructor)
    //public List<ItemOrder> Orders { get; set; } = new();

    //ICollection use the necesary to run in EF Core, Count(), Add(), Remove().
    //  in theory thats all we're gonna need to manage data and this relation, abstraction...
    //IEnumerable - have less methods to manage the data, basically just reading
    //List - More methods of control than ICollection:
    //  Lines[0], .Find(), .BinarySearch(), .Sort(), etc.

}