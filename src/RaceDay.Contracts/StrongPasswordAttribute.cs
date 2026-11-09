using System.ComponentModel.DataAnnotations;

namespace RaceDay.Contracts;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter)]
public sealed class StrongPasswordAttribute : ValidationAttribute
{
    public StrongPasswordAttribute()
        : base("Password must be at least 12 characters and include upper/lowercase letters, a number, and a symbol.")
    {
    }

    public override bool IsValid(object? value) => value is string text && PasswordPolicy.IsStrong(text);
}
