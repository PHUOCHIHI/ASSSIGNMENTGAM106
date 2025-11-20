using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Minecraft.Models
{
    public class Monster
    {
        [Key]
        public int MonsterId { get; set; }

        [Required]
        [StringLength(100)]
        [Column(TypeName = "nvarchar(100)")]
        public string Name { get; set; } = string.Empty;

        [Required]
        [Range(1, int.MaxValue)]
        public int Health { get; set; }

        [Required]
        [Range(0, int.MaxValue)]
        public int Reward { get; set; }

        // Navigation Property
        public virtual ICollection<MonsterKill> MonsterKills { get; set; } = new List<MonsterKill>();
    }
}

