using Herrlog.Application.DTOs;
using Herrlog.Infrastructure.Database;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Herrlog.Application.Queries.Tracking;

public record GetTrackingPointQuery(Guid VehicleId) : IRequest<TrackingPointDto?>;

public class GetTrackingPointHandler(HerrlogDbContext db) : IRequestHandler<GetTrackingPointQuery, TrackingPointDto?>
{
    public async Task<TrackingPointDto?> Handle(GetTrackingPointQuery request, CancellationToken ct)
    {
        return await db.TrackingPoints
            .Where(p => p.VehicleId == request.VehicleId)
            .OrderByDescending(p => p.DateUtc)
            .Select(p => new TrackingPointDto(
                p.Id,
                p.RawPlate ?? string.Empty, 
                p.DateUtc,
                p.Latitude,
                p.Longitude,
                p.Speed))
            .FirstOrDefaultAsync(ct);
    }
}
