using MediatR;
using TrafficCourts.OrdsDataService.Justin;
using ZiggyCreatures.Caching.Fusion;

namespace TrafficCourts.Staff.Service.Features.Lookups.Provinces;

public class Handler : IRequestHandler<Request, Response>
{
    private readonly IProvinceRepository _repository;
    private readonly IFusionCache _cache;
    private readonly ILogger<Handler> _handler;

    public Handler(IProvinceRepository repository, IFusionCache cache, ILogger<Handler> handler)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        _handler = handler ?? throw new ArgumentNullException(nameof(handler));
    }
    public async Task<Response> Handle(Request request, CancellationToken cancellationToken)
    {
        var key = Caching.Cache.Api.Provinces(2);

        var items = await _cache.GetOrSetAsync<List<Domain.Models.Province>>(
            key,
            ct => GetItems(ct),
            options => options.SetDuration(TimeSpan.FromMinutes(10)),
            token: cancellationToken);

        return new Response(items);
    }

    private async Task<List<Domain.Models.Province>> GetItems(CancellationToken cancellationToken)
    {
        var items = await _repository.GetListAsync(cancellationToken);

        var models = items.Select(_ => new Domain.Models.Province
        (
            _.ctry_id.ToString(),
            _.prov_seq_no.ToString(),
            _.prov_nm,
            _.prov_abbreviation_cd
        )).ToList();

        return models;
    }

}
