using System.ComponentModel.DataAnnotations;

namespace DigiStore.Models
{
    public class Banner
    {
        public int Id { get; set; }

        [Required]
        public string ImageName { get; set; }

        public string? Link { get; set; }

        public string Position { get; set; } 

        public bool IsActive { get; set; } = true;
    }
}