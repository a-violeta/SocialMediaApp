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

            /*
            if (value is IFormFile file)
            {
                return ValidateFile(file);
            }
            */

            if (value is ICollection<IFormFile> files)
            {
                if (files.Count == 0)
                    return ValidationResult.Success;

                foreach (var file in files)
                {
                    if (!types.Contains(file.ContentType))
                    {
                        return new ValidationResult(
                            "Allowed file types: PNG, JPEG, GIF, WebP, BMP."
                        );
                    }
                }
                return ValidationResult.Success;
            }

            if (value is IFormFile f)
            {
                if (!types.Contains(f.ContentType))
                {
                    return new ValidationResult(
                        "Allowed file types: PNG, JPEG, GIF, WebP, BMP."
                    );
                }
                return ValidationResult.Success;
            }
            return ValidationResult.Success;
        }
        /*
        private ValidationResult? ValidateFile(IFormFile file)
        {
            if (types.Contains(file.ContentType))
            {
                return ValidationResult.Success;
            }
            return new ValidationResult("Allowed file types: PNG, JPEG, GIF, WebP, BMP.");
        }
        */
    }
}
