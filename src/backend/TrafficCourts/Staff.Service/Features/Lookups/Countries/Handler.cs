using MediatR;
using TrafficCourts.OrdsDataService.Justin;
using ZiggyCreatures.Caching.Fusion;

namespace TrafficCourts.Staff.Service.Features.Lookups.Countries;

public class Handler : IRequestHandler<Request, Response>
{
    private readonly ICountryRepository _repository;
    private readonly IFusionCache _cache;
    private readonly ILogger<Handler> _handler;

    public Handler(ICountryRepository repository, IFusionCache cache, ILogger<Handler> handler)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        _handler = handler ?? throw new ArgumentNullException(nameof(handler));
    }
    public async Task<Response> Handle(Request request, CancellationToken cancellationToken)
    {
        var key = Caching.Cache.Api.Countries(2);

        var items = await _cache.GetOrSetAsync<List<Domain.Models.Country>>(
            key,
            ct => GetItems(ct),
            options => options.SetDuration(TimeSpan.FromMinutes(10)),
            token: cancellationToken);

        return new Response(items);
    }

    private async Task<List<Domain.Models.Country>> GetItems(CancellationToken cancellationToken)
    {
        var items = await _repository.GetListAsync(cancellationToken);

        var models = items.Select(_ => new Domain.Models.Country
        (
            _.ctry_id.ToString(),
            _.ctry_long_nm
        )).ToList();

        return models;
    }
}
