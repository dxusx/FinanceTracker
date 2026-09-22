using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using FinanceTracker.Data;
using FinanceTracker.Models;
using FinanceTracker.Models.Enums;
using FinanceTracker.ViewModels;

namespace FinanceTracker.Controllers;

[Authorize]
public class BudgetsController : Controller
{
    private readonly ApplicationDbContext _context;

    public BudgetsController(ApplicationDbContext context)
    {
        _context = context;
    }

    private string CurrentUserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

    public async Task<IActionResult> Index(int? month, int? year)
    {
        var now = DateTime.Today;
        var selectedMonth = month ?? now.Month;
        var selectedYear = year ?? now.Year;
        var userId = CurrentUserId;

        var budgets = await _context.Budgets
            .Include(b => b.Category)
            .Where(b => b.UserId == userId && b.Month == selectedMonth && b.Year == selectedYear)
            .ToListAsync();

        var expenses = await _context.Transactions
            .Where(t => t.UserId == userId && t.Date.Month == selectedMonth && t.Date.Year == selectedYear)
            .GroupBy(t => t.CategoryId)
            .Select(g => new { CategoryId = g.Key, Spent = g.Sum(x => x.Amount) })
            .ToDictionaryAsync(x => x.CategoryId, x => x.Spent);

        var budgetProgressList = budgets.Select(b =>
        {
            expenses.TryGetValue(b.CategoryId, out var spent);
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

        var model = new BudgetListViewModel
        {
            Month = selectedMonth,
            Year = selectedYear,
            TotalBudget = budgetProgressList.Sum(b => b.Limit),
            TotalSpent = budgetProgressList.Sum(b => b.Spent),
            Budgets = budgetProgressList
        };

        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        var model = new BudgetViewModel
        {
            Month = DateTime.Today.Month,
            Year = DateTime.Today.Year,
            CategoryOptions = await GetAvailableCategoriesSelectListAsync(DateTime.Today.Month, DateTime.Today.Year)
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(BudgetViewModel model)
    {
        var userId = CurrentUserId;

        var existing = await _context.Budgets.AnyAsync(b =>
            b.UserId == userId &&
            b.CategoryId == model.CategoryId &&
            b.Month == model.Month &&
            b.Year == model.Year);

        if (existing)
        {
            ModelState.AddModelError(nameof(model.CategoryId), "Бюджет для этой категории на выбранный месяц уже создан.");
        }

        if (!ModelState.IsValid)
        {
            model.CategoryOptions = await GetAvailableCategoriesSelectListAsync(model.Month, model.Year);
            return View(model);
        }

        var budget = new Budget
        {
            UserId = userId,
            CategoryId = model.CategoryId,
            MonthlyLimit = model.MonthlyLimit,
            Month = model.Month,
            Year = model.Year
        };

        _context.Budgets.Add(budget);
        await _context.SaveChangesAsync();

        TempData["Success"] = "Лимит бюджета успешно установлен!";
        return RedirectToAction(nameof(Index), new { month = model.Month, year = model.Year });
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var userId = CurrentUserId;
        var budget = await _context.Budgets.Include(b => b.Category).FirstOrDefaultAsync(b => b.Id == id && b.UserId == userId);
        if (budget == null)
        {
            return NotFound();
        }

        var model = new BudgetViewModel
        {
            Id = budget.Id,
            CategoryId = budget.CategoryId,
            MonthlyLimit = budget.MonthlyLimit,
            Month = budget.Month,
            Year = budget.Year,
            CategoryOptions = new List<SelectListItem>
            {
                new() { Value = budget.CategoryId.ToString(), Text = budget.Category?.Name ?? "Категория", Selected = true }
            }
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, BudgetViewModel model)
    {
        if (id != model.Id)
        {
            return BadRequest();
        }

        var userId = CurrentUserId;
        var budget = await _context.Budgets.FirstOrDefaultAsync(b => b.Id == id && b.UserId == userId);
        if (budget == null)
        {
            return NotFound();
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        budget.MonthlyLimit = model.MonthlyLimit;
        _context.Update(budget);
        await _context.SaveChangesAsync();

        TempData["Success"] = "Лимит бюджета обновлен!";
        return RedirectToAction(nameof(Index), new { month = budget.Month, year = budget.Year });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var userId = CurrentUserId;
        var budget = await _context.Budgets.FirstOrDefaultAsync(b => b.Id == id && b.UserId == userId);
        if (budget != null)
        {
            var month = budget.Month;
            var year = budget.Year;
            _context.Budgets.Remove(budget);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Бюджет удален.";
            return RedirectToAction(nameof(Index), new { month, year });
        }

        return RedirectToAction(nameof(Index));
    }

    private async Task<List<SelectListItem>> GetAvailableCategoriesSelectListAsync(int month, int year)
    {
        var userId = CurrentUserId;

        var existingCategoryIds = await _context.Budgets
            .Where(b => b.UserId == userId && b.Month == month && b.Year == year)
            .Select(b => b.CategoryId)
            .ToListAsync();

        return await _context.Categories
            .Where(c => (c.UserId == null || c.UserId == userId) &&
                        c.Type == TransactionType.Expense &&
                        !existingCategoryIds.Contains(c.Id))
            .OrderBy(c => c.Name)
            .Select(c => new SelectListItem
            {
                Value = c.Id.ToString(),
                Text = c.Name
            })
            .ToListAsync();
    }
}
