namespace TicketFlow.BLL.Validation;

using System.ComponentModel.DataAnnotations;
using TicketFlow.BLL.Helpers;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
public class NoXssAttribute : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is string str && InputSanitizer.ContainsMaliciousPayload(str))
        {
            return new ValidationResult(
                ErrorMessage ?? $"The field {validationContext.DisplayName} contains potentially unsafe HTML or script content.");
        }

        return ValidationResult.Success;
    }
}