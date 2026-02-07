using System.ComponentModel.DataAnnotations;

namespace DigiStore.Models.ViewModels
{
    public class UserCreateViewModel
    {
        [Display(Name = "نام کاربری (موبایل)")]
        [Required(ErrorMessage = "لطفا {0} را وارد کنید")]
        [MaxLength(11, ErrorMessage = "{0} باید 11 رقم باشد")]
        public string UserName { get; set; }

        [Display(Name = "ایمیل")]
        [EmailAddress(ErrorMessage = "ایمیل معتبر نیست")]
        public string? Email { get; set; }

        [Display(Name = "کلمه عبور")]
        [Required(ErrorMessage = "لطفا {0} را وارد کنید")]
        [MinLength(4, ErrorMessage = "{0} باید حداقل {1} کاراکتر باشد")]
        public string Password { get; set; }

        [Display(Name = "نقش کاربر")]
        [Required(ErrorMessage = "انتخاب نقش الزامی است")]
        public int RoleId { get; set; }
    }
}