namespace Herrlog.Domain.Entities;

public class TrackingPointEntity
{
    public int Id { get; set; }
    public Guid VehicleId { get; set; }
    public VehicleEntity? Vehicle { get; set; }
    public DateTime DateUtc { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public double? Speed { get; set; }
    public double? Direction { get; set; }
    public string? RawPlate { get; set; }
}
