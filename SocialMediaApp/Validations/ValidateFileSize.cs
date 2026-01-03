using System.ComponentModel.DataAnnotations;

namespace SocialMediaApp.Validations
{
    public class ValidateFileSize : ValidationAttribute
    {
        private readonly int _maxBytes;

        public ValidateFileSize(int maxBytes)
        {
            _maxBytes = maxBytes;
        }

        protected override ValidationResult? IsValid(
        object? value,
        ValidationContext validationContext)
        {
            if (value == null)
                return ValidationResult.Success;

            if (value is IFormFile file)
            {
                return Validate(file);
            }

            if (value is IEnumerable<IFormFile> files)
            {
                foreach (var f in files)
                {
                    var result = Validate(f);
                    if (result != ValidationResult.Success)
                        return result;
                }
            }

            return ValidationResult.Success;
        }

        private ValidationResult? Validate(IFormFile file)
        {
            if (file.Length > _maxBytes)
            {
                return new ValidationResult(
                    $"File size must not exceed {_maxBytes / (1024 * 1024)} MB.");
            }

            return ValidationResult.Success;
        }
    }
}
