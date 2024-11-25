using Model = TrafficCourts.Domain.Models.Country;

namespace TrafficCourts.Staff.Service.Features.Lookups.Countries;

public record Response
{
    public Response(IList<Model> items)
    {
        Items = items;
    }

    public IList<Model> Items { get; }
}