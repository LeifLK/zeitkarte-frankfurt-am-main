using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using backend.Entities;
using Microsoft.EntityFrameworkCore;

namespace backend.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<ArrivalTimeEntity> ArrivalTimes { get; set; }

    public DbSet<StopEntity> Stops { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasPostgresExtension("postgis");

        modelBuilder.Entity<ArrivalTimeEntity>()
            .HasKey(s => new { s.OriginStopId, s.DestinationStopId });

        modelBuilder.Entity<StopEntity>()
            .HasKey(s => s.StopId);
    }
}
