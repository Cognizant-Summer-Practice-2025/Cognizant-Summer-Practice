using System.ComponentModel.DataAnnotations;

namespace backend_portfolio.Models.Validation
{
    public class NotEmptyGuidAttribute : ValidationAttribute
    {
        public override bool IsValid(object? value)
        {
            if (value is Guid guid)
            {
                return guid != Guid.Empty;
            }
            return false;
        }

        public override string FormatErrorMessage(string name)
        {
            return $"{name} cannot be empty.";
        }
    }
} 