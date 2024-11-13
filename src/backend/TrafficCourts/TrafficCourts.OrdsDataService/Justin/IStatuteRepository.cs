
namespace TrafficCourts.OrdsDataService.Justin;

public interface IStatuteRepository
{
    Task<Statute?> GetAsync(decimal stat_id, CancellationToken cancellationToken);
    Task<List<Statute>> GetListAsync(CancellationToken cancellationToken);
}
