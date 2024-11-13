using MediatR;

namespace TrafficCourts.Staff.Service.Features.Lookups.Agencies;

public class Request(string? type = null) : IRequest<Response>
{
    public string? Type { get; set; } = type;
}
