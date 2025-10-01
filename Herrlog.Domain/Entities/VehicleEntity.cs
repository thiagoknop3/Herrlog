namespace Herrlog.Domain.Entities;

public class VehicleEntity 
{
    public Guid Id { get; set; }
    public string Plate { get; set; } = string.Empty;
    public string? Model { get; set; }
    public ICollection<TrackingPointEntity> TrackingPoints { get; set; } = [];
}
