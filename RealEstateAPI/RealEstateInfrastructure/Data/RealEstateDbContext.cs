using Microsoft.EntityFrameworkCore;
using RealEstateCore.Models;

namespace RealEstateInfrastructure.Data
{
    public class RealEstateDbContext : DbContext
    {
        public RealEstateDbContext(DbContextOptions<RealEstateDbContext> options)
            : base(options)
        {
        }

        public DbSet<RealEstate> RealEstates { get; set; }
        public DbSet<ChangeLog> ChangeLogs { get; set; }
        public DbSet<RealEstatePhoto> RealEstatePhotos { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }
    }
}
