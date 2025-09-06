using BasarStajApp_New.Entity;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;

namespace BasarStajApp_New.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<GeometryEntity> Geometries => Set<GeometryEntity>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // PostGIS eklentisi
            modelBuilder.HasPostgresExtension("postgis");

            modelBuilder.Entity<GeometryEntity>(b =>
            {
                b.ToTable("geometries");
                b.HasKey(x => x.Id);
                b.Property(x => x.Name).HasMaxLength(100).IsRequired();

                // Geometry kolonu (SRID 4326 ile)
                b.Property(x => x.Feature)
                 .HasColumnType("geometry");
                b.Property(x => x.Type).HasMaxLength(20);


            });
        }
    }
}

