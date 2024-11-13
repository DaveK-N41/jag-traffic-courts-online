using System.Text;

namespace TrafficCourts.Collections;

public static class SortableExtensions
{
    public static IQueryable<T> Sort<T>(this IQueryable<T> items, ISortable? parameters)
    {
        // TODO
        if (parameters?.SortBy is null || parameters.SortBy.Count == 0)
        {
            return items;
        }

        var sorted = items.OrderBy(_ => 0); // fake sort

        for (int i = 0; i < parameters.SortBy.Count; i++)
        {
            string sortBy = parameters.SortBy[i];
            SortDirection direction = GetSortDirection(parameters.SortDirection, i);

            sorted = direction == SortDirection.desc
                ? sorted.ThenByDescending(sortBy)
                : sorted.ThenBy(sortBy);
        }

        return sorted;
    }

    private static SortDirection GetSortDirection(List<SortDirection>? direction, int index)
    {
        if (direction is null) return SortDirection.asc;
        if (direction.Count <= index) return SortDirection.asc;
        return direction[index];
    }

    public static string GetSortBy(this ISortable sortable, IReadOnlyDictionary<string, IReadOnlyList<string>> mapping)
    {
        if (sortable.SortBy is not { Count: > 0 })
        {
            return string.Empty;
        }

        StringBuilder order = new StringBuilder();

        for (int i = 0; i < sortable.SortBy.Count; i++)
        {
            if (i > 0)
            {
                order.Append(',');
            }

            AddFields(order, mapping, sortable.SortBy[i], sortable.SortDirection?[i]);
        }

        return order.ToString();
    }

    private static void AddFields(StringBuilder order, IReadOnlyDictionary<string, IReadOnlyList<string>> mapping, string property, SortDirection? direction)
    {
        // some fields like name may want to sort on multiple fields: surname, given name
        if (mapping.TryGetValue(property, out IReadOnlyList<string>? fields))
        {
            for (int f = 0; f < fields.Count; f++)
            {
                if (f > 0)
                {
                    order.Append(',');
                }
                if (direction == SortDirection.desc)
                {
                    order.Append('-');
                }
                order.Append(fields[f]);
            }
        }
        else
        {
            throw new ArgumentException($"Invalid sort field: {property}");
        }
    }
}