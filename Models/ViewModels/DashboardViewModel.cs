using Minecraft.Models;

namespace Minecraft.Models.ViewModels
{
    public class DashboardViewModel
    {
        public List<Player> Players { get; set; } = new();
        public List<GameMode> GameModes { get; set; } = new();
        public List<Item> Items { get; set; } = new();
        public List<Vehicle> Vehicles { get; set; } = new();
        public List<Quest> Quests { get; set; } = new();
        public List<Monster> Monsters { get; set; } = new();
        public List<Purchase> Purchases { get; set; } = new();
        public List<PlayerQuest> PlayerQuests { get; set; } = new();
        public List<MonsterKill> MonsterKills { get; set; } = new();
    }
}

