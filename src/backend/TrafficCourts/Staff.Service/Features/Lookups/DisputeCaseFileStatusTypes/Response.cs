using Model = TrafficCourts.Domain.Models.DisputeCaseFileStatus;

namespace TrafficCourts.Staff.Service.Features.Lookups.DisputeCaseFileStatusTypes;

public record Response
{
    public Response(IList<Model> items)
    {
        Items = items;
    }

    public IList<Model> Items { get; }
}
