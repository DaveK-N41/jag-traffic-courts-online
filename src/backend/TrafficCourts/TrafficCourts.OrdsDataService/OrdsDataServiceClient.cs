using System.Collections.Specialized;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using System.Web;

namespace TrafficCourts.OrdsDataService;

internal class OrdsDataServiceClient
{
    private readonly HttpClient _httpClient;
    private bool _readContentAsString = false;

    public OrdsDataServiceClient(HttpClient httpClient)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
    }

    public Task<T?> GetAsync<T>(
     string path,
     JsonTypeInfo<T> jsonTypeInfo,
     CancellationToken cancellationToken) => GetAsync(path, parameters: null, jsonTypeInfo, ETagCache.FiveMinutes, cancellationToken);

    public Task<T?> GetAsync<T>(
     string path,
     JsonTypeInfo<T> jsonTypeInfo,
     ETagCache cache,
     CancellationToken cancellationToken) => GetAsync(path, parameters: null, jsonTypeInfo, cache, cancellationToken);


    public async Task<T?> GetAsync<T>(
        string path,
        IReadOnlyDictionary<string, string>? parameters,
        JsonTypeInfo<T> jsonTypeInfo,
        ETagCache cache,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrEmpty(path);
        ArgumentNullException.ThrowIfNull(jsonTypeInfo);

        // build the path with query string parameters
        path = _httpClient.BaseAddress.AbsolutePath + GetPath(path, parameters);

        // create our request and set the request options for caching
        
        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, path);
        request.Options.Set(new HttpRequestOptionsKey<ETagCache>(nameof(ETagCache)), cache);

        // send the request
        HttpResponseMessage response = await _httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();

        T? result = await ReadContentAsync(response, jsonTypeInfo, cancellationToken);

        return result;
    }

    private async Task<T?> ReadContentAsync<T>(HttpResponseMessage response, JsonTypeInfo<T> jsonTypeInfo, CancellationToken cancellationToken)
    {
        T? result;

        if (_readContentAsString)
        {
            // used for debugging
            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            result = JsonSerializer.Deserialize(content, jsonTypeInfo);
        }
        else
        {
            await using var responseStream = await response.Content.ReadAsStreamAsync(cancellationToken);
            result = await JsonSerializer.DeserializeAsync<T>(responseStream, jsonTypeInfo, cancellationToken);
        }

        return result;
    }

    /// <summary>
    /// Builds the path with the query string parameters
    /// </summary>
    /// <param name="path"></param>
    /// <param name="parameters"></param>
    /// <returns></returns>
    private static string GetPath(string path, IReadOnlyDictionary<string, string>? parameters)
    {
        if (parameters != null && parameters.Count > 0)
        {
            var query = BuildQuery(parameters);
            char separator = path.Contains('?') ? '&' : '?';
            path = $"{path}{separator}{query}";
        }

        return path;
    }

    /// <summary>
    /// Builds the query string with encoded key values
    /// </summary>
    /// <param name="parameters"></param>
    /// <returns></returns>
    private static NameValueCollection BuildQuery(IReadOnlyDictionary<string, string> parameters)
    {
        // by using HttpUtility.ParseQueryString, the resulting query string will be properly encoded
        NameValueCollection query = HttpUtility.ParseQueryString(string.Empty);

        foreach (var parameter in parameters.OrderBy(_ => _.Key))
        {
            query[parameter.Key] = parameter.Value;
        }

        return query;
    }
}
