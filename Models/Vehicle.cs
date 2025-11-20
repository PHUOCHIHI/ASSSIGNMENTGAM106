using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Minecraft.Models
{
    public enum VehicleType
    {
        Horse = 1,
        Boat = 2,
        Minecart = 3
    }

    public class Vehicle
    {
        [Key]
        public int VehicleId { get; set; }

        [Required]
        [StringLength(100)]
        [Column(TypeName = "nvarchar(100)")]
        public string Name { get; set; } = string.Empty;

        [StringLength(500)]
        [Column(TypeName = "nvarchar(500)")]
        public string? Image { get; set; }

        [Required]
        [Range(0, int.MaxValue)]
        public int Value { get; set; }

        [Required]
        public VehicleType Type { get; set; }

        // Navigation Property
        public virtual ICollection<Purchase> Purchases { get; set; } = new List<Purchase>();
    }
}

