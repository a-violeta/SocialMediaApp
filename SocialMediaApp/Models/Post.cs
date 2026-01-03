using System.ComponentModel.DataAnnotations;

namespace SocialMediaApp.Models
{
    public class Post
    {
        [Key]
        public int Id { get; set; }

        public string? TextContent { get; set; }

        public DateTime Date { get; set; }

        public string? UserId { get; set; }

        // Proprietati de navigatie: 5

        public virtual ApplicationUser? User { get; set; }

        public virtual ICollection<Comment> Comments { get; set; } = [];

        public virtual ICollection<Image> Images { get; set; } = [];

        public virtual ICollection<Video> Videos { get; set; } = [];

        public virtual ICollection<Likes> WhoLiked { get; set; } = [];
    }
}
