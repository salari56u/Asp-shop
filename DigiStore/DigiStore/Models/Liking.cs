using System.ComponentModel.DataAnnotations;

namespace DigiStore.Models
{
    public class Liking
    {
        [Key]
        public int id { get; set; }

        public int UserId { get; set; }

        public int productId { get; set; }
    }
}
