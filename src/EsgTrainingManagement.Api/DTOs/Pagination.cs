using Microsoft.EntityFrameworkCore;

namespace EsgTrainingManagement.Api.DTOs;

public class PaginationParams
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}

public record PagedResponse<T>(IReadOnlyList<T> Items, int PageNumber, int PageSize, int TotalCount)
{
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
}

public static class PaginationExtensions
{
    public static async Task<PagedResponse<T>> ToPageAsync<T>(this IQueryable<T> query, PaginationParams page,
        CancellationToken ct)
    {
        var count = await query.CountAsync(ct);
        var items = await query.Skip((page.PageNumber - 1) * page.PageSize).Take(page.PageSize).ToListAsync(ct);
        return new(items, page.PageNumber, page.PageSize, count);
    }
}