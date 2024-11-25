using Model = TrafficCourts.Domain.Models.Province;

namespace TrafficCourts.Staff.Service.Features.Lookups.Provinces;

public record Response
{
    public Response(IList<Model> items)
    {
        Items = items;
    }

    public IList<Model> Items { get; }
}