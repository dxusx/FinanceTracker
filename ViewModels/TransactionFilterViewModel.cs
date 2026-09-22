using FinanceTracker.Models;
using FinanceTracker.Models.Enums;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace FinanceTracker.ViewModels;

public class TransactionFilterViewModel
{
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public int? CategoryId { get; set; }
    public TransactionType? Type { get; set; }
    public string? SearchTerm { get; set; }

    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public int TotalItems { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalItems / PageSize);

    public decimal FilteredIncome { get; set; }
    public decimal FilteredExpense { get; set; }
    public decimal FilteredBalance => FilteredIncome - FilteredExpense;

    public List<Transaction> Transactions { get; set; } = new();
    public List<SelectListItem> CategoryOptions { get; set; } = new();
    public string CurrencySymbol { get; set; } = "₽";
}
