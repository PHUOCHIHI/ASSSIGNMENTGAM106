using Microsoft.EntityFrameworkCore;
using Minecraft.Models;

namespace Minecraft.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Player> Players { get; set; }
        public DbSet<Region> Regions { get; set; }
        public DbSet<GameMode> GameModes { get; set; }
        public DbSet<Item> Items { get; set; }
        public DbSet<Quest> Quests { get; set; }
        public DbSet<Monster> Monsters { get; set; }
        public DbSet<Vehicle> Vehicles { get; set; }
        public DbSet<Purchase> Purchases { get; set; }
        public DbSet<PlayerQuest> PlayerQuests { get; set; }
        public DbSet<MonsterKill> MonsterKills { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure Player
            modelBuilder.Entity<Player>(entity =>
            {
                entity.HasIndex(e => e.PlayerCode).IsUnique();
                entity.HasIndex(e => e.Email).IsUnique();
                entity.Property(e => e.PlayerCode).IsRequired();
                entity.Property(e => e.Email).IsRequired();
                entity.Property(e => e.Password).IsRequired();
                // SQLite không hỗ trợ check constraints, validation sẽ được xử lý ở tầng ứng dụng
            });

            modelBuilder.Entity<Region>(entity =>
            {
                entity.HasIndex(e => e.Name).IsUnique();
                entity.Property(e => e.Name).IsRequired();
            });

            // Configure GameMode
            modelBuilder.Entity<GameMode>(entity =>
            {
                entity.HasIndex(e => e.Name).IsUnique();
                entity.Property(e => e.Name).IsRequired();
            });

            // Configure Item
            modelBuilder.Entity<Item>(entity =>
            {
                entity.Property(e => e.Name).IsRequired();
                entity.Property(e => e.Value).IsRequired();
                entity.Property(e => e.Type).IsRequired();
            });

            // Configure Quest
            modelBuilder.Entity<Quest>(entity =>
            {
                entity.Property(e => e.Name).IsRequired();
                entity.Property(e => e.Reward).IsRequired();
            });

            // Configure Monster
            modelBuilder.Entity<Monster>(entity =>
            {
                entity.Property(e => e.Name).IsRequired();
                entity.Property(e => e.Health).IsRequired();
                entity.Property(e => e.Reward).IsRequired();
            });

            // Configure Vehicle
            modelBuilder.Entity<Vehicle>(entity =>
            {
                entity.Property(e => e.Name).IsRequired();
                entity.Property(e => e.Value).IsRequired();
                entity.Property(e => e.Type).IsRequired();
            });

            // Configure Purchase
            modelBuilder.Entity<Purchase>(entity =>
            {
                entity.Property(e => e.PlayerId).IsRequired();
                entity.Property(e => e.PurchaseDate).IsRequired();
                // SQLite không hỗ trợ check constraints, validation sẽ được xử lý ở tầng ứng dụng
            });

            // Configure PlayerQuest
            modelBuilder.Entity<PlayerQuest>(entity =>
            {
                entity.Property(e => e.PlayerId).IsRequired();
                entity.Property(e => e.QuestId).IsRequired();
                entity.Property(e => e.Status).IsRequired();
                // Unique constraint: one player can only have one active quest of the same type
                entity.HasIndex(e => new { e.PlayerId, e.QuestId }).IsUnique();
            });

            // Configure MonsterKill
            modelBuilder.Entity<MonsterKill>(entity =>
            {
                entity.Property(e => e.PlayerId).IsRequired();
                entity.Property(e => e.MonsterId).IsRequired();
                entity.Property(e => e.KillDate).IsRequired();
            });

            // Configure Relationships
            modelBuilder.Entity<Player>()
                .HasOne(p => p.GameMode)
                .WithMany(g => g.Players)
                .HasForeignKey(p => p.GameModeId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Player>()
                .HasOne(p => p.Region)
                .WithMany(r => r.Players)
                .HasForeignKey(p => p.RegionId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Purchase>()
                .HasOne(p => p.Player)
                .WithMany(pl => pl.Purchases)
                .HasForeignKey(p => p.PlayerId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Purchase>()
                .HasOne(p => p.Item)
                .WithMany(i => i.Purchases)
                .HasForeignKey(p => p.ItemId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Purchase>()
                .HasOne(p => p.Vehicle)
                .WithMany(v => v.Purchases)
                .HasForeignKey(p => p.VehicleId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<PlayerQuest>()
                .HasOne(pq => pq.Player)
                .WithMany(p => p.PlayerQuests)
                .HasForeignKey(pq => pq.PlayerId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<PlayerQuest>()
                .HasOne(pq => pq.Quest)
                .WithMany(q => q.PlayerQuests)
                .HasForeignKey(pq => pq.QuestId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<MonsterKill>()
                .HasOne(mk => mk.Player)
                .WithMany(p => p.MonsterKills)
                .HasForeignKey(mk => mk.PlayerId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<MonsterKill>()
                .HasOne(mk => mk.Monster)
                .WithMany(m => m.MonsterKills)
                .HasForeignKey(mk => mk.MonsterId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}

