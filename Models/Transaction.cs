using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FinanceTracker.Models;

public class Transaction
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Укажите сумму")]
    [Range(0.01, 100000000.00, ErrorMessage = "Сумма должна быть больше 0")]
    [Column(TypeName = "decimal(18, 2)")]
    [Display(Name = "Сумма")]
    public decimal Amount { get; set; }

    [Required(ErrorMessage = "Укажите дату операции")]
    [Display(Name = "Дата")]
    public DateTime Date { get; set; } = DateTime.Today;

    [StringLength(250, ErrorMessage = "Примечание не может быть длиннее 250 символов")]
    [Display(Name = "Примечание / Комментарий")]
    public string? Note { get; set; }

    [Required(ErrorMessage = "Выберите категорию")]
    [Display(Name = "Категория")]
    public int CategoryId { get; set; }
    public Category? Category { get; set; }

    [Required]
    public string UserId { get; set; } = string.Empty;
    public ApplicationUser? User { get; set; }
}
