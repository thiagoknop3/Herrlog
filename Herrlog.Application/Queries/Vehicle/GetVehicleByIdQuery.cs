using AutoMapper;
using AutoMapper.QueryableExtensions;
using Herrlog.Application.DTOs;
using Herrlog.Infrastructure.Database;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Herrlog.Application.Queries.Vehicle;

public record GetVehicleByIdQuery(Guid Id) : IRequest<VehicleDto?>;
public class GetVehicleByIdQueryHandler(HerrlogDbContext dbContext, IMapper mapper) 
    : IRequestHandler<GetVehicleByIdQuery, VehicleDto?>
{
    private readonly HerrlogDbContext _dbContext = dbContext;
    private readonly IMapper _mapper = mapper;

    public async Task<VehicleDto?> Handle(GetVehicleByIdQuery query, CancellationToken cancellationToken)
    {
        return query == null
            ? throw new ArgumentNullException(nameof(query))
            : await _dbContext.Vehicles
            .Where(x => x.Id == query.Id)
            .ProjectTo<VehicleDto>(_mapper.ConfigurationProvider)
            .FirstOrDefaultAsync(cancellationToken);
    }
}

