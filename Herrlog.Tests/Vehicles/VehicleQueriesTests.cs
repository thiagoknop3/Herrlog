using AutoMapper;
using FluentAssertions;
using Herrlog.Application.Queries.Vehicle;
using Herrlog.Domain.Entities;
using Xunit;
using static Herrlog.Tests.TestUtilities.TestFixture;

namespace Herrlog.Tests.Vehicles;

public class VehicleQueriesTests
{
    private readonly IMapper _mapper = CreateMapper();

    [Fact]
    public async Task GetVehicleById_Should_Return_Dto()
    {
        await using var db = CreateDbContext();
        var v = new VehicleEntity { Id = Guid.NewGuid(), Plate = "QRY1D23", Model = "QueryCar" };
        db.Vehicles.Add(v);
        await db.SaveChangesAsync();

        var handler = new GetVehicleByIdQueryHandler(db, _mapper);
        var dto = await handler.Handle(new GetVehicleByIdQuery(v.Id), CancellationToken.None);

        dto.Should().NotBeNull();
        dto!.Id.Should().Be(v.Id);
        dto!.Plate.Should().Be("QRY1D23");
    }

    [Fact]
    public async Task GetVehicles_Should_Return_List()
    {
        await using var db = CreateDbContext();
        db.Vehicles.AddRange(
            new VehicleEntity { Id = Guid.NewGuid(), Plate = "AAA0001", Model = "A" },
            new VehicleEntity { Id = Guid.NewGuid(), Plate = "AAA0002", Model = "B" },
            new VehicleEntity { Id = Guid.NewGuid(), Plate = "AAA0003", Model = "C" }
        );
        await db.SaveChangesAsync();

        var handler = new GetVehiclesQueryHandler(db, _mapper);
        var list = await handler.Handle(new GetVehiclesQuery(), CancellationToken.None);

        list.Should().NotBeNull();
        list!.Count.Should().BeGreaterThanOrEqualTo(3);
        list!.Any(x => x.Plate == "AAA0002").Should().BeTrue();
    }
}
