using System.Text.RegularExpressions;

namespace Healthcare.Shared.Kernel.ValueObjects;

public sealed partial record Mrn
{
    public string Value { get; }

    private Mrn(string value) => Value = value;

    public static Mrn Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value) || !MrnPattern().IsMatch(value))
        {
            throw new ArgumentException("MRN format is invalid.", nameof(value));
        }

        return new Mrn(value);
    }

    public override string ToString() => Value;

    [GeneratedRegex(@"^MRN-\d{6,}$")]
    private static partial Regex MrnPattern();
}
