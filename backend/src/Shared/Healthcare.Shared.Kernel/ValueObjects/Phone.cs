using System.Text.RegularExpressions;

namespace Healthcare.Shared.Kernel.ValueObjects;

/// <summary>
/// A phone number in an E.164-ish representation. Only digits and <c>+ - ( )</c> / spaces are
/// permitted; length is bounded. No country normalization is performed in phase 1.
/// </summary>
public sealed partial record Phone
{
    private const int MaxLength = 20;

    public string Value { get; }

    private Phone(string value) => Value = value;

    public static Phone Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Phone is required.", nameof(value));
        }

        var trimmed = value.Trim();

        if (trimmed.Length > MaxLength)
        {
            throw new ArgumentException($"Phone must be {MaxLength} characters or fewer.", nameof(value));
        }

        if (!PhonePattern().IsMatch(trimmed))
        {
            throw new ArgumentException("Phone format is invalid.", nameof(value));
        }

        return new Phone(trimmed);
    }

    public override string ToString() => Value;

    [GeneratedRegex(@"^\+?[0-9][0-9\-\(\) ]{3,19}$")]
    private static partial Regex PhonePattern();
}
