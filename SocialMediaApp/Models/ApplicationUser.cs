using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Identity.Client;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SocialMediaApp.Models
{
    public class ApplicationUser : IdentityUser
    {
        
        [Required(ErrorMessage = "First name is required.")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Last name is required.")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "Setting profile visibility is required.")]
        [RegularExpression(@"^(public|private)$",
            ErrorMessage = "Profile can only be public or private.")]
        public string ProfileVisibility { get; set; }

        [Required(ErrorMessage = "Profile description is required.")]
        public string Description { get; set; }

        [Required(ErrorMessage = "Profile picture is required.")]
        public string ProfilePicture { get; set; }


        // Proprietati de navigatie: 8
        public virtual ICollection<Post> Posts { get; set; } = [];
        public virtual ICollection<Comment> Comments { get; set; } = [];

        public virtual ICollection<Likes> Likes { get; set; } = [];

        public virtual ICollection<GroupMessage> Messages { get; set; } = [];

        public virtual ICollection<GroupUser> Groups { get; set; } = [];

        public virtual ICollection<Follows> Follows { get; set; } = [];

        public virtual ICollection<Follows> Followers { get; set; } = [];

        public virtual ICollection<GroupJoinRequest> JoinRequests { get; set; } = [];
    }
}
