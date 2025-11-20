using Microsoft.EntityFrameworkCore;
using Minecraft.Data;
using Minecraft.Models;
using System.IO;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Configure SQLite Connection - Tự động tạo file .db
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(connectionString));

var app = builder.Build();

// Tự động tạo database và seed dữ liệu nếu chưa tồn tại
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        var logger = services.GetRequiredService<ILogger<Program>>();
        
        logger.LogInformation("Đang kiểm tra database...");
        
        // Tự động tạo database nếu chưa có
        var dbCreated = context.Database.EnsureCreated();
        
        if (dbCreated)
        {
            logger.LogInformation("Database đã được tạo thành công!");
        }
        else
        {
            logger.LogInformation("Database đã tồn tại.");
        }
        
        // Seed dữ liệu nếu chưa có
        if (!context.GameModes.Any())
        {
            logger.LogInformation("Đang seed dữ liệu mẫu...");
            SeedData(context);
            logger.LogInformation("Đã seed dữ liệu mẫu thành công!");
        }
        else
        {
            logger.LogInformation("Dữ liệu đã tồn tại, bỏ qua seed.");
        }
        
        // Hiển thị đường dẫn file database
        var config = services.GetRequiredService<IConfiguration>();
        var dbConnectionString = config.GetConnectionString("DefaultConnection");
        var dbPath = dbConnectionString?.Replace("Data Source=", "").Trim() ?? "MinecraftDB.db";
        var fullPath = Path.GetFullPath(dbPath);
        logger.LogInformation("Database file location: {FullPath}", fullPath);
        
        if (File.Exists(fullPath))
        {
            var fileInfo = new FileInfo(fullPath);
            logger.LogInformation("Database file size: {Size} bytes", fileInfo.Length);
        }
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Lỗi khi tạo database: {Message}", ex.Message);
    }
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

// Hàm seed dữ liệu mẫu
static void SeedData(ApplicationDbContext context)
{
    // Seed GameModes
    if (!context.GameModes.Any())
    {
        context.GameModes.AddRange(
            new GameMode { Name = "Survival", Description = "Chế độ sinh tồn - người chơi phải thu thập tài nguyên và bảo vệ bản thân" },
            new GameMode { Name = "Creative", Description = "Chế độ sáng tạo - người chơi có thể xây dựng tự do không giới hạn" },
            new GameMode { Name = "Adventure", Description = "Chế độ phiêu lưu - người chơi khám phá các bản đồ và hoàn thành nhiệm vụ" },
            new GameMode { Name = "Spectator", Description = "Chế độ khán giả - người chơi có thể quan sát thế giới mà không tương tác" },
            new GameMode { Name = "Hardcore", Description = "Chế độ khó - giống Survival nhưng chỉ có một mạng sống" }
        );
        context.SaveChanges();
    }

    // Seed Items
    if (!context.Items.Any())
    {
        context.Items.AddRange(
            new Item { Name = "Diamond Sword", Image = "/images/diamond_sword.png", Value = 500, Type = ItemType.Weapon },
            new Item { Name = "Iron Pickaxe", Image = "/images/iron_pickaxe.png", Value = 300, Type = ItemType.Tool },
            new Item { Name = "Leather Armor", Image = "/images/leather_armor.png", Value = 200, Type = ItemType.Clothing },
            new Item { Name = "Golden Apple", Image = "/images/golden_apple.png", Value = 100, Type = ItemType.Special },
            new Item { Name = "Enchanted Bow", Image = "/images/enchanted_bow.png", Value = 400, Type = ItemType.Weapon },
            new Item { Name = "Diamond Helmet", Image = "/images/diamond_helmet.png", Value = 450, Type = ItemType.Clothing },
            new Item { Name = "Netherite Axe", Image = "/images/netherite_axe.png", Value = 600, Type = ItemType.Tool }
        );
        context.SaveChanges();
    }

    // Seed Vehicles
    if (!context.Vehicles.Any())
    {
        context.Vehicles.AddRange(
            new Vehicle { Name = "White Horse", Image = "/images/white_horse.png", Value = 250, Type = VehicleType.Horse },
            new Vehicle { Name = "Brown Horse", Image = "/images/brown_horse.png", Value = 250, Type = VehicleType.Horse },
            new Vehicle { Name = "Oak Boat", Image = "/images/oak_boat.png", Value = 150, Type = VehicleType.Boat },
            new Vehicle { Name = "Birch Boat", Image = "/images/birch_boat.png", Value = 150, Type = VehicleType.Boat },
            new Vehicle { Name = "Minecart", Image = "/images/minecart.png", Value = 200, Type = VehicleType.Minecart },
            new Vehicle { Name = "Chest Minecart", Image = "/images/chest_minecart.png", Value = 300, Type = VehicleType.Minecart },
            new Vehicle { Name = "Furnace Minecart", Image = "/images/furnace_minecart.png", Value = 350, Type = VehicleType.Minecart }
        );
        context.SaveChanges();
    }

    // Seed Quests
    if (!context.Quests.Any())
    {
        context.Quests.AddRange(
            new Quest { Name = "First Steps", Description = "Thu thập 10 khối gỗ đầu tiên", Reward = 50 },
            new Quest { Name = "Mining Master", Description = "Khai thác 50 khối đá", Reward = 100 },
            new Quest { Name = "Monster Hunter", Description = "Tiêu diệt 5 quái vật", Reward = 150 },
            new Quest { Name = "Builder", Description = "Xây dựng một ngôi nhà", Reward = 200 },
            new Quest { Name = "Explorer", Description = "Khám phá 3 vùng đất mới", Reward = 250 },
            new Quest { Name = "Craft Master", Description = "Chế tạo 10 công cụ khác nhau", Reward = 300 },
            new Quest { Name = "Dungeon Raider", Description = "Hoàn thành một dungeon", Reward = 400 }
        );
        context.SaveChanges();
    }

    // Seed Monsters
    if (!context.Monsters.Any())
    {
        context.Monsters.AddRange(
            new Monster { Name = "Zombie", Health = 20, Reward = 10 },
            new Monster { Name = "Skeleton", Health = 20, Reward = 15 },
            new Monster { Name = "Creeper", Health = 20, Reward = 20 },
            new Monster { Name = "Spider", Health = 16, Reward = 12 },
            new Monster { Name = "Enderman", Health = 40, Reward = 50 },
            new Monster { Name = "Witch", Health = 26, Reward = 30 },
            new Monster { Name = "Ender Dragon", Health = 200, Reward = 1000 }
        );
        context.SaveChanges();
    }

    // Seed Players
    if (!context.Players.Any())
    {
        var gameMode1 = context.GameModes.First(g => g.Name == "Survival");
        var gameMode2 = context.GameModes.First(g => g.Name == "Creative");
        var gameMode3 = context.GameModes.First(g => g.Name == "Adventure");
        var gameMode4 = context.GameModes.First(g => g.Name == "Spectator");

        context.Players.AddRange(
            new Player { PlayerCode = "PLAYER001", Email = "player1@minecraft.com", Password = "password123", Health = 100, Food = 100, ExperiencePoints = 500, GameModeId = gameMode1.GameModeId },
            new Player { PlayerCode = "PLAYER002", Email = "player2@minecraft.com", Password = "password123", Health = 85, Food = 90, ExperiencePoints = 750, GameModeId = gameMode2.GameModeId },
            new Player { PlayerCode = "PLAYER003", Email = "player3@minecraft.com", Password = "password123", Health = 100, Food = 100, ExperiencePoints = 300, GameModeId = gameMode1.GameModeId },
            new Player { PlayerCode = "PLAYER004", Email = "player4@minecraft.com", Password = "password123", Health = 70, Food = 80, ExperiencePoints = 1200, GameModeId = gameMode3.GameModeId },
            new Player { PlayerCode = "PLAYER005", Email = "player5@minecraft.com", Password = "password123", Health = 100, Food = 100, ExperiencePoints = 200, GameModeId = gameMode4.GameModeId },
            new Player { PlayerCode = "PLAYER006", Email = "player6@minecraft.com", Password = "password123", Health = 95, Food = 95, ExperiencePoints = 600, GameModeId = gameMode1.GameModeId },
            new Player { PlayerCode = "PLAYER007", Email = "player7@minecraft.com", Password = "password123", Health = 100, Food = 100, ExperiencePoints = 150, GameModeId = gameMode2.GameModeId }
        );
        context.SaveChanges();
    }

    // Seed Purchases
    if (!context.Purchases.Any())
    {
        var player1 = context.Players.First(p => p.PlayerCode == "PLAYER001");
        var player2 = context.Players.First(p => p.PlayerCode == "PLAYER002");
        var player3 = context.Players.First(p => p.PlayerCode == "PLAYER003");
        var player4 = context.Players.First(p => p.PlayerCode == "PLAYER004");
        var player5 = context.Players.First(p => p.PlayerCode == "PLAYER005");
        var player6 = context.Players.First(p => p.PlayerCode == "PLAYER006");

        var item1 = context.Items.First(i => i.Name == "Diamond Sword");
        var item2 = context.Items.First(i => i.Name == "Iron Pickaxe");
        var item3 = context.Items.First(i => i.Name == "Leather Armor");
        var item4 = context.Items.First(i => i.Name == "Golden Apple");

        var vehicle1 = context.Vehicles.First(v => v.Name == "White Horse");
        var vehicle3 = context.Vehicles.First(v => v.Name == "Oak Boat");
        var vehicle5 = context.Vehicles.First(v => v.Name == "Minecart");

        context.Purchases.AddRange(
            new Purchase { PlayerId = player1.PlayerId, ItemId = item1.ItemId, VehicleId = null, PurchaseDate = DateTime.Now },
            new Purchase { PlayerId = player1.PlayerId, ItemId = null, VehicleId = vehicle1.VehicleId, PurchaseDate = DateTime.Now.AddDays(-1) },
            new Purchase { PlayerId = player2.PlayerId, ItemId = item2.ItemId, VehicleId = null, PurchaseDate = DateTime.Now.AddDays(-2) },
            new Purchase { PlayerId = player3.PlayerId, ItemId = null, VehicleId = vehicle3.VehicleId, PurchaseDate = DateTime.Now.AddDays(-3) },
            new Purchase { PlayerId = player4.PlayerId, ItemId = item3.ItemId, VehicleId = null, PurchaseDate = DateTime.Now.AddDays(-1) },
            new Purchase { PlayerId = player5.PlayerId, ItemId = null, VehicleId = vehicle5.VehicleId, PurchaseDate = DateTime.Now },
            new Purchase { PlayerId = player6.PlayerId, ItemId = item4.ItemId, VehicleId = null, PurchaseDate = DateTime.Now.AddDays(-5) }
        );
        context.SaveChanges();
    }

    // Seed PlayerQuests
    if (!context.PlayerQuests.Any())
    {
        var player1 = context.Players.First(p => p.PlayerCode == "PLAYER001");
        var player2 = context.Players.First(p => p.PlayerCode == "PLAYER002");
        var player3 = context.Players.First(p => p.PlayerCode == "PLAYER003");
        var player4 = context.Players.First(p => p.PlayerCode == "PLAYER004");
        var player5 = context.Players.First(p => p.PlayerCode == "PLAYER005");
        var player6 = context.Players.First(p => p.PlayerCode == "PLAYER006");

        var quest1 = context.Quests.First(q => q.Name == "First Steps");
        var quest2 = context.Quests.First(q => q.Name == "Mining Master");
        var quest3 = context.Quests.First(q => q.Name == "Monster Hunter");
        var quest4 = context.Quests.First(q => q.Name == "Builder");
        var quest5 = context.Quests.First(q => q.Name == "Explorer");

        context.PlayerQuests.AddRange(
            new PlayerQuest { PlayerId = player1.PlayerId, QuestId = quest1.QuestId, Status = QuestStatus.Completed, CompletedDate = DateTime.Now.AddDays(-5) },
            new PlayerQuest { PlayerId = player1.PlayerId, QuestId = quest2.QuestId, Status = QuestStatus.Completed, CompletedDate = DateTime.Now.AddDays(-3) },
            new PlayerQuest { PlayerId = player2.PlayerId, QuestId = quest3.QuestId, Status = QuestStatus.Completed, CompletedDate = DateTime.Now.AddDays(-2) },
            new PlayerQuest { PlayerId = player3.PlayerId, QuestId = quest1.QuestId, Status = QuestStatus.InProgress, CompletedDate = null },
            new PlayerQuest { PlayerId = player4.PlayerId, QuestId = quest4.QuestId, Status = QuestStatus.Completed, CompletedDate = DateTime.Now.AddDays(-1) },
            new PlayerQuest { PlayerId = player5.PlayerId, QuestId = quest5.QuestId, Status = QuestStatus.InProgress, CompletedDate = null },
            new PlayerQuest { PlayerId = player6.PlayerId, QuestId = quest2.QuestId, Status = QuestStatus.Completed, CompletedDate = DateTime.Now.AddDays(-4) }
        );
        context.SaveChanges();
    }

    // Seed MonsterKills
    if (!context.MonsterKills.Any())
    {
        var player1 = context.Players.First(p => p.PlayerCode == "PLAYER001");
        var player2 = context.Players.First(p => p.PlayerCode == "PLAYER002");
        var player3 = context.Players.First(p => p.PlayerCode == "PLAYER003");
        var player4 = context.Players.First(p => p.PlayerCode == "PLAYER004");
        var player5 = context.Players.First(p => p.PlayerCode == "PLAYER005");
        var player6 = context.Players.First(p => p.PlayerCode == "PLAYER006");

        var monster1 = context.Monsters.First(m => m.Name == "Zombie");
        var monster2 = context.Monsters.First(m => m.Name == "Skeleton");
        var monster3 = context.Monsters.First(m => m.Name == "Creeper");
        var monster4 = context.Monsters.First(m => m.Name == "Spider");
        var monster5 = context.Monsters.First(m => m.Name == "Enderman");

        context.MonsterKills.AddRange(
            new MonsterKill { PlayerId = player1.PlayerId, MonsterId = monster1.MonsterId, KillDate = DateTime.Now.AddDays(-10) },
            new MonsterKill { PlayerId = player1.PlayerId, MonsterId = monster2.MonsterId, KillDate = DateTime.Now.AddDays(-8) },
            new MonsterKill { PlayerId = player2.PlayerId, MonsterId = monster3.MonsterId, KillDate = DateTime.Now.AddDays(-5) },
            new MonsterKill { PlayerId = player3.PlayerId, MonsterId = monster1.MonsterId, KillDate = DateTime.Now.AddDays(-7) },
            new MonsterKill { PlayerId = player4.PlayerId, MonsterId = monster4.MonsterId, KillDate = DateTime.Now.AddDays(-3) },
            new MonsterKill { PlayerId = player5.PlayerId, MonsterId = monster2.MonsterId, KillDate = DateTime.Now.AddDays(-6) },
            new MonsterKill { PlayerId = player6.PlayerId, MonsterId = monster5.MonsterId, KillDate = DateTime.Now.AddDays(-2) }
        );
        context.SaveChanges();
    }
}

