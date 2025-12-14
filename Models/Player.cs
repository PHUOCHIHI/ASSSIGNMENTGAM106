using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Minecraft.Models
{
    public class Player
    {
        [Key]
        public int PlayerId { get; set; }

        [Required]
        [StringLength(50)]
        [Column(TypeName = "nvarchar(50)")]
        public string PlayerCode { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        [EmailAddress]
        [Column(TypeName = "nvarchar(100)")]
        public string Email { get; set; } = string.Empty;

        [Required]
        [StringLength(255)]
        [Column(TypeName = "nvarchar(255)")]
        public string Password { get; set; } = string.Empty;

        [Required]
        [Range(0, 100)]
        public int Health { get; set; } = 100;

        [Required]
        [Range(0, 100)]
        public int Food { get; set; } = 100;

        [Required]
        [Range(0, int.MaxValue)]
        public int ExperiencePoints { get; set; } = 0;

        // Foreign Key
        public int GameModeId { get; set; }

        public int? RegionId { get; set; }

        // Navigation Property
        [ForeignKey("GameModeId")]
        public virtual GameMode? GameMode { get; set; }

        [ForeignKey("RegionId")]
        public virtual Region? Region { get; set; }

        // Navigation Properties
        public virtual ICollection<Purchase> Purchases { get; set; } = new List<Purchase>();
        public virtual ICollection<PlayerQuest> PlayerQuests { get; set; } = new List<PlayerQuest>();
        public virtual ICollection<MonsterKill> MonsterKills { get; set; } = new List<MonsterKill>();
    }
}

