using Healthcare.Shared.Kernel.Domain;

namespace Healthcare.Shared.Kernel.ValueObjects;

/// <summary>
/// A date of birth. Must be in the past and within a plausible human lifespan (not older than
/// 130 years). Stored as <see cref="DateOnly"/> for database portability.
/// </summary>
public sealed record DateOfBirth
{
    private const int MaxAgeYears = 130;

    public DateOnly Value { get; }

    private DateOfBirth(DateOnly value) => Value = value;

    public static DateOfBirth Create(DateOnly value, IClock? clock = null)
    {
        var today = DateOnly.FromDateTime((clock ?? new SystemClock()).UtcNow.DateTime);

        if (value > today)
        {
            throw new ArgumentException("Date of birth cannot be in the future.", nameof(value));
        }

        if (value < today.AddYears(-MaxAgeYears))
        {
            throw new ArgumentException($"Date of birth cannot be more than {MaxAgeYears} years ago.", nameof(value));
        }

        return new DateOfBirth(value);
    }

    public override string ToString() => Value.ToString("yyyy-MM-dd");
}
