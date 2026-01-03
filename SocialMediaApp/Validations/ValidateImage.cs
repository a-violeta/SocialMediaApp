using System.ComponentModel.DataAnnotations;

namespace SocialMediaApp.Validations
{
    public class ValidateImage : ValidationAttribute
    {
        private static readonly string[] types =
        {
            "image/jpeg",
            "image/png",
            "image/gif",
            "image/webp",
            "image/bmp"
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

            return new ValidationResult("Allowed file types: PNG, JPEG, GIF, WebP, BMP.");
        }

        private ValidationResult? ValidateFile(IFormFile file)
        {
            if (types.Contains(file.ContentType))
            {
                return ValidationResult.Success;
            }
            return new ValidationResult("Allowed file types: PNG, JPEG, GIF, WebP, BMP.");
        }
    }
}
