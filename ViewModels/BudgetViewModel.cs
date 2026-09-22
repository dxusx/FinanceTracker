using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace FinanceTracker.ViewModels;

public class BudgetViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Выберите категорию")]
    [Display(Name = "Категория расхода")]
    public int CategoryId { get; set; }

    [Required(ErrorMessage = "Укажите сумму лимита")]
    [Range(0.01, 100000000.00, ErrorMessage = "Лимит должен быть положительным числом")]
    [Display(Name = "Месячный лимит")]
    public decimal MonthlyLimit { get; set; }

    [Display(Name = "Месяц")]
    public int Month { get; set; } = DateTime.Today.Month;

    [Display(Name = "Год")]
    public int Year { get; set; } = DateTime.Today.Year;

    public List<SelectListItem> CategoryOptions { get; set; } = new();
}

public class BudgetListViewModel
{
    public int Month { get; set; } = DateTime.Today.Month;
    public int Year { get; set; } = DateTime.Today.Year;
    public decimal TotalBudget { get; set; }
    public decimal TotalSpent { get; set; }
    public decimal TotalRemaining => TotalBudget - TotalSpent;
    public List<BudgetProgressDto> Budgets { get; set; } = new();
    public string CurrencySymbol { get; set; } = "₽";
}
