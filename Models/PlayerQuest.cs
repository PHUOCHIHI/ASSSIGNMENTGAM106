using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Minecraft.Models
{
    public enum QuestStatus
    {
        InProgress = 1,
        Completed = 2
    }

    public class PlayerQuest
    {
        [Key]
        public int PlayerQuestId { get; set; }

        [Required]
        public int PlayerId { get; set; }

        [Required]
        public int QuestId { get; set; }

        [Required]
        public QuestStatus Status { get; set; } = QuestStatus.InProgress;

        public DateTime? CompletedDate { get; set; }

        // Navigation Properties
        [ForeignKey("PlayerId")]
        public virtual Player? Player { get; set; }

        [ForeignKey("QuestId")]
        public virtual Quest? Quest { get; set; }
    }
}

