using System.ComponentModel.DataAnnotations;

public class AddCreditViewModel
{
    [Required(ErrorMessage = "مبلغ الزامی است")]
    [Range(1000, 10000000, ErrorMessage = "مبلغ باید بین 1,000 تا 10,000,000 تومان باشد")]
    public decimal Amount { get; set; }

    [StringLength(200, ErrorMessage = "توضیحات نمی‌تواند بیشتر از 200 کاراکتر باشد")]
    public string Description { get; set; }
}