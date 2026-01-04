using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SocialMediaApp.Models
{
    public class Likes
    {
        [Key]
        public int Id { get; set; }  // cheie primară, necesară pentru EF

        [Required]
        public string? UserId { get; set; }

        [Required]
        public int? PostId { get; set; } = null!;

        public DateTime LikeDate { get; set; } = DateTime.Now;

        // Proprietati de navigatie: 2

        public virtual ApplicationUser? User { get; set; }

        public virtual Post? Post { get; set; }
    }
}
