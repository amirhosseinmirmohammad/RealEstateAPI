using Serilog;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Versioning;
using RealEstateInfrastructure.Data;
using RealEstateInfrastructure.Repositories;
using RealEstateCore.Interfaces.V1;
using RealEstateApplication.Services.V1;
using RealEstateCore.Models;
using RealEstateApplication.Services.Helpers;
using Elsa.Extensions;
using Elsa.Workflows.Activities;
using Elsa.Workflows.Contracts;
using Elsa.EntityFrameworkCore.Modules.Management;
using Elsa.EntityFrameworkCore.Modules.Runtime;
using Elsa.EntityFrameworkCore.Extensions;
using RealEstateService.ElsaWorkflow;
using Elsa.Alterations.Extensions;
using Elsa.EntityFrameworkCore.Modules.Alterations;
using System.Text.Encodings.Web;
using Elsa.EntityFrameworkCore.Modules.Identity;
using Microsoft.OpenApi.Models;

namespace RealEstateService
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
            _indexPath = Configuration["LuceneSettings:IndexPath"];
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            var connectionString = Configuration.GetConnectionString("RealEstateConnection");

            services.AddLogging();

            ConfigureDatabase(services);

            ConfigureApiVersioning(services);

            ConfigureSwagger(services);

            // اضافه کردن سرویس‌های Elsa
            services.AddElsa(elsa =>
            {
                // پیکربندی مدیریت و runtime با استفاده از EF Core
                elsa.UseWorkflowManagement(management => management.UseEntityFrameworkCore(ef => ef.UseSqlServer(connectionString)));
                elsa.UseWorkflowRuntime(runtime =>
                {
                    runtime.UseEntityFrameworkCore(ef =>
                    {
                        ef.UseSqlServer(connectionString);
                    });
                });
                elsa.UseAlterations(alterations =>
                {
                    alterations.UseEntityFrameworkCore(ef => ef.UseSqlServer(connectionString));
                });

                // Default Identity features for authentication/authorization.
                elsa.UseIdentity(identity =>
                {
                    identity.TokenOptions = options =>
                    {
                        options.SigningKey = "sufficiently-large-secret-signing-key"; // This key needs to be at least 256 bits long.
                        identity.UseAdminUserProvider();
                        identity.TokenOptions = tokenOptions => tokenOptions.SigningKey = "U2FsdGVkX1+6H3D8Q//yQMhInzTdRZI9DbUGetbyaag=";
                    };
                    identity.UseAdminUserProvider();

                    identity.UseEntityFrameworkCore(ef =>
                    {
                        ef.UseSqlServer(connectionString);
                        ef.RunMigrations = true;  // این گزینه باعث می‌شود که مهاجرت‌ها به صورت خودکار اجرا شوند.
                    });
                });

                elsa.AddSwagger();

                // Configure ASP.NET authentication/authorization.
                elsa.UseDefaultAuthentication();

                // استفاده از قابلیت‌های HTTP برای اضافه کردن فعالیت‌های HTTP
                elsa.UseHttp(http => http.ConfigureHttpOptions = options =>
                {
                    options.BaseUrl = new Uri("http://localhost:5041");
                    options.BasePath = "/workflows";
                });

                elsa.AddWorkflowsFrom<Program>();
                elsa.AddActivitiesFrom<Program>();

                elsa.AddActivity<ApproveActivity>();

                // استفاده از مدیریت زمان‌بندی
                elsa.UseScheduling();

                // Expose Elsa API endpoints.
                elsa.UseWorkflowsApi(api =>
                {
                    api.AddFastEndpointsAssembly<Program>();
                });

                elsa.UseLiquid(liquid => liquid.FluidOptions = options => options.Encoder = HtmlEncoder.Default);

                // Enable C# workflow expressions
                elsa.UseCSharp();
            });

            // Configure CORS to allow designer app hosted on a different origin to invoke the APIs.
            services.AddCors(cors => cors.AddDefaultPolicy(policy => policy.AllowAnyHeader().AllowAnyMethod().AllowAnyOrigin().WithExposedHeaders("*")));

            ConfigureDI(services);

            ConfigureFullSearch<RealEstate>(services, _indexPath);

            services.AddAuthorization();

            services.AddControllers();
        }

        public void Configure<App>(App app, IWebHostEnvironment env, IApiVersionDescriptionProvider apiVersionDescriptionProvider)
            where App : IApplicationBuilder, IEndpointRouteBuilder, IHost
        {
            app.UseForwardedHeaders();

            app.UseCors();

            app.UseRouting();

            app.UseAuthentication();

            app.UseAuthorization();

            app.UseWorkflowsApi();

            app.UseWorkflows();

            app.UseStaticFiles();

            if (env.IsDevelopment())
            {
                app.UseSwagger();
                app.UseHsts();
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

            app.MapControllers();

            // اجرای workflow در ابتدای شروع برنامه.
            using (var scope = app.ApplicationServices.CreateScope())
            {
                var workflowRunner = scope.ServiceProvider.GetRequiredService<IWorkflowRunner>();
                var workflow = new Sequence
                {
                    Activities =
                    {
                        new WriteLine("Hello World!"),
                        new WriteLine("Goodbye cruel world...")
                    }
                };
            }
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
            services.AddSwaggerGen(option =>
            {
                //issue for Elsa fix
                option.CustomSchemaIds(type => type.ToString());

                //預設使用JWT token 
                option.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    In = ParameterLocation.Header,
                    Description = "Enter: ApiKey [your API key]",
                    Name = "Authorization",
                    Type = SecuritySchemeType.ApiKey,
                    BearerFormat = "JWT",
                    Scheme = "Bearer"
                });
                option.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type=ReferenceType.SecurityScheme,
                    Id="Bearer"
                }
            },
            new string[]{}
        }
    });
            }); services.ConfigureOptions<ConfigureSwaggerOptions>();
        }

        private void ConfigureDI(IServiceCollection services)
        {
            services.Configure<DatabaseSettings>(Configuration.GetSection("ConnectionStrings"));

            services.AddScoped<IRealEstateRepository, RealEstateRepository>();

            services.AddScoped<RealEstatesService>();

            services.AddScoped<PhotosService>();
        }

        private void ConfigureFullSearch<T>(IServiceCollection services, string indexPath) where T : class, new()
        {
            services.AddScoped<ILuceneEngine<T>>(provider =>
            {
                var logger = provider.GetRequiredService<ILogger<LuceneEngine<T>>>();
                return new LuceneEngine<T>(indexPath, logger);
            });
        }

        private readonly string _indexPath;
    }
}