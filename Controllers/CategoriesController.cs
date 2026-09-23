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
public class CategoriesController : Controller
{
    private readonly ApplicationDbContext _context;

    public CategoriesController(ApplicationDbContext context)
    {
        _context = context;
    }

    private string CurrentUserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

    public async Task<IActionResult> Index()
    {
        var userId = CurrentUserId;
        // подгружаем категории пользователя и общие дефолтные
        List<Category> categories = await _context.Categories
            .Where(c => c.UserId == null || c.UserId == userId)
            .Include(c => c.Transactions.Where(t => t.UserId == userId))
            .OrderBy(c => c.Type)
            .ThenBy(c => c.Name)
            .ToListAsync();

        return View(categories);
    }

    [HttpGet]
    public IActionResult Create(TransactionType type = TransactionType.Expense)
    {
        var model = new CategoryViewModel
        {
            Type = type,
            Icon = type == TransactionType.Income ? "bi-cash-coin" : "bi-cart3",
            ColorHex = type == TransactionType.Income ? "#198754" : "#dc3545"
        };
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CategoryViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var userId = CurrentUserId;

        var exists = await _context.Categories.AnyAsync(c =>
            (c.UserId == null || c.UserId == userId) &&
            c.Type == model.Type &&
            c.Name.ToLower() == model.Name.Trim().ToLower());

        if (exists)
        {
            ModelState.AddModelError(nameof(model.Name), "Категория с таким названием уже существует.");
            return View(model);
        }

        var category = new Category
        {
            Name = model.Name.Trim(),
            Type = model.Type,
            Icon = model.Icon,
            ColorHex = model.ColorHex,
            UserId = userId
        };

        _context.Categories.Add(category);
        await _context.SaveChangesAsync();

        TempData["Success"] = "Категория успешно создана!";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var userId = CurrentUserId;
        var category = await _context.Categories.FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId);

        if (category == null)
        {
            TempData["Error"] = "Системные категории нельзя редактировать или категория не найдена.";
            return RedirectToAction(nameof(Index));
        }

        var model = new CategoryViewModel
        {
            Id = category.Id,
            Name = category.Name,
            Type = category.Type,
            Icon = category.Icon,
            ColorHex = category.ColorHex,
            IsSystemCategory = category.UserId == null
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, CategoryViewModel model)
    {
        if (id != model.Id)
        {
            return BadRequest();
        }

        var userId = CurrentUserId;
        var category = await _context.Categories.FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId);

        if (category == null)
        {
            TempData["Error"] = "Категория не найдена или является защищенной системной.";
            return RedirectToAction(nameof(Index));
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        category.Name = model.Name.Trim();
        category.Type = model.Type;
        category.Icon = model.Icon;
        category.ColorHex = model.ColorHex;

        _context.Update(category);
        await _context.SaveChangesAsync();

        TempData["Success"] = "Категория обновлена!";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var userId = CurrentUserId;
        var category = await _context.Categories
            .Include(c => c.Transactions)
            .FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId);

        if (category == null)
        {
            TempData["Error"] = "Системную категорию нельзя удалить.";
            return RedirectToAction(nameof(Index));
        }

        if (category.Transactions.Any())
        {
            TempData["Error"] = "Невозможно удалить категорию, так как по ней уже совершены финансовые операции. Сначала удалите или перенесите операции.";
            return RedirectToAction(nameof(Index));
        }

        _context.Categories.Remove(category);
        await _context.SaveChangesAsync();

        TempData["Success"] = "Категория успешно удалена.";
        return RedirectToAction(nameof(Index));
    }
}
