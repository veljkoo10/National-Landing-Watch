using Enzivor.Api.Models.Domain;
using Microsoft.EntityFrameworkCore;

namespace Enzivor.Api.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<LandfillSite> LandfillSites => Set<LandfillSite>();
        public DbSet<LandfillDetection> LandfillDetections => Set<LandfillDetection>();
        public DbSet<UserMessage> UserMessages => Set<UserMessage>();
        public DbSet<UserReport> UserReports => Set<UserReport>();

        protected override void OnModelCreating(ModelBuilder b)
        {
            b.Entity<LandfillSite>(e =>
            {
                e.HasKey(x => x.Id);

                e.Property(x => x.Category).IsRequired();
                e.Property(x => x.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
                e.Property(x => x.UpdatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");

                e.HasIndex(x => x.Category);
                e.HasIndex(x => x.RegionTag);
            });

            b.Entity<LandfillDetection>(e =>
            {
                e.HasKey(x => x.Id);
                e.Property(x => x.ImageName).IsRequired();
                e.HasIndex(x => x.ImageName);

                e.HasOne(x => x.LandfillSite)
                 .WithMany(s => s.Detections)
                 .HasForeignKey(x => x.LandfillSiteId)
                 .OnDelete(DeleteBehavior.SetNull);
            });

            b.Entity<UserMessage>(e =>
            {
                e.HasKey(x => x.Id);
                e.Property(x => x.Name).IsRequired();
                e.Property(x => x.Email).IsRequired();
                e.Property(x => x.Subject).IsRequired();
                e.Property(x => x.Content).IsRequired();
                e.Property(x => x.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            });

            b.Entity<UserReport>(e =>
            {
                e.HasKey(x => x.Id);
                e.Property(x => x.Latitude).IsRequired();
                e.Property(x => x.Longitude).IsRequired();
                e.Property(x => x.PlaceName).IsRequired(false);
                e.Property(x => x.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            });
        }

        public override Task<int> SaveChangesAsync(CancellationToken ct = default)
        {
            foreach (var entry in ChangeTracker.Entries<LandfillSite>())
            {
                if (entry.State == EntityState.Modified)
                    entry.Entity.UpdatedAt = DateTime.UtcNow;
            }
            return base.SaveChangesAsync(ct);
        }
    }
}