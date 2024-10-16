//using Microsoft.EntityFrameworkCore;
//using Microsoft.EntityFrameworkCore.Design;
//using Microsoft.Extensions.Configuration;
//using System.IO;

//namespace RealEstateInfrastructure.Data
//{
//    public class RealEstateDbContextFactory : IDesignTimeDbContextFactory<RealEstateDbContext>
//    {
//        public RealEstateDbContext CreateDbContext(string[] args)
//        {
//            // ساخت تنظیمات برای خواندن از appsettings.json از پروژه اصلی
//            IConfigurationRoot configuration = new ConfigurationBuilder()
//                .SetBasePath(Path.Combine(Directory.GetCurrentDirectory(), "../RealEstateService"))
//                .AddJsonFile("appsettings.json")
//                .Build();

//            // خواندن connection string از appsettings.json
//            var connectionString = configuration.GetConnectionString("RealEstateConnection");

//            // ساخت DbContextOptions با استفاده از SqlServer
//            var optionsBuilder = new DbContextOptionsBuilder<RealEstateDbContext>();
//            optionsBuilder.UseSqlServer(connectionString);

//            return new RealEstateDbContext(optionsBuilder.Options);
//        }
//    }
//}
