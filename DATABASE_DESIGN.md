# Thiết kế Cơ sở dữ liệu Minecraft

## 1. Phân tích bài toán

### Các thực thể (Entities) chính:
1. **Player** - Người chơi
2. **GameMode** - Chế độ chơi
3. **Item** - Vật phẩm trong cửa hàng
4. **Vehicle** - Phương tiện di chuyển
5. **Quest** - Nhiệm vụ
6. **Monster** - Quái vật
7. **Purchase** - Giao dịch mua hàng
8. **PlayerQuest** - Nhiệm vụ của người chơi
9. **MonsterKill** - Lịch sử tiêu diệt quái vật

## 2. Các thuộc tính của từng thực thể

### Player (Người chơi)
- **PlayerId** (PK): ID người chơi
- **PlayerCode** (Unique): Mã người chơi
- **Email** (Unique): Email đăng ký
- **Password**: Mật khẩu
- **Health**: Sức khỏe (0-100)
- **Food**: Thanh thức ăn (0-100)
- **ExperiencePoints**: Điểm kinh nghiệm
- **GameModeId** (FK): Chế độ chơi

### GameMode (Chế độ chơi)
- **GameModeId** (PK): ID chế độ
- **Name** (Unique): Tên chế độ (Survival, Creative, Adventure, Spectator)
- **Description**: Mô tả

### Item (Vật phẩm)
- **ItemId** (PK): ID vật phẩm
- **Name**: Tên vật phẩm
- **Image**: Đường dẫn hình ảnh
- **Value**: Giá trị (điểm kinh nghiệm cần để mua)
- **Type**: Loại (Clothing=1, Weapon=2, Tool=3, Special=4)

### Vehicle (Phương tiện)
- **VehicleId** (PK): ID phương tiện
- **Name**: Tên phương tiện
- **Image**: Đường dẫn hình ảnh
- **Value**: Giá trị (điểm kinh nghiệm cần để mua)
- **Type**: Loại (Horse=1, Boat=2, Minecart=3)

### Quest (Nhiệm vụ)
- **QuestId** (PK): ID nhiệm vụ
- **Name**: Tên nhiệm vụ
- **Description**: Mô tả
- **Reward**: Phần thưởng (điểm kinh nghiệm)

### Monster (Quái vật)
- **MonsterId** (PK): ID quái vật
- **Name**: Tên quái vật
- **Health**: Máu
- **Reward**: Phần thưởng khi tiêu diệt

### Purchase (Giao dịch mua hàng)
- **PurchaseId** (PK): ID giao dịch
- **PlayerId** (FK): ID người chơi
- **ItemId** (FK, Nullable): ID vật phẩm (nếu mua item)
- **VehicleId** (FK, Nullable): ID phương tiện (nếu mua vehicle)
- **PurchaseDate**: Ngày mua
- **Ràng buộc**: Phải có ItemId HOẶC VehicleId, không được có cả hai

### PlayerQuest (Nhiệm vụ của người chơi)
- **PlayerQuestId** (PK): ID bản ghi
- **PlayerId** (FK): ID người chơi
- **QuestId** (FK): ID nhiệm vụ
- **Status**: Trạng thái (InProgress=1, Completed=2)
- **CompletedDate**: Ngày hoàn thành
- **Ràng buộc Unique**: Một người chơi chỉ có thể có một bản ghi cho mỗi nhiệm vụ

### MonsterKill (Lịch sử tiêu diệt quái vật)
- **MonsterKillId** (PK): ID bản ghi
- **PlayerId** (FK): ID người chơi
- **MonsterId** (FK): ID quái vật
- **KillDate**: Ngày tiêu diệt

## 3. Mối quan hệ giữa các thực thể

### Quan hệ 1-Nhiều (One-to-Many):
1. **GameMode** → **Player**: Một chế độ chơi có nhiều người chơi
2. **Player** → **Purchase**: Một người chơi có nhiều giao dịch mua hàng
3. **Item** → **Purchase**: Một vật phẩm có thể được mua nhiều lần
4. **Vehicle** → **Purchase**: Một phương tiện có thể được mua nhiều lần
5. **Player** → **PlayerQuest**: Một người chơi có nhiều nhiệm vụ
6. **Quest** → **PlayerQuest**: Một nhiệm vụ có thể được gán cho nhiều người chơi
7. **Player** → **MonsterKill**: Một người chơi có thể tiêu diệt nhiều quái vật
8. **Monster** → **MonsterKill**: Một loại quái vật có thể bị tiêu diệt nhiều lần

## 4. Ràng buộc (Constraints)

### Khóa chính (Primary Keys):
- Tất cả các bảng đều có khóa chính tự tăng (IDENTITY)

### Khóa ngoại (Foreign Keys):
- Player.GameModeId → GameMode.GameModeId
- Purchase.PlayerId → Player.PlayerId
- Purchase.ItemId → Item.ItemId
- Purchase.VehicleId → Vehicle.VehicleId
- PlayerQuest.PlayerId → Player.PlayerId
- PlayerQuest.QuestId → Quest.QuestId
- MonsterKill.PlayerId → Player.PlayerId
- MonsterKill.MonsterId → Monster.MonsterId

### Ràng buộc duy nhất (Unique Constraints):
- Player.PlayerCode (Unique)
- Player.Email (Unique)
- GameMode.Name (Unique)
- PlayerQuest (PlayerId, QuestId) (Unique)

### Ràng buộc kiểm tra (Check Constraints):
- Player.Health: 0 <= Health <= 100
- Player.Food: 0 <= Food <= 100
- Player.ExperiencePoints: >= 0
- Item.Value: >= 0
- Vehicle.Value: >= 0
- Quest.Reward: >= 0
- Monster.Health: > 0
- Monster.Reward: >= 0
- Purchase: Phải có ItemId HOẶC VehicleId (không được có cả hai)

### Ràng buộc xóa (Delete Behavior):
- Khi xóa Player: CASCADE cho Purchase, PlayerQuest, MonsterKill
- Khi xóa GameMode: RESTRICT (không cho xóa nếu còn Player)
- Khi xóa Item/Vehicle: RESTRICT (không cho xóa nếu còn Purchase)
- Khi xóa Quest: RESTRICT (không cho xóa nếu còn PlayerQuest)
- Khi xóa Monster: RESTRICT (không cho xóa nếu còn MonsterKill)

## 5. Quy tắc nghiệp vụ

1. **Tạo tài khoản**: Mỗi người chơi phải có PlayerCode và Email duy nhất
2. **Mua hàng**: 
   - Mỗi lần mua chỉ mua 1 item HOẶC 1 vehicle
   - Phải có đủ điểm kinh nghiệm (ExperiencePoints >= Value)
   - Sau khi mua, ExperiencePoints giảm đi đúng bằng Value
3. **Hoàn thành nhiệm vụ**: 
   - Khi hoàn thành, người chơi nhận Reward điểm kinh nghiệm
   - Một người chơi chỉ có thể có một bản ghi cho mỗi nhiệm vụ
4. **Tiêu diệt quái vật**: 
   - Khi tiêu diệt, người chơi nhận Reward điểm kinh nghiệm

## 6. Cấu hình SQL Server

- **Database Name**: MinecraftDB
- **Connection String**: 
  ```
  Server=(localdb)\mssqllocaldb;Database=MinecraftDB;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True
  ```

## 7. Dữ liệu mẫu

Mỗi bảng đã được chèn ít nhất 5 bản ghi mẫu trong file `Database_Script.sql`.

