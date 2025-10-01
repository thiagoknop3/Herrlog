using AutoMapper;
using FluentAssertions;
using Herrlog.Application.Commands.Vehicle;
using Herrlog.Application.DTOs;
using Herrlog.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Xunit;
using static Herrlog.Tests.TestUtilities.TestFixture;

namespace Herrlog.Tests.Vehicles;

public class VehicleCommandsTests
{
    private readonly IMapper _mapper = CreateMapper();

    [Fact]
    public async Task CreateVehicle_Should_Persist_And_Return_Dto()
    {
        await using var db = CreateDbContext();
        var validator = OkValidator<VehicleCreateDto>();
        var handler = new CreateVehicleHandler(db,  validator, _mapper);

        var cmd = new CreateVehicleCommand(new VehicleCreateDto("ABC1D23", "Test Car"));
        var created = await handler.Handle(cmd, CancellationToken.None);

        created.Should().NotBeNull();
        created.Plate.Should().Be("ABC1D23");

        var entity = await db.Vehicles.AsNoTracking().SingleAsync(v => v.Id == created.Id);
        entity.Model.Should().Be("Test Car");
    }

    [Fact]
    public async Task UpdateVehicle_Should_Modify_Entity()
    {
        await using var db = CreateDbContext();
        var v = new VehicleEntity { Id = Guid.NewGuid(), Plate = "AAA0A00", Model = "Old" };
        db.Vehicles.Add(v);
        await db.SaveChangesAsync();

        var validator = OkValidator<VehicleUpdateDto>();
        var handler = new UpdateVehicleHandler(db, validator);

        var cmd = new UpdateVehicleCommand(v.Id, new VehicleUpdateDto (v.Id,  "BBB1B11",  "New" ));
        var updated =  await handler.Handle(cmd, CancellationToken.None);

        updated.Should().NotBeNull();
        updated.Plate.Should().Be("BBB1B11");

        var entity = await db.Vehicles.AsNoTracking().SingleAsync(x => x.Id == v.Id);
        entity.Model.Should().Be("New");
    }

    [Fact]
    public async Task DeleteVehicle_Should_Remove_From_Db()
    {
        await using var db = CreateDbContext();
        var v = new VehicleEntity { Id = Guid.NewGuid(), Plate = "DEL1ETE", Model = "ToRemove" };
        db.Vehicles.Add(v);
        await db.SaveChangesAsync();

        var handler = new DeleteVehicleHandler(db);
        await handler.Handle(new DeleteVehicleCommand(v.Id), CancellationToken.None);

        var exists = await db.Vehicles.AnyAsync(x => x.Id == v.Id);
        exists.Should().BeFalse();
    }
}
