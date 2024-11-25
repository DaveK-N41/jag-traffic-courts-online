namespace TrafficCourts.OrdsDataService.Justin;

public interface ICityRepository
{
    Task<List<City>> GetListAsync(CancellationToken cancellationToken);
}
