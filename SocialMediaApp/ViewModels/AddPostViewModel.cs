using Microsoft.IdentityModel.Tokens;
using System.ComponentModel.DataAnnotations;
using SocialMediaApp.Validations;

namespace SocialMediaApp.ViewModels
{
    public class AddPostViewModel : IValidatableObject
    {
        public string? TextContent { get; set; }

        [ValidateImage]
        [ValidateFileSize(5 * 1024  * 1024)]
        public ICollection<IFormFile> Images { get; set; } = [];

        [ValidateVideo]
        [ValidateFileSize(50 * 1024 * 1024)]
        public ICollection<IFormFile> Videos { get; set; } = [];

        //ID urile fisierelor existente ca sa stiu in edit cum sa le tratez
        public List<int>? ExistingImageIds { get; set; } = new();
        public List<int>? ExistingVideoIds { get; set; } = new();

        //pentru stergerea fisierelor existente, daca se vrea sa se scoata din postare la editarea postarii
        public List<int>? RemoveImageIds { get; set; } = new();
        public List<int>? RemoveVideoIds { get; set; } = new();

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (string.IsNullOrWhiteSpace(TextContent) &&
                Images.IsNullOrEmpty() &&
                Videos.IsNullOrEmpty())
            {
                yield return new ValidationResult("Post cannot be empty.");
            }
            yield return ValidationResult.Success;
        }
    }
}
