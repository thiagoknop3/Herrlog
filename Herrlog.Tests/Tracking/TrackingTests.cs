
using AutoMapper;
using FluentAssertions;
using Herrlog.Application.Commands.Tracking;
using Herrlog.Application.DTOs;
using Herrlog.Application.Queries.Tracking;
using Herrlog.Domain.Entities;

using Microsoft.EntityFrameworkCore;
using Xunit;
using static Herrlog.Tests.TestUtilities.TestFixture;

namespace Herrlog.Tests.Tracking;

public class TrackingTests
{
    private readonly IMapper _mapper = CreateMapper();

    [Fact]
    public async Task UploadTracking_Should_Insert_Points_For_Vehicle()
    {
        await using var db = CreateDbContext();

        var vehicle = new VehicleEntity { Id = Guid.NewGuid(), Plate = "TRK1", Model = "Truck" };
        db.Vehicles.Add(vehicle);
        await db.SaveChangesAsync();

        var now = DateTime.UtcNow;
        var items = new List<TrackingUploadItemDto>
        {
            MakeItem("TRK1", -23.0, -46.0, now),
            MakeItem("TRK1", -23.1, -46.1, now.AddMinutes(1))
        };

        var handler = new UploadTrackingHandler(db); 

        var inserted = await handler.Handle(new UploadTrackingCommand(items), CancellationToken.None);

        inserted.Sucess.Should().Be(2);
        var count = await db.TrackingPoints.CountAsync(tp => tp.VehicleId == vehicle.Id);
        count.Should().Be(2);
    }

    [Fact]
    public async Task GetTrackingPoints_Should_Return_Last_Created()
    {
        await using var db = CreateDbContext();

        var vId = Guid.NewGuid();
        db.Vehicles.Add(new VehicleEntity { Id = vId, Plate = "TRK2" });

        db.TrackingPoints.AddRange(
            new TrackingPointEntity { Id =1, VehicleId = vId, Latitude = -23,   Longitude = -46,   DateUtc = DateTime.UtcNow.AddMinutes(-2) },
            new TrackingPointEntity { Id = 2, VehicleId = vId, Latitude = -23.1, Longitude = -46.1, DateUtc = DateTime.UtcNow.AddMinutes(-1) }
        );
        await db.SaveChangesAsync();

        var handler = new GetTrackingPointHandler(db);
        var dto = await handler.Handle(new GetTrackingPointQuery(vId), CancellationToken.None);

        dto.Should().NotBeNull();
        dto.Id.Should().Be(2);        
    }


    private static TrackingUploadItemDto MakeItem(string plate, double lat, double lon, DateTime tsUtc)
    => new TrackingUploadItemDto(
        plate,                 // Plate
        "TESTMODEL",           // DeviceModel
        1L,                    // DeviceId (long)
        1L,                    // PositionId (long)
        tsUtc.ToString("yyyy-MM-ddTHH:mm:ss"), // Date
        tsUtc.ToString("yyyy-MM-ddTHH:mm:ss"), // DateUTC
        "1",                   // Realtime
        "ON",                  // Ignition
        0d,                    // Odometer
        "0",                   // Horimeter
        "",                    // Address
        0d,                    // Direction
        "H",                   // Header
        "1",                   // GpsFix
        0d,                    // Speed
        "0",                   // MainBattery
        "0",                   // BackupBattery
        lat,                   // Latitude (double)
        lon,                   // Longitude (double)
        "",                    // DriverName
        "0",                   // DriverId
        "0",                   // Input1
        "0",                   // Input2
        "0",                   // Output1
        "0",                   // Output2
        "",                    // Rs232
        0,                     // IsLbs (int)
        "",                    // Rpm
        "GPS",                 // OriginPosition
        "0%"                   // BatteryPercentual
    );
}
