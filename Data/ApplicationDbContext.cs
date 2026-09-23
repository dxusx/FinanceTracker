using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using FinanceTracker.Models;
using FinanceTracker.Models.Enums;

namespace FinanceTracker.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Transaction> Transactions => Set<Transaction>();
    public DbSet<Budget> Budgets => Set<Budget>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Category>(entity =>
        {
            entity.HasOne(c => c.User)
                  .WithMany(u => u.CustomCategories)
                  .HasForeignKey(c => c.UserId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.Property(c => c.Name).HasMaxLength(50).IsRequired();
            entity.Property(c => c.Icon).HasMaxLength(50).HasDefaultValue("bi-tag");
            entity.Property(c => c.ColorHex).HasMaxLength(10).HasDefaultValue("#0d6efd");
        });

        builder.Entity<Transaction>(entity =>
        {
            entity.HasOne(t => t.Category)
                  .WithMany(c => c.Transactions)
                  .HasForeignKey(t => t.CategoryId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(t => t.User)
                  .WithMany(u => u.Transactions)
                  .HasForeignKey(t => t.UserId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.Property(t => t.Amount).HasColumnType("decimal(18, 2)");
        });

        builder.Entity<Budget>(entity =>
        {
            entity.HasOne(b => b.Category)
                  .WithMany(c => c.Budgets)
                  .HasForeignKey(b => b.CategoryId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(b => b.User)
                  .WithMany(u => u.Budgets)
                  .HasForeignKey(b => b.UserId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.Property(b => b.MonthlyLimit).HasColumnType("decimal(18, 2)");
            entity.HasIndex(b => new { b.UserId, b.CategoryId, b.Year, b.Month }).IsUnique();
        });

        builder.Entity<Category>().HasData(
            new Category { Id = 1, Name = "Зарплата / Стипендия", Type = TransactionType.Income, Icon = "bi-cash-coin", ColorHex = "#198754", UserId = null },
            new Category { Id = 2, Name = "Подработка", Type = TransactionType.Income, Icon = "bi-laptop", ColorHex = "#20c997", UserId = null },
            new Category { Id = 3, Name = "Переводы от родителей", Type = TransactionType.Income, Icon = "bi-wallet2", ColorHex = "#0dcaf0", UserId = null },
            new Category { Id = 4, Name = "Прочие доходы", Type = TransactionType.Income, Icon = "bi-piggy-bank", ColorHex = "#6c757d", UserId = null },

            new Category { Id = 10, Name = "Супермаркеты", Type = TransactionType.Expense, Icon = "bi-cart3", ColorHex = "#dc3545", UserId = null },
            new Category { Id = 11, Name = "Обеды / Кафе", Type = TransactionType.Expense, Icon = "bi-cup-hot", ColorHex = "#fd7e14", UserId = null },
            new Category { Id = 12, Name = "Транспорт", Type = TransactionType.Expense, Icon = "bi-car-front", ColorHex = "#6f42c1", UserId = null },
            new Category { Id = 13, Name = "Подписки", Type = TransactionType.Expense, Icon = "bi-music-note", ColorHex = "#0d6efd", UserId = null },
            new Category { Id = 14, Name = "Жилье и связь", Type = TransactionType.Expense, Icon = "bi-house-door", ColorHex = "#055160", UserId = null },
            new Category { Id = 15, Name = "Учеба и книги", Type = TransactionType.Expense, Icon = "bi-book", ColorHex = "#0f5132", UserId = null },
            new Category { Id = 16, Name = "Развлечения", Type = TransactionType.Expense, Icon = "bi-controller", ColorHex = "#e83e8c", UserId = null },
            new Category { Id = 17, Name = "Прочее", Type = TransactionType.Expense, Icon = "bi-tags", ColorHex = "#495057", UserId = null }
        );
    }
}
