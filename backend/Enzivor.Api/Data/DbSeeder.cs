using Enzivor.Api.Models.Domain;
using Enzivor.Api.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace Enzivor.Api.Data
{
    public static class DbSeeder
    {
        public static async Task SeedAsync(AppDbContext db)
        {
            await db.Database.MigrateAsync();

            if (!await db.LandfillSites.AnyAsync())
            {
                db.LandfillSites.AddRange(
                    new LandfillSite
                    {
                        Name = "Vinca (example)",
                        Category = LandfillCategory.Sanitary,
                        PointLat = 44.7443,
                        PointLon = 20.6051,
                        RegionTag = "Beograd",
                        EstimatedAreaM2 = 250000
                    },
                    new LandfillSite
                    {
                        Name = "Glozan (example)",
                        Category = LandfillCategory.Illegal,
                        PointLat = 45.2985,
                        PointLon = 19.5712,
                        RegionTag = "Vojvodina",
                        EstimatedAreaM2 = 12000
                    }
                    );
                await db.SaveChangesAsync();
            }
        }
    }
}
