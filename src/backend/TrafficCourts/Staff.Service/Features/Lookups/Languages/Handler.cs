using MediatR;
using TrafficCourts.OrdsDataService.Justin;
using ZiggyCreatures.Caching.Fusion;

namespace TrafficCourts.Staff.Service.Features.Lookups.Languages;

public class Handler : IRequestHandler<Request, Response>
{
    private readonly ILanguageRepository _repository;
    private readonly IFusionCache _cache;
    private readonly ILogger<Handler> _handler;

    public Handler(ILanguageRepository repository, IFusionCache cache, ILogger<Handler> handler)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        _handler = handler ?? throw new ArgumentNullException(nameof(handler));
    }
    public async Task<Response> Handle(Request request, CancellationToken cancellationToken)
    {
        var key = Caching.Cache.Api.Languages(2);

        var items = await _cache.GetOrSetAsync<List<Domain.Models.Language>>(
            key,
            ct => GetItems(ct),
            options => options.SetDuration(TimeSpan.FromMinutes(10)),
            token: cancellationToken);

        return new Response(items);
    }

    private async Task<List<Domain.Models.Language>> GetItems(CancellationToken cancellationToken)
    {
        var items = await _repository.GetListAsync(cancellationToken);

        var models = items.Select(_ => new Domain.Models.Language
        (
            _.cdln_language_cd,
            _.cdln_language_dsc
        )).ToList();

        return models;
    }
}
