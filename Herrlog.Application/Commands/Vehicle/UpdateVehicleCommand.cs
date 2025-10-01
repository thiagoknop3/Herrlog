using FluentValidation;
using Herrlog.Application.DTOs;
using Herrlog.Domain.Entities;
using Herrlog.Infrastructure.Database;
using MediatR;

namespace Herrlog.Application.Commands.Vehicle;

public record UpdateVehicleCommand(Guid Id, VehicleUpdateDto Dto) : IRequest<VehicleEntity>;

public class UpdateVehicleHandler(HerrlogDbContext db, IValidator<VehicleUpdateDto> validator) : IRequestHandler<UpdateVehicleCommand, VehicleEntity>
{
    public async Task<VehicleEntity> Handle(UpdateVehicleCommand request, CancellationToken ct)
    {
        var val = await validator.ValidateAsync(request.Dto, ct);
        if (!val.IsValid) throw new ValidationException(val.Errors);

        var v = await db.Vehicles.FindAsync([request.Id], ct) ?? throw new KeyNotFoundException("Vehicle not found");
        v.Plate = request.Dto.Plate;
        v.Model = request.Dto.Model;
        await db.SaveChangesAsync(ct);
        return v;
    }
}