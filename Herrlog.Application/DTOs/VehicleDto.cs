namespace Herrlog.Application.DTOs;
public record VehicleCreateDto(string Plate, string? Model);
public record VehicleUpdateDto(Guid Id, string Plate, string? Model);
public record VehicleDto(Guid Id, string Plate, string? Model);
