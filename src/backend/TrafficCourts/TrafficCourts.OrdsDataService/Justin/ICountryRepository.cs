namespace TrafficCourts.OrdsDataService.Justin
{
    public interface ICountryRepository
    {
        Task<List<Country>> GetListAsync(CancellationToken cancellationToken);
    }
}