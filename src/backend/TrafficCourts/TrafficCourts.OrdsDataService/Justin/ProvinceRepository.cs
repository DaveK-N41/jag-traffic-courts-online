using Microsoft.Extensions.Logging;

namespace TrafficCourts.OrdsDataService.Justin;

internal class ProvinceRepository : OrdsRepository<ProvinceRepository>, IProvinceRepository
{
    public ProvinceRepository(OrdsDataServiceClient client, ILogger<ProvinceRepository> logger) 
        : base(client, "/v2/justin_provinces", logger)
    {
    }

    public async Task<List<Province>> GetListAsync(CancellationToken cancellationToken)
    {
        var response = await GetListAsync(
            parameters: null,
            JsonContext.Default.OrdsDataServiceCollectionResponseProvince,
            ETagCache.OneDay,
            cancellationToken);

        return response?.Rows ?? [];
    }
}
