using System.ComponentModel.DataAnnotations;

namespace SocialMediaApp.ViewModels
{
    public class EditProfileViewModel
    {
        [Required(ErrorMessage = "First name is required")]
        public string? FirstName { get; set; }

        [Required(ErrorMessage = "Last name is required")]
        public string? LastName { get; set; }

        [Required(ErrorMessage = "Setting profile visibility is required")]
        [RegularExpression(@"^(public|private)$",
            ErrorMessage = "The profile can only be public or private")]
        public string? ProfileVisibility { get; set; }

        [Required(ErrorMessage = "Setting a profile description is required")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "A profile picture is required")]
        public string? ExistingProfilePicture { get; set; }

        public IFormFile? ProfilePicture { get; set; } = null;
    }
}
