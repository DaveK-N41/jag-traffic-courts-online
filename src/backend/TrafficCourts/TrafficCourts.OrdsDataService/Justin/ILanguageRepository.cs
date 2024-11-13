namespace TrafficCourts.OrdsDataService.Justin;

public interface ILanguageRepository
{
    Task<List<Language>> GetListAsync(CancellationToken cancellationToken);
}