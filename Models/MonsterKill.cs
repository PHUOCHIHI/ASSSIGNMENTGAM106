using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Minecraft.Models
{
    public class MonsterKill
    {
        [Key]
        public int MonsterKillId { get; set; }

        [Required]
        public int PlayerId { get; set; }

        [Required]
        public int MonsterId { get; set; }

        [Required]
        public DateTime KillDate { get; set; } = DateTime.Now;

        // Navigation Properties
        [ForeignKey("PlayerId")]
        public virtual Player? Player { get; set; }

        [ForeignKey("MonsterId")]
        public virtual Monster? Monster { get; set; }
    }
}

