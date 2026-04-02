namespace BACKSGEDI.Domain.Common;

public record PagedRequest
{
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 10;

    public int Skip => (Page - 1) * PageSize;

    public int GetSanitizedPageSize(int maxPageSize = 100) => 
        PageSize > maxPageSize ? maxPageSize : (PageSize <= 0 ? 10 : PageSize);
}

public record PagedResponse<T>
{
    public IEnumerable<T> Items { get; init; } = [];
    public int TotalCount { get; init; }
    public int Page { get; init; }
    public int PageSize { get; init; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    public bool HasNextPage => Page < TotalPages;
    public bool HasPreviousPage => Page > 1;

    public static PagedResponse<T> Create(IEnumerable<T> items, int totalCount, int page, int pageSize)
    {
        return new PagedResponse<T>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }
}
