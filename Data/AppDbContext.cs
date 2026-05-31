using Microsoft.EntityFrameworkCore;
using PhoneStore.Models;

namespace PhoneStore.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Review> Reviews => Set<Review>();
    public DbSet<WalletTransaction> WalletTransactions => Set<WalletTransaction>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var isSqlite = Database.ProviderName?.Contains("Sqlite", StringComparison.OrdinalIgnoreCase) == true;

        modelBuilder.Entity<User>(e =>
        {
            e.HasIndex(u => u.Email).IsUnique();
            e.Property(u => u.Role).HasDefaultValue("User");
            e.Property(u => u.IsActive).HasDefaultValue(true);
            if (!isSqlite)
                e.Property(u => u.WalletBalance).HasColumnType("decimal(18,2)");
            e.Property(u => u.WalletBalance).HasDefaultValue(0m);
            e.HasMany(u => u.WalletTransactions)
             .WithOne(w => w.User)
             .HasForeignKey(w => w.UserId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Product>(e =>
        {
            e.HasKey(p => p.Id);
            e.Property(p => p.Id).ValueGeneratedNever();
            e.Property(p => p.Name).IsRequired();
            if (!isSqlite)
                e.Property(p => p.Price).HasColumnType("decimal(18,2)");
        });

        modelBuilder.Entity<Review>(e =>
        {
            e.HasKey(r => r.Id);
            e.Property(r => r.Id).ValueGeneratedOnAdd();
            if (!isSqlite)
                e.Property(r => r.ReviewerName).HasMaxLength(100);
            e.HasOne(r => r.ReviewerUser)
             .WithMany()
             .HasForeignKey(r => r.ReviewerUserId)
             .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<WalletTransaction>(e =>
        {
            e.HasKey(w => w.Id);
            if (!isSqlite)
                e.Property(w => w.Amount).HasColumnType("decimal(18,2)");
            if (!isSqlite)
                e.Property(w => w.BalanceBefore).HasColumnType("decimal(18,2)");
            if (!isSqlite)
                e.Property(w => w.BalanceAfter).HasColumnType("decimal(18,2)");
            if (!isSqlite)
                e.Property(w => w.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
        });

        modelBuilder.Entity<Order>(e =>
        {
            e.HasKey(o => o.Id);
            e.Property(o => o.Id).ValueGeneratedOnAdd();
            if (!isSqlite)
                e.Property(o => o.TotalAmount).HasColumnType("decimal(18,2)");
            if (!isSqlite)
                e.Property(o => o.OrderDate).HasDefaultValueSql("GETUTCDATE()");
            e.HasOne(o => o.User)
             .WithMany()
             .HasForeignKey(o => o.UserId)
             .OnDelete(DeleteBehavior.SetNull);
            e.HasMany(o => o.Items)
             .WithOne(i => i.Order)
             .HasForeignKey(i => i.OrderId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<OrderItem>(e =>
        {
            e.HasKey(i => i.Id);
            e.Property(i => i.Id).ValueGeneratedOnAdd();
            if (!isSqlite)
                e.Property(i => i.Price).HasColumnType("decimal(18,2)");
        });
    }
}
