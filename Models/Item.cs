using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Minecraft.Models
{
    public enum ItemType
    {
        Clothing = 1,
        Weapon = 2,
        Tool = 3,
        Special = 4
    }

    public class Item
    {
        [Key]
        public int ItemId { get; set; }

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
        public ItemType Type { get; set; }

        // Navigation Property
        public virtual ICollection<Purchase> Purchases { get; set; } = new List<Purchase>();
    }
}

