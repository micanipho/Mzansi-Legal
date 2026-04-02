using Abp.AspNetCore;
using Abp.AspNetCore.Mvc.Antiforgery;
using Abp.AspNetCore.SignalR.Hubs;
using Abp.Castle.Logging.Log4Net;
using Abp.Extensions;
using backend.Configuration;
using backend.EntityFrameworkCore;
using backend.Identity;
using Microsoft.EntityFrameworkCore;
using Castle.Facilities.Logging;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace backend.Web.Host.Startup
{
    public class Startup
    {
        private const string _defaultCorsPolicyName = "localhost";

        private const string _apiVersion = "v1";

        private readonly IConfigurationRoot _appConfiguration;
        private readonly IWebHostEnvironment _hostingEnvironment;

        public Startup(IWebHostEnvironment env)
        {
            _hostingEnvironment = env;
            _appConfiguration = env.GetAppConfiguration();
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddHealthChecks();

            //MVC
            services.AddControllersWithViews(options =>
            {
                options.Filters.Add(new AbpAutoValidateAntiforgeryTokenAttribute());
            });

            IdentityRegistrar.Register(services);
            AuthConfigurer.Configure(services, _appConfiguration);

            services.AddSignalR();

            // Named HttpClient for OpenAI APIs (embeddings + chat completions + language detection/translation).
            // EmbeddingAppService, RagAppService, and LanguageAppService all resolve this client by name
            // via IHttpClientFactory. LanguageAppService is auto-registered by ABP as an ApplicationService.
            services.AddHttpClient("OpenAI", client =>
            {
                client.BaseAddress = new Uri(_appConfiguration["OpenAI:BaseUrl"]);
                client.Timeout = TimeSpan.FromSeconds(30);
            });

            // Hosted service that pre-loads chunk embeddings into the RAG service at startup.
            services.AddHostedService<RagStartupService>();

            // Configure CORS for angular2 UI
            services.AddCors(
                options => options.AddPolicy(
                    _defaultCorsPolicyName,
                    builder => builder
                        .WithOrigins(
                            // App:CorsOrigins in appsettings.json can contain more than one address separated by comma.
                            _appConfiguration["App:CorsOrigins"]
                                .Split(",", StringSplitOptions.RemoveEmptyEntries)
                                .Select(o => o.RemovePostFix("/"))
                                .ToArray()
                        )
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowCredentials()
                )
            );

            // Swagger - Enable this line and the related lines in Configure method to enable swagger UI
            ConfigureSwagger(services);

            // Configure Abp and Dependency Injection
            services.AddAbpWithoutCreatingServiceProvider<backendWebHostModule>(
                // Configure Log4Net logging
                options => options.IocManager.IocContainer.AddFacility<LoggingFacility>(
                    f => f.UseAbpLog4Net().WithConfig(Path.Combine(AppContext.BaseDirectory, _hostingEnvironment.IsDevelopment()
                        ? "log4net.config"
                        : "log4net.Production.config"
                    ))
                )
            );
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory loggerFactory)
        {
            // Migrations must run before UseAbp() — ABP's seeder queries the schema
            // immediately inside UseAbp() and will crash if tables don't exist yet.
            ApplyDatabaseMigrations(app);

            app.UseAbp(options => { options.UseAbpRequestLocalization = false; }); // Initializes ABP framework.

            app.UseCors(_defaultCorsPolicyName); // Enable CORS!

            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseAbpRequestLocalization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapHealthChecks("/api/health");
                endpoints.MapHub<AbpCommonHub>("/signalr");
                endpoints.MapControllerRoute("default", "{controller=Home}/{action=Index}/{id?}");
                endpoints.MapControllerRoute("defaultWithArea", "{area}/{controller=Home}/{action=Index}/{id?}");
            });

            // Enable middleware to serve generated Swagger as a JSON endpoint
            app.UseSwagger(c => { c.RouteTemplate = "swagger/{documentName}/swagger.json"; });

            // Enable middleware to serve swagger-ui assets (HTML, JS, CSS etc.)
            app.UseSwaggerUI(options =>
            {
                // specifying the Swagger JSON endpoint.
                options.SwaggerEndpoint($"/swagger/{_apiVersion}/swagger.json", $"backend API {_apiVersion}");
                options.IndexStream = () => Assembly.GetExecutingAssembly()
                    .GetManifestResourceStream("backend.Web.Host.wwwroot.swagger.ui.index.html");
                options.DisplayRequestDuration(); // Controls the display of the request duration (in milliseconds) for "Try it out" requests.
            }); // URL: /swagger
        }

        /// <summary>
        /// Applies all pending EF Core migrations to the database. Called before UseAbp()
        /// so the schema exists when ABP's seeder runs. Creates the DbContext directly from
        /// configuration rather than via Windsor, which is not yet initialised at this point.
        /// </summary>
        private void ApplyDatabaseMigrations(IApplicationBuilder app)
        {
            var connectionString = _appConfiguration.GetConnectionString("Default");
            var optionsBuilder = new DbContextOptionsBuilder<backendDbContext>();
            backendDbContextConfigurer.Configure(optionsBuilder, connectionString);

            try
            {
                using (var dbContext = new backendDbContext(optionsBuilder.Options))
                {
                    dbContext.Database.Migrate();
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Database migration failed at startup. The service will not start.", ex);
            }
        }

        private void ConfigureSwagger(IServiceCollection services)
        {
            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc(_apiVersion, new OpenApiInfo
                {
                    Version = _apiVersion,
                    Title = "backend API",
                    Description = "backend",
                    // uncomment if needed TermsOfService = new Uri("https://example.com/terms"),
                    Contact = new OpenApiContact
                    {
                        Name = "backend",
                        Email = string.Empty,
                        Url = new Uri("https://twitter.com/aspboilerplate"),
                    },
                    License = new OpenApiLicense
                    {
                        Name = "MIT License",
                        Url = new Uri("https://github.com/aspnetboilerplate/aspnetboilerplate/blob/dev/LICENSE.md"),
                    }
                });
                options.DocInclusionPredicate((docName, description) => true);

                // Define the BearerAuth scheme that's in use
                options.AddSecurityDefinition("bearerAuth", new OpenApiSecurityScheme()
                {
                    Description =
                        "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey
                });

                //add summaries to swagger
                bool canShowSummaries = _appConfiguration.GetValue<bool>("Swagger:ShowSummaries");
                if (canShowSummaries)
                {
                    var hostXmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                    var hostXmlPath = Path.Combine(AppContext.BaseDirectory, hostXmlFile);
                    options.IncludeXmlComments(hostXmlPath);

                    var applicationXml = $"backend.Application.xml";
                    var applicationXmlPath = Path.Combine(AppContext.BaseDirectory, applicationXml);
                    options.IncludeXmlComments(applicationXmlPath);

                    var webCoreXmlFile = $"backend.Web.Core.xml";
                    var webCoreXmlPath = Path.Combine(AppContext.BaseDirectory, webCoreXmlFile);
                    options.IncludeXmlComments(webCoreXmlPath);
                }
            });
        }
    }
}


