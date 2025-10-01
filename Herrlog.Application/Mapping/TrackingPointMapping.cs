using AutoMapper;
using Herrlog.Application.DTOs;
using Herrlog.Domain.Entities;

namespace Herrlog.Application.Mapping;

public class TrackingPointMapping : Profile
{
    public TrackingPointMapping()
    {
        CreateMap<TrackingPointEntity, TrackingPointDto>();
    }
}
