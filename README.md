# Minecraft Game Management System

Hệ thống quản lý game Minecraft sử dụng ASP.NET Core MVC và SQLite.

## Yêu cầu hệ thống

- .NET 8.0 hoặc cao hơn
- Visual Studio 2022 hoặc VS Code

## Cài đặt

### 1. Cài đặt packages

```bash
dotnet restore
```

### 2. Chạy ứng dụng

```bash
dotnet build
dotnet run
```

Hoặc chạy từ Visual Studio bằng cách nhấn F5.

**Lưu ý quan trọng:** Khi chạy ứng dụng lần đầu, file `MinecraftDB.db` sẽ **tự động được tạo** trong thư mục gốc của project cùng với dữ liệu mẫu (seed data). Bạn không cần phải tạo database thủ công!

### 3. Cấu hình Connection String (Tùy chọn)

Connection string mặc định trong `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=MinecraftDB.db"
  }
}
```

Bạn có thể thay đổi tên file database hoặc đường dẫn nếu muốn.

## Cấu trúc dự án

```
Minecraft/
├── Controllers/
│   └── HomeController.cs          # Controller chính với các API endpoints
├── Data/
│   └── ApplicationDbContext.cs    # DbContext với cấu hình SQLite
├── Models/
│   ├── Player.cs                   # Model người chơi
│   ├── GameMode.cs                 # Model chế độ chơi
│   ├── Item.cs                     # Model vật phẩm
│   ├── Vehicle.cs                  # Model phương tiện
│   ├── Quest.cs                    # Model nhiệm vụ
│   ├── Monster.cs                  # Model quái vật
│   ├── Purchase.cs                 # Model giao dịch mua hàng
│   ├── PlayerQuest.cs             # Model nhiệm vụ của người chơi
│   ├── MonsterKill.cs             # Model lịch sử tiêu diệt quái vật
│   └── ResponseAPI.cs             # Model response API
├── Views/
│   ├── Home/
│   │   ├── Index.cshtml
│   │   └── Privacy.cshtml
│   └── Shared/
│       └── _Layout.cshtml
├── Program.cs                      # Entry point và cấu hình (tự động tạo DB)
├── appsettings.json               # Cấu hình ứng dụng
├── Minecraft.csproj               # Project file với các packages
└── MinecraftDB.db                 # File database SQLite (tự động tạo khi chạy)
```

## API Endpoints

### Player Management

- **GET** `/Home/GetPlayers` - Lấy danh sách tất cả người chơi
- **GET** `/Home/GetPlayer?id={id}` - Lấy thông tin người chơi theo ID
- **POST** `/Home/CreatePlayer` - Tạo người chơi mới

### Items & Vehicles

- **GET** `/Home/GetItems` - Lấy danh sách vật phẩm
- **GET** `/Home/GetVehicles` - Lấy danh sách phương tiện
- **POST** `/Home/PurchaseItem` - Mua vật phẩm hoặc phương tiện

### Quests

- **GET** `/Home/GetQuests` - Lấy danh sách nhiệm vụ
- **POST** `/Home/CompleteQuest` - Hoàn thành nhiệm vụ

### Monsters

- **POST** `/Home/KillMonster` - Ghi nhận tiêu diệt quái vật

## Ví dụ sử dụng API

### Tạo người chơi mới

```json
POST /Home/CreatePlayer
Content-Type: application/json

{
  "playerCode": "PLAYER008",
  "email": "player8@minecraft.com",
  "password": "password123",
  "health": 100,
  "food": 100,
  "experiencePoints": 0,
  "gameModeId": 1
}
```

### Mua vật phẩm

```json
POST /Home/PurchaseItem
Content-Type: application/json

{
  "playerId": 1,
  "itemId": 1,
  "vehicleId": null
}
```

### Hoàn thành nhiệm vụ

```json
POST /Home/CompleteQuest
Content-Type: application/json

{
  "playerId": 1,
  "questId": 1
}
```

### Tiêu diệt quái vật

```json
POST /Home/KillMonster
Content-Type: application/json

{
  "playerId": 1,
  "monsterId": 1
}
```

## Thiết kế Database

Xem file `DATABASE_DESIGN.md` để biết chi tiết về:
- Các thực thể và thuộc tính
- Mối quan hệ giữa các bảng
- Ràng buộc và quy tắc nghiệp vụ

## Lưu ý

- Mỗi lần mua chỉ mua 1 item HOẶC 1 vehicle
- Khi mua, điểm kinh nghiệm sẽ giảm đi đúng bằng giá trị của item/vehicle
- Khi hoàn thành nhiệm vụ hoặc tiêu diệt quái vật, điểm kinh nghiệm sẽ tăng
- PlayerCode và Email phải là duy nhất

## Phát triển thêm

Để thêm tính năng mới:
1. Tạo Model mới trong thư mục `Models/`
2. Thêm DbSet vào `ApplicationDbContext`
3. Tạo Migration: `dotnet ef migrations add MigrationName`
4. Cập nhật database: `dotnet ef database update`
5. Thêm API endpoints trong Controller

