using System.ComponentModel.DataAnnotations;

namespace DigiStore.Models.ViewModels
{
    public class UserEditViewModel
    {
        public int Id { get; set; }

        [Display(Name = "نام کاربری (موبایل)")]
        [Required(ErrorMessage = "لطفا {0} را وارد کنید")]
        public string UserName { get; set; }

        [Display(Name = "ایمیل")]
        [EmailAddress(ErrorMessage = "ایمیل معتبر نیست")]
        public string? Email { get; set; }

        [Display(Name = "کلمه عبور جدید")]
        // اینجا Required نداریم چون شاید نخواهد رمز را عوض کند
        public string? Password { get; set; }

        [Display(Name = "نقش کاربر")]
        public int RoleId { get; set; }
    }
}