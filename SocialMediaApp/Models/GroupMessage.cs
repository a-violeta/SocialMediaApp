using System.ComponentModel.DataAnnotations;

namespace SocialMediaApp.Models
{
    public class GroupMessage
    {
        [Key]
        public int Id { get; set; }

        public int GroupId { get; set; }

        public string? UserId { get; set; }

        public string TextContent { get; set; }

        //adaugare data mesajului, nu am folosit o decat in messages view
        public DateTime CreatedAt { get; set; } = DateTime.Now;


        // Proprietati de navigatie: 2

        public virtual Group? Group { get; set; }
        
        public virtual ApplicationUser? User { get; set; }
    }
}
