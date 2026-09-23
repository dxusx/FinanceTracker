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
public class TransactionsController : Controller
{
    private readonly ApplicationDbContext _context;

    public TransactionsController(ApplicationDbContext context)
    {
        _context = context;
    }

    private string CurrentUserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

    public async Task<IActionResult> Index(
        DateTime? startDate,
        DateTime? endDate,
        int? categoryId,
        TransactionType? type,
        string? searchTerm,
        int page = 1,
        int pageSize = 10)
    {
        var userId = CurrentUserId;

        var query = _context.Transactions
            .Include(t => t.Category)
            .Where(t => t.UserId == userId);

        if (startDate.HasValue)
        {
            query = query.Where(t => t.Date >= startDate.Value.Date);
        }

        if (endDate.HasValue)
        {
            // Временный фикс для часового пояса
            var endOfDay = endDate.Value.Date.AddDays(1).AddTicks(-1);
            query = query.Where(t => t.Date <= endOfDay);
        }

        if (categoryId.HasValue && categoryId.Value > 0)
        {
            query = query.Where(t => t.CategoryId == categoryId.Value);
        }

        if (type.HasValue)
        {
            query = query.Where(t => t.Category != null && t.Category.Type == type.Value);
        }

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var search = searchTerm.Trim().ToLower();
            query = query.Where(t => (t.Note != null && t.Note.ToLower().Contains(search))
                                  || (t.Category != null && t.Category.Name.ToLower().Contains(search)));
        }

        List<Transaction> allFiltered = await query.ToListAsync();
        decimal filteredIncome = allFiltered.Where(t => t.Category?.Type == TransactionType.Income).Sum(t => t.Amount);
        decimal filteredExpense = allFiltered.Where(t => t.Category?.Type == TransactionType.Expense).Sum(t => t.Amount);
        int totalItems = allFiltered.Count;

        List<Transaction> transactions = allFiltered
            .OrderByDescending(t => t.Date)
            .ThenByDescending(t => t.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        List<SelectListItem> userCategories = await _context.Categories
            .Where(c => c.UserId == null || c.UserId == userId)
            .OrderBy(c => c.Type)
            .ThenBy(c => c.Name)
            .Select(c => new SelectListItem
            {
                Value = c.Id.ToString(),
                Text = $"{(c.Type == TransactionType.Income ? "[+] " : "[-] ")}{c.Name}",
                Selected = categoryId == c.Id
            })
            .ToListAsync();

        var model = new TransactionFilterViewModel
        {
            StartDate = startDate,
            EndDate = endDate,
            CategoryId = categoryId,
            Type = type,
            SearchTerm = searchTerm,
            Page = page,
            PageSize = pageSize,
            TotalItems = totalItems,
            FilteredIncome = filteredIncome,
            FilteredExpense = filteredExpense,
            Transactions = transactions,
            CategoryOptions = userCategories
        };

        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> Create(TransactionType? type = null)
    {
        var model = new TransactionFormViewModel
        {
            Date = DateTime.Today,
            Type = type ?? TransactionType.Expense,
            CategoryOptions = await GetCategorySelectListAsync(type ?? TransactionType.Expense)
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(TransactionFormViewModel model)
    {
        var userId = CurrentUserId;

        var category = await _context.Categories.FirstOrDefaultAsync(c => c.Id == model.CategoryId && (c.UserId == null || c.UserId == userId));
        if (category == null)
        {
            ModelState.AddModelError(nameof(model.CategoryId), "Выбранная категория не найдена.");
        }

        // Проверяем, чтобы расход не ушел в минус
        if (model.Amount <= 0)
        {
            ModelState.AddModelError(nameof(model.Amount), "Сумма должна быть больше нуля");
        }

        if (!ModelState.IsValid)
        {
            model.CategoryOptions = await GetCategorySelectListAsync(model.Type);
            return View(model);
        }

        var transaction = new Transaction
        {
            UserId = userId,
            CategoryId = model.CategoryId,
            Amount = model.Amount,
            Date = model.Date,
            Note = model.Note
        };

        _context.Transactions.Add(transaction);
        await _context.SaveChangesAsync();

        TempData["Success"] = "Операция успешно добавлена!";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var userId = CurrentUserId;
        var transaction = await _context.Transactions
            .Include(t => t.Category)
            .FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);

        if (transaction == null)
        {
            return NotFound();
        }

        var model = new TransactionFormViewModel
        {
            Id = transaction.Id,
            Amount = transaction.Amount,
            Date = transaction.Date,
            Note = transaction.Note,
            CategoryId = transaction.CategoryId,
            Type = transaction.Category?.Type ?? TransactionType.Expense,
            CategoryOptions = await GetCategorySelectListAsync(transaction.Category?.Type ?? TransactionType.Expense)
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, TransactionFormViewModel model)
    {
        if (id != model.Id)
        {
            return BadRequest();
        }

        var userId = CurrentUserId;
        var transaction = await _context.Transactions.FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);
        if (transaction == null)
        {
            return NotFound();
        }

        var category = await _context.Categories.FirstOrDefaultAsync(c => c.Id == model.CategoryId && (c.UserId == null || c.UserId == userId));
        if (category == null)
        {
            ModelState.AddModelError(nameof(model.CategoryId), "Выбранная категория не найдена.");
        }

        if (!ModelState.IsValid)
        {
            model.CategoryOptions = await GetCategorySelectListAsync(model.Type);
            return View(model);
        }

        transaction.Amount = model.Amount;
        transaction.Date = model.Date;
        transaction.Note = model.Note;
        transaction.CategoryId = model.CategoryId;

        _context.Update(transaction);
        await _context.SaveChangesAsync();

        TempData["Success"] = "Операция успешно обновлена!";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var userId = CurrentUserId;
        var transaction = await _context.Transactions.FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);
        if (transaction != null)
        {
            _context.Transactions.Remove(transaction);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Операция удалена.";
        }

        return RedirectToAction(nameof(Index));
    }


    [HttpGet]
    public async Task<IActionResult> GetCategoriesByType(TransactionType type)
    {
        var list = await GetCategorySelectListAsync(type);
        return Json(list);
    }

    private async Task<List<SelectListItem>> GetCategorySelectListAsync(TransactionType type)
    {
        var userId = CurrentUserId;
        return await _context.Categories
            .Where(c => (c.UserId == null || c.UserId == userId) && c.Type == type)
            .OrderBy(c => c.Name)
            .Select(c => new SelectListItem
            {
                Value = c.Id.ToString(),
                Text = c.Name
            })
            .ToListAsync();
    }
}
