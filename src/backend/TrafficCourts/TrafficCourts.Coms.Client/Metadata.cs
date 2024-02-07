﻿namespace TrafficCourts.Coms.Client;

internal static class Metadata
{
    private const string Id = "coms-id";
    private const string Name = "coms-name";
    private static readonly StringComparer _comparer= StringComparer.OrdinalIgnoreCase;

    public static bool IsInternal(string key)
    {
        return _comparer.Equals(key, Id) || _comparer.Equals(key, Name);
    }

    public static bool IsNotInternal(KeyValuePair<string, string> item) => !IsInternal(item.Key);
    public static bool IsInternal(KeyValuePair<string, string> item) => IsInternal(item.Key);
    public static bool IsName(string key) => _comparer.Equals(key, Name);

    /// <summary>
    /// Creates the Metadata dictionary from the source with the correct <see cref="IEqualityComparer"/>.
    /// </summary>
    public static IReadOnlyDictionary<string, string> Create(IEnumerable<KeyValuePair<string, string>>? source = null)
    {
        if (source is null)
        {
            return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        }

        return new Dictionary<string, string>(source.Where(IsNotInternal), StringComparer.OrdinalIgnoreCase);
    }
}
