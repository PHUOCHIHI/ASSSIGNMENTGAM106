-- =============================================
-- Minecraft Database Creation Script
-- SQL Server Database Script
-- =============================================

-- Create Database
IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'MinecraftDB')
BEGIN
    CREATE DATABASE MinecraftDB;
END
GO

USE MinecraftDB;
GO

-- =============================================
-- Create Tables
-- =============================================

-- Table: GameModes
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GameModes]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[GameModes] (
        [GameModeId] INT IDENTITY(1,1) PRIMARY KEY,
        [Name] NVARCHAR(50) NOT NULL UNIQUE,
        [Description] NVARCHAR(500) NULL
    );
END
GO

-- Table: Players
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Players]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[Players] (
        [PlayerId] INT IDENTITY(1,1) PRIMARY KEY,
        [PlayerCode] NVARCHAR(50) NOT NULL UNIQUE,
        [Email] NVARCHAR(100) NOT NULL UNIQUE,
        [Password] NVARCHAR(255) NOT NULL,
        [Health] INT NOT NULL DEFAULT 100,
        [Food] INT NOT NULL DEFAULT 100,
        [ExperiencePoints] INT NOT NULL DEFAULT 0,
        [GameModeId] INT NOT NULL,
        CONSTRAINT [FK_Players_GameModes] FOREIGN KEY ([GameModeId]) REFERENCES [GameModes]([GameModeId]),
        CONSTRAINT [CK_Player_Health] CHECK ([Health] >= 0 AND [Health] <= 100),
        CONSTRAINT [CK_Player_Food] CHECK ([Food] >= 0 AND [Food] <= 100),
        CONSTRAINT [CK_Player_ExperiencePoints] CHECK ([ExperiencePoints] >= 0)
    );
END
GO

-- Table: Items
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Items]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[Items] (
        [ItemId] INT IDENTITY(1,1) PRIMARY KEY,
        [Name] NVARCHAR(100) NOT NULL,
        [Image] NVARCHAR(500) NULL,
        [Value] INT NOT NULL,
        [Type] INT NOT NULL, -- 1: Clothing, 2: Weapon, 3: Tool, 4: Special
        CONSTRAINT [CK_Item_Value] CHECK ([Value] >= 0)
    );
END
GO

-- Table: Vehicles
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Vehicles]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[Vehicles] (
        [VehicleId] INT IDENTITY(1,1) PRIMARY KEY,
        [Name] NVARCHAR(100) NOT NULL,
        [Image] NVARCHAR(500) NULL,
        [Value] INT NOT NULL,
        [Type] INT NOT NULL, -- 1: Horse, 2: Boat, 3: Minecart
        CONSTRAINT [CK_Vehicle_Value] CHECK ([Value] >= 0)
    );
END
GO

-- Table: Quests
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Quests]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[Quests] (
        [QuestId] INT IDENTITY(1,1) PRIMARY KEY,
        [Name] NVARCHAR(100) NOT NULL,
        [Description] NVARCHAR(1000) NULL,
        [Reward] INT NOT NULL,
        CONSTRAINT [CK_Quest_Reward] CHECK ([Reward] >= 0)
    );
END
GO

-- Table: Monsters
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Monsters]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[Monsters] (
        [MonsterId] INT IDENTITY(1,1) PRIMARY KEY,
        [Name] NVARCHAR(100) NOT NULL,
        [Health] INT NOT NULL,
        [Reward] INT NOT NULL,
        CONSTRAINT [CK_Monster_Health] CHECK ([Health] > 0),
        CONSTRAINT [CK_Monster_Reward] CHECK ([Reward] >= 0)
    );
END
GO

-- Table: Purchases
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Purchases]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[Purchases] (
        [PurchaseId] INT IDENTITY(1,1) PRIMARY KEY,
        [PlayerId] INT NOT NULL,
        [ItemId] INT NULL,
        [VehicleId] INT NULL,
        [PurchaseDate] DATETIME NOT NULL DEFAULT GETDATE(),
        CONSTRAINT [FK_Purchases_Players] FOREIGN KEY ([PlayerId]) REFERENCES [Players]([PlayerId]) ON DELETE CASCADE,
        CONSTRAINT [FK_Purchases_Items] FOREIGN KEY ([ItemId]) REFERENCES [Items]([ItemId]),
        CONSTRAINT [FK_Purchases_Vehicles] FOREIGN KEY ([VehicleId]) REFERENCES [Vehicles]([VehicleId]),
        CONSTRAINT [CK_Purchase_ItemOrVehicle] CHECK (([ItemId] IS NOT NULL AND [VehicleId] IS NULL) OR ([ItemId] IS NULL AND [VehicleId] IS NOT NULL))
    );
END
GO

-- Table: PlayerQuests
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[PlayerQuests]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[PlayerQuests] (
        [PlayerQuestId] INT IDENTITY(1,1) PRIMARY KEY,
        [PlayerId] INT NOT NULL,
        [QuestId] INT NOT NULL,
        [Status] INT NOT NULL DEFAULT 1, -- 1: InProgress, 2: Completed
        [CompletedDate] DATETIME NULL,
        CONSTRAINT [FK_PlayerQuests_Players] FOREIGN KEY ([PlayerId]) REFERENCES [Players]([PlayerId]) ON DELETE CASCADE,
        CONSTRAINT [FK_PlayerQuests_Quests] FOREIGN KEY ([QuestId]) REFERENCES [Quests]([QuestId]),
        CONSTRAINT [UQ_PlayerQuests_Player_Quest] UNIQUE ([PlayerId], [QuestId])
    );
END
GO

-- Table: MonsterKills
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[MonsterKills]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[MonsterKills] (
        [MonsterKillId] INT IDENTITY(1,1) PRIMARY KEY,
        [PlayerId] INT NOT NULL,
        [MonsterId] INT NOT NULL,
        [KillDate] DATETIME NOT NULL DEFAULT GETDATE(),
        CONSTRAINT [FK_MonsterKills_Players] FOREIGN KEY ([PlayerId]) REFERENCES [Players]([PlayerId]) ON DELETE CASCADE,
        CONSTRAINT [FK_MonsterKills_Monsters] FOREIGN KEY ([MonsterId]) REFERENCES [Monsters]([MonsterId])
    );
END
GO

-- =============================================
-- Insert Sample Data
-- =============================================

-- Insert GameModes (at least 5 records)
IF NOT EXISTS (SELECT * FROM [GameModes])
BEGIN
    INSERT INTO [GameModes] ([Name], [Description]) VALUES
    ('Survival', 'Chế độ sinh tồn - người chơi phải thu thập tài nguyên và bảo vệ bản thân'),
    ('Creative', 'Chế độ sáng tạo - người chơi có thể xây dựng tự do không giới hạn'),
    ('Adventure', 'Chế độ phiêu lưu - người chơi khám phá các bản đồ và hoàn thành nhiệm vụ'),
    ('Spectator', 'Chế độ khán giả - người chơi có thể quan sát thế giới mà không tương tác'),
    ('Hardcore', 'Chế độ khó - giống Survival nhưng chỉ có một mạng sống');
END
GO

-- Insert Items (at least 5 records)
IF NOT EXISTS (SELECT * FROM [Items])
BEGIN
    INSERT INTO [Items] ([Name], [Image], [Value], [Type]) VALUES
    ('Diamond Sword', '/images/diamond_sword.png', 500, 2),
    ('Iron Pickaxe', '/images/iron_pickaxe.png', 300, 3),
    ('Leather Armor', '/images/leather_armor.png', 200, 1),
    ('Golden Apple', '/images/golden_apple.png', 100, 4),
    ('Enchanted Bow', '/images/enchanted_bow.png', 400, 2),
    ('Diamond Helmet', '/images/diamond_helmet.png', 450, 1),
    ('Netherite Axe', '/images/netherite_axe.png', 600, 3);
END
GO

-- Insert Vehicles (at least 5 records)
IF NOT EXISTS (SELECT * FROM [Vehicles])
BEGIN
    INSERT INTO [Vehicles] ([Name], [Image], [Value], [Type]) VALUES
    ('White Horse', '/images/white_horse.png', 250, 1),
    ('Brown Horse', '/images/brown_horse.png', 250, 1),
    ('Oak Boat', '/images/oak_boat.png', 150, 2),
    ('Birch Boat', '/images/birch_boat.png', 150, 2),
    ('Minecart', '/images/minecart.png', 200, 3),
    ('Chest Minecart', '/images/chest_minecart.png', 300, 3),
    ('Furnace Minecart', '/images/furnace_minecart.png', 350, 3);
END
GO

-- Insert Quests (at least 5 records)
IF NOT EXISTS (SELECT * FROM [Quests])
BEGIN
    INSERT INTO [Quests] ([Name], [Description], [Reward]) VALUES
    ('First Steps', 'Thu thập 10 khối gỗ đầu tiên', 50),
    ('Mining Master', 'Khai thác 50 khối đá', 100),
    ('Monster Hunter', 'Tiêu diệt 5 quái vật', 150),
    ('Builder', 'Xây dựng một ngôi nhà', 200),
    ('Explorer', 'Khám phá 3 vùng đất mới', 250),
    ('Craft Master', 'Chế tạo 10 công cụ khác nhau', 300),
    ('Dungeon Raider', 'Hoàn thành một dungeon', 400);
END
GO

-- Insert Monsters (at least 5 records)
IF NOT EXISTS (SELECT * FROM [Monsters])
BEGIN
    INSERT INTO [Monsters] ([Name], [Health], [Reward]) VALUES
    ('Zombie', 20, 10),
    ('Skeleton', 20, 15),
    ('Creeper', 20, 20),
    ('Spider', 16, 12),
    ('Enderman', 40, 50),
    ('Witch', 26, 30),
    ('Ender Dragon', 200, 1000);
END
GO

-- Insert Players (at least 5 records)
IF NOT EXISTS (SELECT * FROM [Players])
BEGIN
    INSERT INTO [Players] ([PlayerCode], [Email], [Password], [Health], [Food], [ExperiencePoints], [GameModeId]) VALUES
    ('PLAYER001', 'player1@minecraft.com', 'password123', 100, 100, 500, 1),
    ('PLAYER002', 'player2@minecraft.com', 'password123', 85, 90, 750, 2),
    ('PLAYER003', 'player3@minecraft.com', 'password123', 100, 100, 300, 1),
    ('PLAYER004', 'player4@minecraft.com', 'password123', 70, 80, 1200, 3),
    ('PLAYER005', 'player5@minecraft.com', 'password123', 100, 100, 200, 4),
    ('PLAYER006', 'player6@minecraft.com', 'password123', 95, 95, 600, 1),
    ('PLAYER007', 'player7@minecraft.com', 'password123', 100, 100, 150, 2);
END
GO

-- Insert Purchases (at least 5 records)
IF NOT EXISTS (SELECT * FROM [Purchases])
BEGIN
    INSERT INTO [Purchases] ([PlayerId], [ItemId], [VehicleId], [PurchaseDate]) VALUES
    (1, 1, NULL, GETDATE()),
    (1, NULL, 1, DATEADD(day, -1, GETDATE())),
    (2, 2, NULL, DATEADD(day, -2, GETDATE())),
    (3, NULL, 3, DATEADD(day, -3, GETDATE())),
    (4, 3, NULL, DATEADD(day, -1, GETDATE())),
    (5, NULL, 5, GETDATE()),
    (6, 4, NULL, DATEADD(day, -5, GETDATE()));
END
GO

-- Insert PlayerQuests (at least 5 records)
IF NOT EXISTS (SELECT * FROM [PlayerQuests])
BEGIN
    INSERT INTO [PlayerQuests] ([PlayerId], [QuestId], [Status], [CompletedDate]) VALUES
    (1, 1, 2, DATEADD(day, -5, GETDATE())),
    (1, 2, 2, DATEADD(day, -3, GETDATE())),
    (2, 3, 2, DATEADD(day, -2, GETDATE())),
    (3, 1, 1, NULL),
    (4, 4, 2, DATEADD(day, -1, GETDATE())),
    (5, 5, 1, NULL),
    (6, 2, 2, DATEADD(day, -4, GETDATE()));
END
GO

-- Insert MonsterKills (at least 5 records)
IF NOT EXISTS (SELECT * FROM [MonsterKills])
BEGIN
    INSERT INTO [MonsterKills] ([PlayerId], [MonsterId], [KillDate]) VALUES
    (1, 1, DATEADD(day, -10, GETDATE())),
    (1, 2, DATEADD(day, -8, GETDATE())),
    (2, 3, DATEADD(day, -5, GETDATE())),
    (3, 1, DATEADD(day, -7, GETDATE())),
    (4, 4, DATEADD(day, -3, GETDATE())),
    (5, 2, DATEADD(day, -6, GETDATE())),
    (6, 5, DATEADD(day, -2, GETDATE()));
END
GO

PRINT 'Database created and sample data inserted successfully!';
GO

