using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FinanceTracker.Data;
using FinanceTracker.Models;
using FinanceTracker.Models.Enums;
using FinanceTracker.ViewModels;

namespace FinanceTracker.Controllers;

[Authorize]
public class DashboardController : Controller
{
    private readonly ApplicationDbContext _context;

    public DashboardController(ApplicationDbContext context)
    {
        _context = context;
    }

    private string CurrentUserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

    public async Task<IActionResult> Index(string period = "this_month", DateTime? customStart = null, DateTime? customEnd = null)
    {
        var now = DateTime.Today;
        DateTime startDate;
        DateTime endDate;

        switch (period)
        {
            case "last_month":
                var lastMonth = now.AddMonths(-1);
                startDate = new DateTime(lastMonth.Year, lastMonth.Month, 1);
                endDate = startDate.AddMonths(1).AddDays(-1);
                break;
            case "last_3_months":
                startDate = new DateTime(now.Year, now.Month, 1).AddMonths(-2);
                endDate = new DateTime(now.Year, now.Month, DateTime.DaysInMonth(now.Year, now.Month));
                break;
            case "this_year":
                startDate = new DateTime(now.Year, 1, 1);
                endDate = new DateTime(now.Year, 12, 31);
                break;
            case "custom":
                startDate = customStart ?? new DateTime(now.Year, now.Month, 1);
                endDate = customEnd ?? now;
                break;
            case "all_time":
                startDate = DateTime.MinValue;
                endDate = DateTime.MaxValue;
                break;
            case "this_month":
            default:
                period = "this_month";
                startDate = new DateTime(now.Year, now.Month, 1);
                endDate = new DateTime(now.Year, now.Month, DateTime.DaysInMonth(now.Year, now.Month));
                break;
        }

        var userId = CurrentUserId;

        var transactionsQuery = _context.Transactions
            .Include(t => t.Category)
            .Where(t => t.UserId == userId);

        if (period != "all_time")
        {
            transactionsQuery = transactionsQuery.Where(t => t.Date >= startDate && t.Date <= endDate);
        }

        List<Transaction> periodTransactions = await transactionsQuery.ToListAsync();

        decimal totalIncome = 0;
        // Считаем общую сумму расходов за выбранный месяц
        decimal totalExpense = 0;

        foreach (var item in periodTransactions)
        {
            if (item.Category?.Type == TransactionType.Income)
            {
                totalIncome += item.Amount;
            }
            else if (item.Category?.Type == TransactionType.Expense)
            {
                totalExpense += item.Amount;
            }
        }

        var categoryExpenses = periodTransactions
            .Where(t => t.Category?.Type == TransactionType.Expense && t.Category != null)
            .GroupBy(t => t.Category!)
            .Select(g => new CategorySummaryDto
            {
                CategoryName = g.Key.Name,
                Icon = g.Key.Icon,
                ColorHex = g.Key.ColorHex,
                Amount = g.Sum(x => x.Amount),
                Percentage = totalExpense > 0 ? Math.Round((double)(g.Sum(x => x.Amount) / totalExpense) * 100, 1) : 0
            })
            .OrderByDescending(x => x.Amount)
            .ToList();

        var categoryIncomes = periodTransactions
            .Where(t => t.Category?.Type == TransactionType.Income && t.Category != null)
            .GroupBy(t => t.Category!)
            .Select(g => new CategorySummaryDto
            {
                CategoryName = g.Key.Name,
                Icon = g.Key.Icon,
                ColorHex = g.Key.ColorHex,
                Amount = g.Sum(x => x.Amount),
                Percentage = totalIncome > 0 ? Math.Round((double)(g.Sum(x => x.Amount) / totalIncome) * 100, 1) : 0
            })
            .OrderByDescending(x => x.Amount)
            .ToList();

        // TODO: добавить пагинацию, если записей будет больше сотни
        List<Transaction> recentTransactions = await _context.Transactions
            .Include(t => t.Category)
            .Where(t => t.UserId == userId)
            .OrderByDescending(t => t.Date)
            .ThenByDescending(t => t.Id)
            .Take(7)
            .ToListAsync();

        var monthlyLabels = new List<string>();
        var monthlyIncomes = new List<decimal>();
        var monthlyExpenses = new List<decimal>();

        var sixMonthsAgo = new DateTime(now.Year, now.Month, 1).AddMonths(-5);
        var sixMonthsTransactions = await _context.Transactions
            .Include(t => t.Category)
            .Where(t => t.UserId == userId && t.Date >= sixMonthsAgo)
            .ToListAsync();

        for (int i = -5; i <= 0; i++)
        {
            var targetMonth = new DateTime(now.Year, now.Month, 1).AddMonths(i);
            var label = targetMonth.ToString("MMM yyyy", new System.Globalization.CultureInfo("ru-RU"));
            monthlyLabels.Add(label);

            var monthTrans = sixMonthsTransactions
                .Where(t => t.Date.Year == targetMonth.Year && t.Date.Month == targetMonth.Month)
                .ToList();

            var inc = monthTrans.Where(t => t.Category?.Type == TransactionType.Income).Sum(t => t.Amount);
            var exp = monthTrans.Where(t => t.Category?.Type == TransactionType.Expense).Sum(t => t.Amount);

            monthlyIncomes.Add(inc);
            monthlyExpenses.Add(exp);
        }

        var currentMonthBudgets = await _context.Budgets
            .Include(b => b.Category)
            .Where(b => b.UserId == userId && b.Month == now.Month && b.Year == now.Year)
            .ToListAsync();

        var currentMonthExpenses = await _context.Transactions
            .Where(t => t.UserId == userId && t.Date.Month == now.Month && t.Date.Year == now.Year)
            .GroupBy(t => t.CategoryId)
            .Select(g => new { CategoryId = g.Key, Spent = g.Sum(x => x.Amount) })
            .ToDictionaryAsync(x => x.CategoryId, x => x.Spent);

        var budgetProgresses = currentMonthBudgets.Select(b =>
        {
            currentMonthExpenses.TryGetValue(b.CategoryId, out var spent);
            return new BudgetProgressDto
            {
                BudgetId = b.Id,
                CategoryName = b.Category?.Name ?? "Категория",
                Icon = b.Category?.Icon ?? "bi-tag",
                ColorHex = b.Category?.ColorHex ?? "#0d6efd",
                Limit = b.MonthlyLimit,
                Spent = spent
            };
        }).ToList();

        var model = new DashboardViewModel
        {
            StartDate = startDate,
            EndDate = endDate,
            SelectedPeriod = period,
            TotalIncome = totalIncome,
            TotalExpense = totalExpense,
            RecentTransactions = recentTransactions,
            CategoryExpenses = categoryExpenses,
            CategoryIncomes = categoryIncomes,
            MonthlyLabels = monthlyLabels,
            MonthlyIncomes = monthlyIncomes,
            MonthlyExpenses = monthlyExpenses,
            BudgetProgresses = budgetProgresses
        };

        return View(model);
    }
}
