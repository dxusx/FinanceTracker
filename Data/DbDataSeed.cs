using Microsoft.AspNetCore.Identity;
using FinanceTracker.Models;

namespace FinanceTracker.Data;

public static class DbDataSeed
{
    public static async Task SeedAsync(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        // Создаем тестового студента, если в базе еще никого нет
        const string studentEmail = "student@mail.ru";
        var studentUser = await userManager.FindByEmailAsync(studentEmail);

        if (studentUser == null)
        {
            studentUser = new ApplicationUser
            {
                UserName = studentEmail,
                Email = studentEmail,
                FullName = "Карим",
                EmailConfirmed = true,
                CurrencySymbol = "₽"
            };

            var result = await userManager.CreateAsync(studentUser, "Student123!");
            if (result.Succeeded)
            {
                var today = DateTime.Today;

                // Реалистичные операции студента
                var transactions = new List<Transaction>
                {
                    new() { UserId = studentUser.Id, CategoryId = 1, Amount = 18500m, Date = new DateTime(today.Year, today.Month, 5), Note = "Стипендия" },
                    new() { UserId = studentUser.Id, CategoryId = 2, Amount = 6000m, Date = new DateTime(today.Year, today.Month, 12), Note = "Подработка" },

                    new() { UserId = studentUser.Id, CategoryId = 10, Amount = 1450m, Date = new DateTime(today.Year, today.Month, 6), Note = "Пятерочка" },
                    new() { UserId = studentUser.Id, CategoryId = 10, Amount = 289m, Date = new DateTime(today.Year, today.Month, 14), Note = "Пятерочка (снеки)" },
                    new() { UserId = studentUser.Id, CategoryId = 12, Amount = 65m, Date = new DateTime(today.Year, today.Month, 7), Note = "Пополнение Тройки / Проездной" },
                    new() { UserId = studentUser.Id, CategoryId = 12, Amount = 750m, Date = new DateTime(today.Year, today.Month, 1), Note = "Пополнение Тройки / Проездной на месяц" },
                    new() { UserId = studentUser.Id, CategoryId = 11, Amount = 890m, Date = new DateTime(today.Year, today.Month, 9), Note = "Додо Пицца" },
                    new() { UserId = studentUser.Id, CategoryId = 11, Amount = 320m, Date = new DateTime(today.Year, today.Month, 11), Note = "Обед в столовой универа" },
                    new() { UserId = studentUser.Id, CategoryId = 13, Amount = 299m, Date = new DateTime(today.Year, today.Month, 10), Note = "Яндекс Музыка" },
                    new() { UserId = studentUser.Id, CategoryId = 14, Amount = 450m, Date = new DateTime(today.Year, today.Month, 2), Note = "Оплата мобильной связи" },
                    new() { UserId = studentUser.Id, CategoryId = 15, Amount = 540m, Date = new DateTime(today.Year, today.Month, 8), Note = "Тетради и ручки к сессии" }
                };

                context.Transactions.AddRange(transactions);

                // Лимиты на текущий месяц
                var budgets = new List<Budget>
                {
                    new() { UserId = studentUser.Id, CategoryId = 10, MonthlyLimit = 15000m, Month = today.Month, Year = today.Year },
                    new() { UserId = studentUser.Id, CategoryId = 11, MonthlyLimit = 6000m, Month = today.Month, Year = today.Year },
                    new() { UserId = studentUser.Id, CategoryId = 12, MonthlyLimit = 2000m, Month = today.Month, Year = today.Year }
                };

                context.Budgets.AddRange(budgets);
                await context.SaveChangesAsync();
            }
        }
    }
}
