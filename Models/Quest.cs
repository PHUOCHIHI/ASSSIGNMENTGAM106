using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Minecraft.Models
{
    public class Quest
    {
        [Key]
        public int QuestId { get; set; }

        [Required]
        [StringLength(100)]
        [Column(TypeName = "nvarchar(100)")]
        public string Name { get; set; } = string.Empty;

        [StringLength(1000)]
        [Column(TypeName = "nvarchar(1000)")]
        public string? Description { get; set; }

        [Required]
        [Range(0, int.MaxValue)]
        public int Reward { get; set; }

        // Navigation Property
        public virtual ICollection<PlayerQuest> PlayerQuests { get; set; } = new List<PlayerQuest>();
    }
}

