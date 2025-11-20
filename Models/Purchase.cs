using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Minecraft.Models
{
    public class Purchase
    {
        [Key]
        public int PurchaseId { get; set; }

        [Required]
        public int PlayerId { get; set; }

        // Item purchase (nullable)
        public int? ItemId { get; set; }

        // Vehicle purchase (nullable)
        public int? VehicleId { get; set; }

        [Required]
        public DateTime PurchaseDate { get; set; } = DateTime.Now;

        // Navigation Properties
        [ForeignKey("PlayerId")]
        public virtual Player? Player { get; set; }

        [ForeignKey("ItemId")]
        public virtual Item? Item { get; set; }

        [ForeignKey("VehicleId")]
        public virtual Vehicle? Vehicle { get; set; }
    }
}

