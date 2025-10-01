using AutoMapper;
using FluentValidation;
using Herrlog.Application.Commands.Tracking;
using Herrlog.Application.Commands.Vehicle;
using Herrlog.Application.DTOs;
using Herrlog.Application.Mapping;
using Herrlog.Application.Queries.Tracking;
using Herrlog.Application.Queries.Vehicle;
using Herrlog.Application.Validation;
using Herrlog.Infrastructure.Database;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// --- CONFIG ---
builder.Services.Configure<Microsoft.AspNetCore.Http.Json.JsonOptions>(o =>
{
    o.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    o.SerializerOptions.WriteIndented = false;
});

// --- INFRA ---
var mdf = Path.GetFullPath(
    Path.Combine(builder.Environment.ContentRootPath,
        @"..\Herrlog.Infrastructure\Database\HerrlogDb.mdf"));
var cs = builder.Configuration.GetConnectionString("SqlServerTemplate")!
           .Replace("|MDF_PATH|", mdf);
builder.Services.AddDbContext<HerrlogDbContext>(opt => opt.UseSqlServer(cs));


// --- MAPPINGS (AUTOMAPPER) ---
builder.Services.AddSingleton<IMapper>(sp =>
{

    var expr = new MapperConfigurationExpression();
    expr.ConstructServicesUsing(t => ActivatorUtilities.GetServiceOrCreateInstance(sp, t));
    expr.AddMaps(typeof(VehicleMapping).Assembly);
    expr.AddMaps(typeof(TrackingPointMapping).Assembly);

    var loggerFactory = sp.GetRequiredService<ILoggerFactory>();

    var config = new MapperConfiguration(expr, loggerFactory);

    return new Mapper(config, sp.GetService);
});
// --- MEDIATR ---
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<GetVehiclesQuery>());
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<GetVehicleByIdQuery>());
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<GetTrackingPointQuery>());
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<CreateVehicleCommand>());
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<UploadTrackingCommand>());
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<UpdateVehicleCommand>());
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<DeleteVehicleCommand>());


//JSON Options
builder.Services.ConfigureHttpJsonOptions(o =>
{
    o.SerializerOptions.PropertyNameCaseInsensitive = true;
    o.SerializerOptions.NumberHandling = JsonNumberHandling.AllowReadingFromString;
    o.SerializerOptions.AllowTrailingCommas = true;
    o.SerializerOptions.ReadCommentHandling = JsonCommentHandling.Skip;
});

// --- APP SERVICES ---
builder.Services.AddValidatorsFromAssemblyContaining<VehicleCreateValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<VehicleUpdateValidator>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddCors(p => p.AddDefaultPolicy(policy =>
    policy.AllowAnyHeader().AllowAnyMethod().AllowAnyOrigin()
));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Herrlog API");
        c.RoutePrefix = string.Empty;
    });
}

app.UseCors();

// CRUD for Vehicles

app.MapGet("/api/vehicles", async (IMediator mediator)
    => Results.Ok(await mediator.Send(new GetVehiclesQuery())));

app.MapGet("/api/vehicles/{id:guid}", async (Guid id, IMediator mediator) =>
{
    var result = await mediator.Send(new GetVehicleByIdQuery(id));
    return result is null ? Results.NotFound() : Results.Ok(result);
});

app.MapPost("/api/vehicles", async (VehicleCreateDto dto, IMediator mediator) =>
{
    try
    {
        var created = await mediator.Send(new CreateVehicleCommand(dto));
        return Results.Created($"/api/vehicles/{created.Id}", created);
    }
    catch (ValidationException ex)
    {
        var errors = ex.Errors
            .GroupBy(e => e.PropertyName)
            .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());

        return Results.ValidationProblem(
            errors: errors,
            title: "Validation failed",
            statusCode: StatusCodes.Status422UnprocessableEntity,
            detail: "Please fix the highlighted fields."
        );
    }

});

app.MapPut("/api/vehicles/{id:guid}", async (Guid id, VehicleUpdateDto dto, IMediator mediator) =>
{
    try
    {
        var result = await mediator.Send(new UpdateVehicleCommand(id, dto));
        return Results.Ok(result);
    }
    catch (ValidationException ex)
    {
        var errors = ex.Errors
            .GroupBy(e => e.PropertyName)
            .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());

        return Results.ValidationProblem(
            errors: errors,
            title: "Validation failed",
            statusCode: StatusCodes.Status422UnprocessableEntity,
            detail: "Please fix the highlighted fields."
        );
    }

});

app.MapDelete("/api/vehicles/{id:guid}", async (Guid id, IMediator mediator) =>
{
    var (deleted, message) = await mediator.Send(new DeleteVehicleCommand(id));
    return deleted ? Results.Ok() : Results.NotFound(message);
});

// Tracking (Json)
app.MapPost("/api/tracking/upload/",
    async ([FromBody] List<TrackingUploadItemDto> items,
           IMediator mediator,
           CancellationToken ct) =>
    {
        if (items is null || items.Count == 0)
            return Results.ValidationProblem(
                new Dictionary<string, string[]> { ["body"] = ["Invalid or empty JSON list."] },
                title: "Validation failed",
                statusCode: StatusCodes.Status400BadRequest);

        var results = await mediator.Send(new UploadTrackingCommand(items), ct);
        if (results.Errors.Count > 0)
            return Results.Problem(
                detail: string.Join(" | ", results.Errors.Select(e => $"Plate {e.Plate}: {e.Message}")),
                title: "Some items were not processed",
                statusCode: StatusCodes.Status207MultiStatus);
        return Results.Ok(new { results.Sucess, count = items.Count });
    })
.Produces(StatusCodes.Status200OK)
.ProducesValidationProblem(StatusCodes.Status400BadRequest);

app.MapGet("/api/tracking/{vehicleId:guid}", async (Guid vehicleId, IMediator mediator)
    => Results.Ok(await mediator.Send(new GetTrackingPointQuery(vehicleId))));

app.Run();
