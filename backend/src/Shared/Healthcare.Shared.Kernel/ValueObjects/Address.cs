namespace Healthcare.Shared.Kernel.ValueObjects;

/// <summary>
/// A postal address. Every component is bounded in length; street/city/country are required
/// when an address is supplied (state/postal code are optional to support international addresses).
/// </summary>
public sealed record Address
{
    private const int ComponentMaxLength = 128;
    private const int CountryMaxLength = 2;

    public string Street { get; }
    public string City { get; }
    public string? State { get; }
    public string? PostalCode { get; }
    public string Country { get; }

    private Address(string street, string city, string? state, string? postalCode, string country)
    {
        Street = street;
        City = city;
        State = state;
        PostalCode = postalCode;
        Country = country;
    }

    public static Address Create(string street, string city, string? state, string? postalCode, string country)
    {
        if (string.IsNullOrWhiteSpace(country))
        {
            throw new ArgumentException("Country is required.", nameof(country));
        }

        var normalizedCountry = country.Trim().ToUpperInvariant();
        if (normalizedCountry.Length is < 2 or > CountryMaxLength)
        {
            throw new ArgumentException("Country must be a 2-letter ISO code (e.g. EG).", nameof(country));
        }

        return new Address(
            street: RequireAndBound(street, nameof(street)),
            city: RequireAndBound(city, nameof(city)),
            state: BoundOptional(state),
            postalCode: BoundOptional(postalCode),
            country: normalizedCountry);
    }

    public static Address? CreateOrDefault(string? street, string? city, string? state, string? postalCode, string? country)
    {
        var hasAny = !string.IsNullOrWhiteSpace(street)
                     || !string.IsNullOrWhiteSpace(city)
                     || !string.IsNullOrWhiteSpace(state)
                     || !string.IsNullOrWhiteSpace(postalCode)
                     || !string.IsNullOrWhiteSpace(country);

        return hasAny
            ? Create(street ?? string.Empty, city ?? string.Empty, state, postalCode, country ?? string.Empty)
            : null;
    }

    private static string RequireAndBound(string? value, string paramName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException($"{paramName} is required.", paramName);
        }

        var trimmed = value.Trim();
        return trimmed.Length > ComponentMaxLength
            ? throw new ArgumentException($"{paramName} must be {ComponentMaxLength} characters or fewer.", paramName)
            : trimmed;
    }

    private static string? BoundOptional(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var trimmed = value.Trim();
        return trimmed.Length > ComponentMaxLength
            ? throw new ArgumentException($"Address component must be {ComponentMaxLength} characters or fewer.")
            : trimmed;
    }

    public override string ToString() =>
        $"{Street}, {City}{(State is null ? string.Empty : ", " + State)} {PostalCode}, {Country}";
}
