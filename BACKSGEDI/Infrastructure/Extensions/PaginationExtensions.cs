using Microsoft.EntityFrameworkCore;
using BACKSGEDI.Domain.Common;

namespace BACKSGEDI.Infrastructure.Extensions;

public static class PaginationExtensions
{
    public static async Task<PagedResponse<T>> ToPagedResponseAsync<T>(
        this IQueryable<T> query, 
        PagedRequest req, 
        CancellationToken ct = default)
    {
        var totalCount = await query.CountAsync(ct);

        var pageSize = req.GetSanitizedPageSize(maxPageSize: 50);
        var skip = (req.Page - 1) * pageSize; 

        var items = await query
            .Skip(skip)
            .Take(pageSize)
            .ToListAsync(ct);
        return PagedResponse<T>.Create(items, totalCount, req.Page, pageSize);
    }
}