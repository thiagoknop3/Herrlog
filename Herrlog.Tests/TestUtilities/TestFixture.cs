using AutoMapper;
using Castle.Core.Logging;
using FluentValidation;
using FluentValidation.Results;
using Herrlog.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace Herrlog.Tests.TestUtilities;

public static class TestFixture
{
    public static HerrlogDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<HerrlogDbContext>();
        options.UseInMemoryDatabase($"herrlog-tests-{Guid.NewGuid():N}");
        options.EnableSensitiveDataLogging();
        return new HerrlogDbContext(options.Options);
    }

    public static IMapper CreateMapper()
    {
        var loggerFactory = NullLoggerFactory.Instance;
        var expr = new MapperConfigurationExpression();
        expr.AddMaps(typeof(Application.Mapping.VehicleMapping).Assembly);
        expr.AddMaps(typeof(Application.Mapping.TrackingPointMapping).Assembly);
        expr.ConstructServicesUsing(t => Activator.CreateInstance(t)!);
        var config = new AutoMapper.MapperConfiguration(expr, loggerFactory);
        return new Mapper(config);
    }

    public static IValidator<T> OkValidator<T>()
    {
        var mock = new Mock<IValidator<T>>();
        mock.Setup(v => v.ValidateAsync(It.IsAny<T>(), default))
            .ReturnsAsync(new ValidationResult());
        return mock.Object;
    }
}
