using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FinanceTracker.Models;

public class Budget
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Выберите категорию")]
    [Display(Name = "Категория")]
    public int CategoryId { get; set; }
    public Category? Category { get; set; }

    [Required(ErrorMessage = "Укажите лимит расходов")]
    [Range(0.01, 100000000.00, ErrorMessage = "Лимит должен быть больше 0")]
    [Column(TypeName = "decimal(18, 2)")]
    [Display(Name = "Месячный лимит")]
    public decimal MonthlyLimit { get; set; }

    [Required]
    [Range(1, 12)]
    public int Month { get; set; } = DateTime.Today.Month;

    [Required]
    [Range(2020, 2100)]
    public int Year { get; set; } = DateTime.Today.Year;

    [Required]
    public string UserId { get; set; } = string.Empty;
    public ApplicationUser? User { get; set; }
}
