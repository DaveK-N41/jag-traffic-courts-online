using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace TrafficCourts.OrdsDataService.Test;

public abstract class OrdsDataServiceTest
{
    protected const string? SkipReason = "Integration Test";
    protected readonly ServiceProvider _serviceProvider;

    private readonly string? _baseAddress = Environment.GetEnvironmentVariable("TCOORDS_BASEADDRESS");
    private readonly string? _username = Environment.GetEnvironmentVariable("TCOORDS_USERNAME");
    private readonly string? _password = Environment.GetEnvironmentVariable("TCOORDS_PASSWORD");

    protected OrdsDataServiceTest()
    {
        if (!string.IsNullOrEmpty(_baseAddress) && !string.IsNullOrEmpty(_username) && !string.IsNullOrEmpty(_password))
        {
            IConfiguration configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>
                {
                    ["OrdsDataService:BaseAddress"] = _baseAddress,
                    ["OrdsDataService:Username"] = _username,
                    ["OrdsDataService:Password"] = _password
                })
                .Build();

            ServiceCollection services = new();
            services.AddOrdsDataService(configuration);
            _serviceProvider = services.BuildServiceProvider();
        }
    }
}
