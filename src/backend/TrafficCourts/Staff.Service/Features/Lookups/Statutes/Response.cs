using Model = TrafficCourts.Domain.Models.Statute;

namespace TrafficCourts.Staff.Service.Features.Lookups.Statutes;

public record Response
{
    public Response(IList<Model> items)
    {
        Items = items;
    }

    public IList<Model> Items { get; }
}
