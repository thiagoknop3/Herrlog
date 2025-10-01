
using Herrlog.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Herrlog.Infrastructure.Database;

public class HerrlogDbContext(DbContextOptions<HerrlogDbContext> options) : DbContext(options)
{
    public DbSet<VehicleEntity> Vehicles => Set<VehicleEntity>();
    public DbSet<TrackingPointEntity> TrackingPoints => Set<TrackingPointEntity>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        b.Entity<VehicleEntity>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Plate).IsRequired().HasMaxLength(20);
            e.Property(x => x.Model).HasMaxLength(80);
            e.HasIndex(x => x.Plate).IsUnique();
        });

        b.Entity<TrackingPointEntity>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Latitude).IsRequired();
            e.Property(x => x.Longitude).IsRequired();
            e.Property(x => x.DateUtc).IsRequired();
            e.HasOne(x => x.Vehicle)
                .WithMany(v => v.TrackingPoints)
                .HasForeignKey(x => x.VehicleId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
