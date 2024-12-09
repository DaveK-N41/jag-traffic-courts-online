using Model = TrafficCourts.Domain.Models.Agency;

namespace TrafficCourts.Staff.Service.Features.Lookups.Agencies;

public record Response
{
    public Response(IList<Model> items)
    {
        Items = items;
    }

    public IList<Model> Items { get; }
}
