using System.ComponentModel.DataAnnotations;

namespace SocialMediaApp.Models
{
    public class Group
    {
        [Key]
        public int Id { get; set; }
        [Required(ErrorMessage = "Grupul trebuie să aibă un nume")]
        public string? Name { get; set; }
        [Required(ErrorMessage = "Grupul trebuie să aibă o descriere")]
        public string? Description { get; set; }

        // Proprietati de navigatie: 3

        public virtual ICollection<GroupUser> Users { get; set; } = [];

        public virtual ICollection<GroupMessage> Messages { get; set; } = [];

        public virtual ICollection<GroupJoinRequest> JoinRequests { get; set; } = [];
    }
}
