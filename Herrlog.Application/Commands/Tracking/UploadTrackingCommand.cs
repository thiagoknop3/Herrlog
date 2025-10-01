using Herrlog.Application.DTOs;
using Herrlog.Domain.Entities;
using Herrlog.Infrastructure.Database;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Herrlog.Application.Commands.Tracking;

public record UploadTrackingCommand(List<TrackingUploadItemDto> Items) : IRequest<UploadTrackingResponse>;
public record UploadTrackingResponseError(string Plate, string Message);

public record UploadTrackingResponse(int Sucess, List<UploadTrackingResponseError> Errors);

public class UploadTrackingHandler(HerrlogDbContext db) : IRequestHandler<UploadTrackingCommand, UploadTrackingResponse>
{
    public async Task<UploadTrackingResponse> Handle(UploadTrackingCommand request, CancellationToken ct)
    {
        var errors = new List<UploadTrackingResponseError>();
        int success = 0;
        foreach (var it in request.Items)
        {
            try
            {
                var vehicle = await db.Vehicles.FirstOrDefaultAsync(v => v.Plate == it.Plate, cancellationToken: ct);
                if (vehicle == null)
                {
                    errors.Add(new UploadTrackingResponseError(it.Plate, "Vehicle doenst exists"));
                    continue;
                }

                var hasUtc = DateTime.TryParse(it.DateUTC, out var parsedUtc);
                if (!hasUtc && DateTime.TryParse(it.Date, out var parsedLocal))
                    parsedUtc = DateTime.SpecifyKind(parsedLocal, DateTimeKind.Local).ToUniversalTime();

                db.TrackingPoints.Add(new TrackingPointEntity
                {
                    VehicleId = vehicle.Id,
                    DateUtc = hasUtc ? parsedUtc : DateTime.UtcNow,
                    Latitude = it.Latitude,
                    Longitude = it.Longitude,
                    Speed = double.IsNaN(it.Speed) ? null : it.Speed,
                    Direction = double.IsNaN(it.Direction) ? null : it.Direction,
                    RawPlate = it.Plate
                });
            }
            catch (Exception)
            {
                errors.Add(new UploadTrackingResponseError(it.Plate, "An unexpected error happened."));
            }
        }
        success = await db.SaveChangesAsync(ct);
        return new UploadTrackingResponse(success, errors);
    }
}
