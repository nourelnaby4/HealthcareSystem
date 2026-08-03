using System.Text.RegularExpressions;

namespace Healthcare.Shared.Kernel.ValueObjects;

/// <summary>
/// A Medical Record Number (MRN). System-generated, immutable, and unique. Format:
/// <c>&lt;FACILITY-CODE&gt;-NNNNNN&lt;CHECK&gt;</c> where the trailing check digit is a Luhn-style
/// mod-11 checksum that guards against transcription errors. Validation here is structural;
/// uniqueness is guaranteed by a database unique index.
/// </summary>
public sealed partial record Mrn
{
    public string Value { get; }

    private Mrn(string value) => Value = value;

    /// <summary>Constructs an MRN from an already-formatted, validated value (e.g. from the database).</summary>
    public static Mrn FromStored(string value)
    {
        if (string.IsNullOrWhiteSpace(value) || !MrnPattern().IsMatch(value))
        {
            throw new ArgumentException("MRN format is invalid.", nameof(value));
        }

        return new Mrn(value);
    }

    public override string ToString() => Value;

    [GeneratedRegex(@"^[A-Z0-9]{2,16}-\d{6}[0-9]$")]
    private static partial Regex MrnPattern();
}
