using Microsoft.Extensions.DependencyInjection;
using TrafficCourts.OrdsDataService.Justin;
using TrafficCourts.OrdsDataService.Tco;

namespace TrafficCourts.OrdsDataService.Test;

public class CodeTableTests : OrdsDataServiceTest
{
    [Fact(Skip = SkipReason)]
    public async Task get_justin_agencies()
    {
        var repository = _serviceProvider.GetRequiredService<IAgencyRepository>();

        var values = await repository.GetListAsync(CancellationToken.None);
        Assert.NotEmpty(values);

        var expected = values.First().agen_id;
        var value = await repository.GetAsync(expected, CancellationToken.None);
        Assert.NotNull(value);
        Assert.Equal(expected, value.agen_id);
    }

    [Fact(Skip = SkipReason)]
    public async Task get_justin_cities()
    {
        var repository = _serviceProvider.GetRequiredService<ICityRepository>();
        var values = await repository.GetListAsync(CancellationToken.None);
        Assert.NotEmpty(values);
    }

    [Fact(Skip = SkipReason)]
    public async Task get_justin_countries()
    {
        var repository = _serviceProvider.GetRequiredService<ICountryRepository>();
        var values = await repository.GetListAsync(CancellationToken.None);
        Assert.NotEmpty(values);
    }

    [Fact(Skip = SkipReason)]
    public async Task get_justin_languages()
    {
        var repository = _serviceProvider.GetRequiredService<ILanguageRepository>();
        var values = await repository.GetListAsync(CancellationToken.None);
        Assert.NotEmpty(values);
    }

    [Fact(Skip = SkipReason)]
    public async Task get_justin_provinces()
    {
        var repository = _serviceProvider.GetRequiredService<IProvinceRepository>();
        var values = await repository.GetListAsync(CancellationToken.None);
        Assert.NotEmpty(values);
    }

    [Fact(Skip = SkipReason)]
    public async Task get_justin_statutes()
    {
        var repository = _serviceProvider.GetRequiredService<IStatuteRepository>();
        var values = await repository.GetListAsync(CancellationToken.None);
        Assert.NotEmpty(values);

        var expected = values.First().stat_id;
        var value = await repository.GetAsync(expected, CancellationToken.None);
        Assert.NotNull(value);
        Assert.Equal(expected, value.stat_id);
    }

    [Fact(Skip = SkipReason)]
    public async Task get_tco_audit_log_entry_types()
    {
        var repository = _serviceProvider.GetRequiredService<IAuditLogEntryTypeRepository>();
        var values = await repository.GetListAsync(CancellationToken.None);
        Assert.NotEmpty(values);
    }

    [Fact(Skip = SkipReason)]
    public async Task get_tco_dispute_status_types()
    {
        var repository = _serviceProvider.GetRequiredService<IDisputeStatusTypeRepository>();
        var values = await repository.GetListAsync(CancellationToken.None);
        Assert.NotEmpty(values);
    }
}
