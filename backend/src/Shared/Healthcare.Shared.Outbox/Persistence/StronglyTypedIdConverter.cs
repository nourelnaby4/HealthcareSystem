using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Healthcare.Shared.Kernel.Ids;

namespace Healthcare.Shared.Outbox.Persistence;

/// <summary>
/// EF Core value converter that maps a <see cref="StronglyTypedId{TValue}"/> backed by a
/// <see cref="Guid"/> to/from its primitive database representation. Strongly-typed IDs are
/// persistence-ignorant; this conversion lives in the reliability kernel so every module maps
/// them identically.
/// </summary>
public sealed class StronglyTypedIdConverter<TId> : ValueConverter<TId, Guid>
    where TId : StronglyTypedId<Guid>
{
    public StronglyTypedIdConverter()
        : base(id => id.Value, value => Create(value))
    {
    }

    private static TId Create(Guid value)
    {
        var instance = Activator.CreateInstance(typeof(TId), value);
        return instance is null
            ? throw new InvalidOperationException($"Could not create an instance of {typeof(TId).Name}.")
            : (TId)instance;
    }
}
