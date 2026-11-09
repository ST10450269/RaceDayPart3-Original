namespace RaceDay.Contracts;

/// <summary>Applies the RaceDay registration password requirements.</summary>
public static class PasswordPolicy
{
    public const int MinimumLength = 12;

    public static bool IsStrong(string? password)
    {
        if (string.IsNullOrWhiteSpace(password) || password.Length < MinimumLength)
            return false;

        return password.Any(char.IsUpper)
            && password.Any(char.IsLower)
            && password.Any(char.IsDigit)
            && password.Any(c => !char.IsLetterOrDigit(c) && !char.IsWhiteSpace(c));
    }
}
