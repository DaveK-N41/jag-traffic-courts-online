using Microsoft.Extensions.Logging;

namespace TrafficCourts.OrdsDataService.Justin;

internal class LanguageRepository : OrdsRepository<LanguageRepository>, ILanguageRepository
{
    public LanguageRepository(OrdsDataServiceClient client, ILogger<LanguageRepository> logger) 
        : base(client, "/v2/justin_languages", logger)
    {
    }

    public async Task<List<Language>> GetListAsync(CancellationToken cancellationToken)
    {
        var response = await GetListAsync(
            parameters: null,
            JsonContext.Default.OrdsDataServiceCollectionResponseLanguage,
            ETagCache.OneDay,
            cancellationToken);

        return response?.Rows ?? [];
    }
}
