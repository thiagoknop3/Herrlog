
using AutoMapper;
using FluentValidation;
using Herrlog.Application.DTOs;
using Herrlog.Domain.Entities;
using Herrlog.Infrastructure.Database;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Herrlog.Application.Commands.Vehicle;

public record CreateVehicleCommand(VehicleCreateDto Dto) : IRequest<VehicleDto>;

public class CreateVehicleHandler(HerrlogDbContext db, IValidator<VehicleCreateDto> validator, IMapper mapper)
    : IRequestHandler<CreateVehicleCommand, VehicleDto>
{
    public async Task<VehicleDto> Handle(CreateVehicleCommand request, CancellationToken ct)
    {
        var val = await validator.ValidateAsync(request.Dto, ct);
        if (!val.IsValid) throw new ValidationException(val.Errors);
        if (await db.Vehicles.AnyAsync(v => v.Plate == request.Dto.Plate, ct))
            throw new ValidationException("Plate must be unique");

        var entity = mapper.Map<VehicleEntity>(request.Dto);
        entity.Id = Guid.NewGuid();
        db.Vehicles.Add(entity);
        await db.SaveChangesAsync(ct);
        return mapper.Map<VehicleDto>(entity);
    }
}