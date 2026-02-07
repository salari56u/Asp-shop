using System.ComponentModel.DataAnnotations;

public class UpdateReviewViewModel
{
    [Required]
    public int ReviewId { get; set; }

    [Range(1, 5, ErrorMessage = "امتیاز باید بین 1 تا 5 باشد")]
    public int? Rating { get; set; }

    [StringLength(1000, MinimumLength = 10, ErrorMessage = "نظر باید بین 10 تا 1000 کاراکتر باشد")]
    public string Comment { get; set; }
}
