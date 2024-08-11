using Serilog;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Versioning;
using RealEstateInfrastructure.Data;
using RealEstateInfrastructure.Repositories;
using RealEstateCore.Interfaces.V1;
using RealEstateApplication.Services.V1;

namespace RealEstateService
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddLogging();

            ConfigureDatabase(services);

            ConfigureApiVersioning(services);

            ConfigureSwagger(services);

            ConfigureDI(services);

            services.AddAuthorization();

            services.AddControllers();
        }

        public void Configure<App>(App app, IWebHostEnvironment env, IApiVersionDescriptionProvider apiVersionDescriptionProvider)
            where App : IApplicationBuilder, IEndpointRouteBuilder, IHost
        {
            app.UseForwardedHeaders();

            app.UseRouting();

            app.UseCors(builder => builder
                .AllowAnyOrigin()
                .AllowAnyHeader()
                .AllowAnyMethod());

            app.UseStaticFiles();

            if (env.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI(options =>
                {
                    foreach (var description in apiVersionDescriptionProvider.ApiVersionDescriptions)
                    {
                        options.SwaggerEndpoint($"/swagger/{description.GroupName}/swagger.json",
                            description.GroupName.ToUpperInvariant());
                    }
                });
            }

            app.UseSerilogRequestLogging();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();
        }

        private void ConfigureDatabase(IServiceCollection services)
        {
            services.AddDbContext<RealEstateDbContext>(options =>
            {
                string connectionString = Configuration.GetConnectionString("RealEstateConnection");
                options.UseSqlServer(connectionString);
                options.UseLazyLoadingProxies();
            });
        }

        private void ConfigureApiVersioning(IServiceCollection services)
        {
            services.AddApiVersioning(opt =>
            {
                opt.DefaultApiVersion = new ApiVersion(1, 0);
                opt.AssumeDefaultVersionWhenUnspecified = true;
                opt.ReportApiVersions = true;
                opt.ApiVersionReader = ApiVersionReader.Combine(
                    new UrlSegmentApiVersionReader(),
                    new HeaderApiVersionReader("x-api-version"),
                    new MediaTypeApiVersionReader("x-api-version"));
            });

            services.AddVersionedApiExplorer(setup =>
            {
                setup.GroupNameFormat = "'v'VVV";
                setup.SubstituteApiVersionInUrl = true;
            });
        }

        private void ConfigureSwagger(IServiceCollection services)
        {
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen();
            services.ConfigureOptions<ConfigureSwaggerOptions>();
        }

        private void ConfigureDI(IServiceCollection services)
        {
            services.AddScoped<IRealEstateRepository, RealEstateRepository>();
            services.AddScoped<RealEstateApplication.Services.V1.RealEstatesService>();
            services.AddScoped<PhotosService>();
        }
    }
}
