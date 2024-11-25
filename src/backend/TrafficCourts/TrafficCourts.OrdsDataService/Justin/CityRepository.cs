using Microsoft.Extensions.Logging;

namespace TrafficCourts.OrdsDataService.Justin;

internal class CityRepository : OrdsRepository<CityRepository>, ICityRepository
{
    public CityRepository(OrdsDataServiceClient client, ILogger<CityRepository> logger) 
        : base(client, "/v2/justin_cities", logger)
    {
    }

    public async Task<List<City>> GetListAsync(CancellationToken cancellationToken)
    {
        var response = await GetListAsync(
            parameters: null,
            JsonContext.Default.OrdsDataServiceCollectionResponseCity,
            ETagCache.OneDay,
            cancellationToken);

        return response?.Rows ?? [];
    }
}
