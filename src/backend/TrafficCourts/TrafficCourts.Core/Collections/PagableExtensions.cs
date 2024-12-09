using X.PagedList;

namespace TrafficCourts.Collections;

public static class PagableExtensions
{
    public static IPagedList<T> Page<T>(this IQueryable<T> items, IPagable? parameters, int defaultPageSize = 25)
    {
        int page = parameters?.PageNumber ?? 1;
        int size = parameters?.PageSize ?? defaultPageSize;

        var paged = new PagedList<T>(items, page, size);
        return paged;
    }

    /// <summary>
    /// Gets the offset rows as a string.
    /// </summary>
    /// <param name="pagable"></param>
    /// <returns></returns>
    public static string GetOffsetRows(this IPagable pagable)
    {
        int pageSize = pagable.PageSize ?? 25;
        int pageNumber = pagable.PageNumber ?? 1;
        string offset = ((pageNumber - 1) * pageSize).ToString();
        return offset;
    }

    /// <summary>
    /// Gets the fetch row count as a string.
    /// </summary>
    /// <param name="pagable"></param>
    /// <returns></returns>
    public static string GetFetchRows(this IPagable pagable)
    {
        int pageSize = pagable.PageSize ?? 25;
        return pageSize.ToString();
    }

}