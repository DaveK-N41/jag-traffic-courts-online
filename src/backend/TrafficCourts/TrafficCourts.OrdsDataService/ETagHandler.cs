using Microsoft.Extensions.Caching.Memory;
using System.Diagnostics.Metrics;

namespace TrafficCourts.OrdsDataService;

public enum ETagCachePolicy
{
    /// <summary>
    /// Cache the response and use If-None-Match header if we have the etag.
    /// </summary>
    Cache,
    /// <summary>
    /// Do not use If-None-Match on this request.
    /// </summary>
    NoCache
}

public record ETagCache
{
    public ETagCachePolicy Policy { get; init; }
    public TimeSpan Expiration { get; init; } = TimeSpan.Zero;

    public static readonly ETagCache NoCache = new ETagCache { Policy = ETagCachePolicy.NoCache };
    public static readonly ETagCache OneMinute = new ETagCache { Policy = ETagCachePolicy.Cache, Expiration = TimeSpan.FromMinutes(1) };
    public static readonly ETagCache FiveMinutes = new ETagCache { Policy = ETagCachePolicy.Cache, Expiration = TimeSpan.FromMinutes(5) };
    public static readonly ETagCache OneHour = new ETagCache { Policy = ETagCachePolicy.Cache, Expiration = TimeSpan.FromHours(1) };
    public static readonly ETagCache OneDay = new ETagCache { Policy = ETagCachePolicy.Cache, Expiration = TimeSpan.FromDays(1) };
}


public class ETagHandler : DelegatingHandler
{
    public const string NoCache = "no-cache";

    private readonly IMemoryCache _cache;

    private readonly Counter<long> _cacheHitCounter;
    private readonly Counter<long> _cacheMissCounter;

    public ETagHandler(IMemoryCache cache, IMeterFactory factory)
    {
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));

        var meter = factory.Create(OrdsDataServiceOperationMetrics.MeterName);
        _cacheHitCounter = meter.CreateCounter<long>("ordsdataservice_etag_cache_hits");
        _cacheMissCounter = meter.CreateCounter<long>("ordsdataservice_etag_cache_misses");
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (!RequestSupportsCaching(request))
        {
            return await base.SendAsync(request, cancellationToken);
        }

        // get this requests cache expiration time
        var expiration = GetAbsoluteExpiration(request);

        var cacheKey = $"etag-{request.RequestUri}";

        // add the If-None-Match header if we have a cached response
        if (_cache.TryGetValue(cacheKey, out (string ETag, HttpResponseMessage ResponseMessage, byte[] ResponseBody) cacheEntry))
        {
            request.Headers.IfNoneMatch.Add(new System.Net.Http.Headers.EntityTagHeaderValue(cacheEntry.ETag));
        }

        var response = await base.SendAsync(request, cancellationToken);

        var pathTag = new KeyValuePair<string, object?>("path", request.RequestUri!.AbsolutePath);
        var expirationTag = new KeyValuePair<string, object?>("expiration-secs", (int)expiration.TotalSeconds);

        // already had the response
        if (response.StatusCode == System.Net.HttpStatusCode.NotModified)
        {
            _cacheHitCounter.Add(1, pathTag, expirationTag);
            response = CreateResponse(request, cacheEntry);
            return response;
        }
        else
        {
            _cacheMissCounter.Add(1, pathTag, expirationTag); // this same request was different than the cached response
        }

        if (response.Headers.ETag != null)
        {
            var responseBody = await response.Content.ReadAsByteArrayAsync(cancellationToken);
            
            // create response template
            var cachedResponse = new HttpResponseMessage(response.StatusCode)
            {
                Content = new ByteArrayContent([]),
                ReasonPhrase = response.ReasonPhrase,
                RequestMessage = request,
                Version = response.Version
            };

            foreach (var header in response.Headers)
            {
                cachedResponse.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }

            foreach (var header in response.Content.Headers)
            {
                cachedResponse.Content.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }

            var cacheEntryOptions = new MemoryCacheEntryOptions()
                .SetSize(responseBody.Length)           // Set the size of the cache entry
                .SetAbsoluteExpiration(expiration);     // Set absolute expiration time

            _cache.Set(cacheKey, (response.Headers.ETag.Tag, cachedResponse, responseBody));
        }

        return response;
    }

    private static HttpResponseMessage CreateResponse(HttpRequestMessage request, (string ETag, HttpResponseMessage ResponseMessage, byte[] ResponseBody) cacheEntry)
    {
        // create a new response using the cached entry as a template
        HttpResponseMessage response = new HttpResponseMessage(cacheEntry.ResponseMessage.StatusCode)
        {
            Content = new ByteArrayContent(cacheEntry.ResponseBody),
            ReasonPhrase = cacheEntry.ResponseMessage.ReasonPhrase,
            RequestMessage = request,
            Version = cacheEntry.ResponseMessage.Version,
        };

        foreach (var header in cacheEntry.ResponseMessage.Headers)
        {
            response.Headers.TryAddWithoutValidation(header.Key, header.Value);
        }

        foreach (var header in cacheEntry.ResponseMessage.Content.Headers)
        {
            response.Content.Headers.TryAddWithoutValidation(header.Key, header.Value);
        }

        return response;
    }

    private bool RequestSupportsCaching(HttpRequestMessage request)
    {
        // only GET requests support caching
        if (request.Method != HttpMethod.Get || request.RequestUri is null)
        {
            return false;
        }

        // did the caller explicitly ask to not cache this request?
        if (request.Options.TryGetValue(new HttpRequestOptionsKey<ETagCache>(nameof(ETagCache)), out ETagCache? settings))
        {
            if (settings.Policy == ETagCachePolicy.NoCache || settings.Expiration <= TimeSpan.Zero)
            {
                return false;
            }
        }

        return true;
    }

    private TimeSpan GetAbsoluteExpiration(HttpRequestMessage request)
    {
        if (request.Options.TryGetValue(new HttpRequestOptionsKey<ETagCache>(nameof(ETagCache)), out ETagCache? settings))
        {
            if (settings.Expiration <= TimeSpan.Zero)
            {
                return TimeSpan.Zero;
            }
            
            return settings.Expiration;
        }

        return TimeSpan.FromMinutes(5); 

    }
}


