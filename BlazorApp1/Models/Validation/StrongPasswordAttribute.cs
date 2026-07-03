using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace BlazorApp1.Models.Validation;

public partial class StrongPasswordAttribute : ValidationAttribute
{
    public StrongPasswordAttribute()
    {
        ErrorMessage = "Password must be at least 8 characters and include an uppercase letter, a lowercase letter, and a number.";
    }

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        var password = value as string ?? "";

        return PasswordRegex().IsMatch(password)
            ? ValidationResult.Success
            : new ValidationResult(ErrorMessage);
    }

    [GeneratedRegex(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).{8,}$")]
    private static partial Regex PasswordRegex();
}
