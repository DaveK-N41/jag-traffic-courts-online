
namespace TrafficCourts.Staff.Service.Features.Lookups.Cities;

public class City
{
}

public record Response
{
    public Response(IList<City> items)
    {
        Items = items;
    }

    public IList<City> Items { get; }
}