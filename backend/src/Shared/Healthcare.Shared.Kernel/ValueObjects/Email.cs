using System.Text.RegularExpressions;

namespace Healthcare.Shared.Kernel.ValueObjects;

/// <summary>
/// A normalized email address. Stored lower-cased; validated with a pragmatic RFC-ish pattern.
/// </summary>
public sealed partial record Email
{
    private const int MaxLength = 256;

    public string Value { get; }

    private Email(string value) => Value = value;

    public static Email Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Email is required.", nameof(value));
        }

        var normalized = value.Trim().ToLowerInvariant();

        if (normalized.Length > MaxLength)
        {
            throw new ArgumentException($"Email must be {MaxLength} characters or fewer.", nameof(value));
        }

        if (!EmailPattern().IsMatch(normalized))
        {
            throw new ArgumentException("Email format is invalid.", nameof(value));
        }

        return new Email(normalized);
    }

    public static bool TryCreate(string? value, out Email? email)
    {
        try
        {
            email = Create(value ?? string.Empty);
            return true;
        }
        catch (ArgumentException)
        {
            email = null;
            return false;
        }
    }

    public override string ToString() => Value;

    [GeneratedRegex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.IgnoreCase)]
    private static partial Regex EmailPattern();
}
