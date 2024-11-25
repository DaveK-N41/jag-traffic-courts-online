
namespace TrafficCourts.OrdsDataService.Justin;

public interface IProvinceRepository
{
    Task<List<Province>> GetListAsync(CancellationToken cancellationToken);
}
