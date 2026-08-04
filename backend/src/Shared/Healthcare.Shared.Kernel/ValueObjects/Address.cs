namespace Healthcare.Shared.Kernel.ValueObjects;

public sealed record Address
{
    private const int MaxLength = 128;

    public string Street { get; }
    public string City { get; }
    public string? State { get; }
    public string? PostalCode { get; }
    public string Country { get; }

    private Address(string street, string city, string? state, string? postalCode, string country)
        => (Street, City, State, PostalCode, Country) = (street, city, state, postalCode, country);

    public static Address Create(string street, string city, string? state, string? postalCode, string country)
        => new(Field(street), Field(city), Optional(state), Optional(postalCode), NormalizeCountry(country));

    public static Address? CreateOrDefault(string? street, string? city, string? state, string? postalCode, string? country)
        => AnyProvided(street, city, state, postalCode, country)
            ? Create(street ?? "", city ?? "", state, postalCode, country ?? "")
            : null;

    private static string Field(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Value is required.");
        }

        return Bound(value!);
    }

    private static string? Optional(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : Bound(value!);

    private static string Bound(string value)
    {
        var trimmed = value.Trim();
        return trimmed.Length > MaxLength
            ? throw new ArgumentException($"Value must be {MaxLength} characters or fewer.")
            : trimmed;
    }

    private static string NormalizeCountry(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Country is required.");
        }

        var normalized = value.Trim().ToUpperInvariant();
        return normalized.Length == 2
            ? normalized
            : throw new ArgumentException("Country must be a 2-letter ISO code (e.g. EG).");
    }

    private static bool AnyProvided(params string?[] values) =>
        Array.Exists(values, v => !string.IsNullOrWhiteSpace(v));

    public override string ToString() =>
        $"{Street}, {City}{(State is null ? "" : ", " + State)} {PostalCode}, {Country}";
}
