
using Herrlog.Infrastructure.Database;
using MediatR;

namespace Herrlog.Application.Commands.Vehicle;

public record DeleteVehicleCommand(Guid Id) : IRequest<(bool deleted, string message)>;
public class DeleteVehicleHandler(HerrlogDbContext db) : IRequestHandler<DeleteVehicleCommand, (bool, string)>
{
    public async Task<(bool,string)> Handle(DeleteVehicleCommand request, CancellationToken ct)
    {
        var v = await db.Vehicles.FindAsync([request.Id], ct);
        if (v is null) return new (false, "Vehicle doesnt exists");
        db.Remove(v);
        await db.SaveChangesAsync(ct);
        return new(true, "Vehicle deleted");
;    }


}
