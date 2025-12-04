using System.ComponentModel.DataAnnotations;

namespace DigiStore.Models 
{
    public class Slider
    {
        public int Id { get; set; }

        [Required]
        public string ImageName { get; set; }

        public string? Title { get; set; } 

        public string? Link { get; set; } 

        public bool IsActive { get; set; } = true;

        public int SortOrder { get; set; } 
    }
}