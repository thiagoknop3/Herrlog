using AutoMapper;
using AutoMapper.QueryableExtensions;
using Herrlog.Application.DTOs;
using Herrlog.Infrastructure.Database;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Herrlog.Application.Queries.Vehicle;

public record GetVehiclesQuery() : IRequest<List<VehicleDto>>;

public class GetVehiclesQueryHandler(HerrlogDbContext db, IMapper mapper) : IRequestHandler<GetVehiclesQuery, List<VehicleDto>>
{
    public async Task<List<VehicleDto>> Handle(GetVehiclesQuery query, CancellationToken cancellationToken)
        => await db.Vehicles.AsNoTracking().ProjectTo<VehicleDto>(mapper.ConfigurationProvider).ToListAsync(cancellationToken);
}

