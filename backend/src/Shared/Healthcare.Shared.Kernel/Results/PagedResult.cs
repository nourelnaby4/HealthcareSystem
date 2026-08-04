namespace Healthcare.Shared.Kernel.Results;
public sealed class PagedResult<T>
{
    public IReadOnlyList<T> Items { get; }
    public int Page { get; }
    public int PageSize { get; }
    public long TotalCount { get; }

    public int TotalPages => PageSize > 0
        ? (int)Math.Ceiling(TotalCount / (double)PageSize)
        : 0;

    public bool HasNext => Page < TotalPages;
    public bool HasPrevious => Page > 1;

    public PagedResult(IReadOnlyList<T> items, int page, int pageSize, long totalCount)
    {
        Items = items ?? Array.Empty<T>();
        Page = page;
        PageSize = pageSize;
        TotalCount = totalCount;
    }

    public static PagedResult<T> Empty(int page = 1, int pageSize = 20)
        => new(Array.Empty<T>(), page, pageSize, 0);
}
