namespace TrafficCourts.OrdsDataService.Justin;

public interface IAgencyRepository
{
    Task<Agency?> GetAsync(decimal agen_id, CancellationToken cancellationToken);

    Task<List<Agency>> GetListAsync(CancellationToken cancellationToken);
    Task<List<Agency>> GetListAsync(string agencyType, CancellationToken cancellationToken);
}
