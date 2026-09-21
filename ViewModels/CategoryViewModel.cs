using System.ComponentModel.DataAnnotations;
using FinanceTracker.Models.Enums;

namespace FinanceTracker.ViewModels;

public class CategoryViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Введите название категории")]
    [StringLength(50, MinimumLength = 2, ErrorMessage = "Название должно содержать от 2 до 50 символов")]
    [Display(Name = "Название")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Выберите тип категории")]
    [Display(Name = "Тип")]
    public TransactionType Type { get; set; } = TransactionType.Expense;

    [Required(ErrorMessage = "Выберите иконку")]
    [Display(Name = "Иконка")]
    public string Icon { get; set; } = "bi-tag";

    [Required(ErrorMessage = "Выберите цвет")]
    [Display(Name = "Цвет")]
    public string ColorHex { get; set; } = "#0d6efd";

    public bool IsSystemCategory { get; set; }
}
