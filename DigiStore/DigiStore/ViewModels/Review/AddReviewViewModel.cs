using System.ComponentModel.DataAnnotations;

public class AddReviewViewModel
{
    [Required(ErrorMessage = "لطفا امتیاز دهید")]
    [Range(1, 5, ErrorMessage = "امتیاز باید بین 1 تا 5 باشد")]
    public int Rating { get; set; }

    [Required(ErrorMessage = "لطفا نظر خود را بنویسید")]
    [StringLength(1000, ErrorMessage = "نظر نمی‌تواند بیشتر از 1000 کاراکتر باشد")]
    public string Comment { get; set; }

    public int ProductId { get; set; }
}