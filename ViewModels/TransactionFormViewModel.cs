using System.ComponentModel.DataAnnotations;
using FinanceTracker.Models.Enums;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace FinanceTracker.ViewModels;

public class TransactionFormViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Укажите сумму")]
    [Range(0.01, 100000000.00, ErrorMessage = "Сумма должна быть положительной (от 0.01 до 100 000 000)")]
    [Display(Name = "Сумма")]
    public decimal Amount { get; set; }

    [Required(ErrorMessage = "Выберите дату операции")]
    [DataType(DataType.Date)]
    [Display(Name = "Дата")]
    public DateTime Date { get; set; } = DateTime.Today;

    [StringLength(250, ErrorMessage = "Комментарий не должен превышать 250 символов")]
    [Display(Name = "Примечание / Комментарий")]
    public string? Note { get; set; }

    [Required(ErrorMessage = "Выберите категорию")]
    [Display(Name = "Категория")]
    public int CategoryId { get; set; }

    [Display(Name = "Тип")]
    public TransactionType Type { get; set; } = TransactionType.Expense;

    public List<SelectListItem> CategoryOptions { get; set; } = new();
}
