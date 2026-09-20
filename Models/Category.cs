using System.ComponentModel.DataAnnotations;
using FinanceTracker.Models.Enums;

namespace FinanceTracker.Models;

public class Category
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Введите название категории")]
    [StringLength(50, MinimumLength = 2, ErrorMessage = "Длина названия от 2 до 50 символов")]
    [Display(Name = "Название")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Выберите тип категории")]
    [Display(Name = "Тип операции")]
    public TransactionType Type { get; set; }

    [Display(Name = "Иконка")]
    public string Icon { get; set; } = "bi-tag";

    [Display(Name = "Цвет")]
    public string ColorHex { get; set; } = "#0d6efd";

    public string? UserId { get; set; }
    public ApplicationUser? User { get; set; }

    public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
    public ICollection<Budget> Budgets { get; set; } = new List<Budget>();
}
