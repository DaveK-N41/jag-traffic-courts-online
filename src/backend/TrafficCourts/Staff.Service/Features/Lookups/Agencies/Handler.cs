using MediatR;
using TrafficCourts.OrdsDataService.Justin;
using ZiggyCreatures.Caching.Fusion;

namespace TrafficCourts.Staff.Service.Features.Lookups.Agencies;

public class Handler : IRequestHandler<Request, Response>
{
    private readonly IAgencyRepository _repository;
    private readonly IFusionCache _cache;
    private readonly ILogger<Handler> _logger;

    public Handler(IAgencyRepository repository, IFusionCache cache, ILogger<Handler> logger)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Response> Handle(Request request, CancellationToken cancellationToken)
    {
        var key = request.Type is not null 
            ? Caching.Cache.Api.Agencies(request.Type, 2)
            : Caching.Cache.Api.Agencies(2);

        var items = await _cache.GetOrSetAsync<List<Domain.Models.Agency>>(
            key,
            ct => GetItems(request.Type, ct),
            options => options.SetDuration(TimeSpan.FromMinutes(10)),
            token: cancellationToken);

        return new Response(items);
    }

    private async Task<List<Domain.Models.Agency>> GetItems(string? type, CancellationToken cancellationToken)
    {
        List<Agency> items = type is not null 
            ? await _repository.GetListAsync(type, cancellationToken) 
            : await _repository.GetListAsync(cancellationToken);

        List<Domain.Models.Agency> models = items.Select(_ => new Domain.Models.Agency
        (
            _.agen_id.ToString(),
            _.agen_agency_nm,
            _.cdat_agency_type_cd
        )).ToList();

        return models;
    }
}
