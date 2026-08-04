namespace Healthcare.Shared.Kernel.ValueObjects;

public sealed record DateOfBirth
{
    private const int MaxAgeYears = 130;

    public DateOnly Value { get; }

    private DateOfBirth(DateOnly value) => Value = value;

    public static DateOfBirth Create(DateOnly value)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

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
