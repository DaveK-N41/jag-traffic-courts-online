using MediatR;
using TrafficCourts.Domain.Models;
using TrafficCourts.OrdsDataService.Tco;
using ZiggyCreatures.Caching.Fusion;

namespace TrafficCourts.Staff.Service.Features.Lookups.DisputeCaseFileStatusTypes;

public class Handler : IRequestHandler<Request, Response>
{
    private readonly IDisputeStatusTypeRepository _repository;
    private readonly IFusionCache _cache;
    private readonly ILogger<Handler> _logger;

    public Handler(IDisputeStatusTypeRepository repository, IFusionCache cache, ILogger<Handler> logger)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Response> Handle(Request request, CancellationToken cancellationToken)
    {
        var key = Caching.Cache.Api.DisputeCaseFileStatusTypes(2);

        var items = await _cache.GetOrSetAsync<List<DisputeCaseFileStatus>> (
            key,
            GetItems,
            options => options.SetDuration(TimeSpan.FromMinutes(60)),
            token: cancellationToken);

        return new Response(items);
    }

    private async Task<List<DisputeCaseFileStatus>> GetItems(CancellationToken cancellationToken)
    {
        var items = await _repository.GetListAsync(cancellationToken);

        var models = items.Select(item => new DisputeCaseFileStatus
        {
            Code = item.dispute_status_type_cd,
            Description = item.dispute_status_type_dsc
        }).ToList();

        return models;
    }
}
