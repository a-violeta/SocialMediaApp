using System.ComponentModel.DataAnnotations;

namespace SocialMediaApp.Validations
{
    public class ValidateVideo : ValidationAttribute
    {
        private readonly string[] types =
        {
            "video/mp4",
            "video/webm",
            "video/ogg",
            "video/quicktime"
        };
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value == null)
            {
                return ValidationResult.Success;
            }

            if (value is IFormFile file)
            {
                return ValidateFile(file);
            }

            if (value is ICollection<IFormFile> files)
            {
                foreach (IFormFile f in files)
                {
                    ValidationResult? result = ValidateFile(f);
                    if (ValidateFile(f) != ValidationResult.Success)
                    {
                        return ValidateFile(f);
                    }
                    return ValidationResult.Success;
                }
            }

            return new ValidationResult("Allowed file types: MP4, WebM, OGG, MOV");
        }

        private ValidationResult? ValidateFile(IFormFile file)
        {
            if (types.Contains(file.ContentType))
            {
                return ValidationResult.Success;
            }
            return new ValidationResult("Allowed file types: MP4, WebM, OGG, MOV");
        }
    }
}
