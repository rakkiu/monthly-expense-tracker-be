using ExpenseTracker.Domain.Entities;
using ExpenseTracker.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace ExpenseTracker.Infrastructure.Persistence.Seed;

public static class DefaultCategoriesSeeder
{
    public static readonly List<(string Name, TransactionType Type, string Color, string? Icon)> Defaults =
    [
        ("Ăn uống", TransactionType.Expense, "#EF4444", "utensils"),
        ("Đi lại", TransactionType.Expense, "#F59E0B", "car"),
        ("Giải trí", TransactionType.Expense, "#8B5CF6", "film"),
        ("Sức khỏe", TransactionType.Expense, "#10B981", "heart"),
        ("Học tập", TransactionType.Expense, "#3B82F6", "book"),
        ("Mua sắm", TransactionType.Expense, "#EC4899", "shopping-bag"),
        ("Hóa đơn", TransactionType.Expense, "#6B7280", "file-invoice"),
        ("Khác", TransactionType.Expense, "#9CA3AF", "ellipsis"),
        ("Lương", TransactionType.Income, "#10B981", "wallet"),
        ("Freelance", TransactionType.Income, "#3B82F6", "laptop"),
        ("Đầu tư", TransactionType.Income, "#8B5CF6", "chart-line"),
        ("Khác", TransactionType.Income, "#9CA3AF", "plus-circle"),
    ];

    public static async Task SeedAsync(AppDbContext context)
    {
        if (await context.Categories.AnyAsync(c => c.IsDefault))
            return;

        // Get all users and seed default categories for each
        var users = await context.Users.ToListAsync();

        foreach (var user in users)
        {
            foreach (var (name, type, color, icon) in Defaults)
            {
                context.Categories.Add(new Category
                {
                    Id = Guid.NewGuid(),
                    UserId = user.Id,
                    Name = name,
                    Type = type,
                    Color = color,
                    Icon = icon,
                    IsDefault = true
                });
            }
        }

        await context.SaveChangesAsync();
    }
}
