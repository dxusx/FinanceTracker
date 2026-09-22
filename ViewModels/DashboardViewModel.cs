using FinanceTracker.Models;

namespace FinanceTracker.ViewModels;

public class DashboardViewModel
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string SelectedPeriod { get; set; } = "this_month";

    public decimal TotalIncome { get; set; }
    public decimal TotalExpense { get; set; }
    public decimal NetBalance => TotalIncome - TotalExpense;

    public string CurrencySymbol { get; set; } = "₽";

    public List<Transaction> RecentTransactions { get; set; } = new();

    public List<CategorySummaryDto> CategoryExpenses { get; set; } = new();
    public List<CategorySummaryDto> CategoryIncomes { get; set; } = new();

    public List<string> MonthlyLabels { get; set; } = new();
    public List<decimal> MonthlyIncomes { get; set; } = new();
    public List<decimal> MonthlyExpenses { get; set; } = new();

    public List<BudgetProgressDto> BudgetProgresses { get; set; } = new();
}

public class CategorySummaryDto
{
    public string CategoryName { get; set; } = string.Empty;
    public string Icon { get; set; } = "bi-tag";
    public string ColorHex { get; set; } = "#0d6efd";
    public decimal Amount { get; set; }
    public double Percentage { get; set; }
}

public class BudgetProgressDto
{
    public int BudgetId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public string Icon { get; set; } = "bi-tag";
    public string ColorHex { get; set; } = "#0d6efd";
    public decimal Limit { get; set; }
    public decimal Spent { get; set; }
    public decimal Remaining => Limit - Spent;
    public double Percentage => Limit > 0 ? Math.Round((double)(Spent / Limit) * 100, 1) : 0;
    public bool IsOverBudget => Spent > Limit;
}
