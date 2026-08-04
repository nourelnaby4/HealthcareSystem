namespace Healthcare.Shared.Kernel.Ids;

public abstract record Id(Guid Value)
{
    protected static Guid NewValue() => Guid.CreateVersion7();

    public override string ToString() => Value.ToString();
}
