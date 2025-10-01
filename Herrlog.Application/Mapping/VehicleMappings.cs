using AutoMapper;
using Herrlog.Application.DTOs;
using Herrlog.Domain.Entities;

namespace Herrlog.Application.Mapping;

public class VehicleMapping : Profile
{
    public VehicleMapping()
    {
        CreateMap<VehicleEntity, VehicleDto>();
        CreateMap<VehicleCreateDto, VehicleEntity>();
        CreateMap<VehicleUpdateDto, VehicleEntity>();
    }
}
