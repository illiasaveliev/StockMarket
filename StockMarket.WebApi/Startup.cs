using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.PlatformAbstractions;
using StockMarket.Domain;
using StockMarket.Service;
using Swashbuckle.AspNetCore.Swagger;

namespace StockMarket.WebApi
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
            services.AddAuthentication(sharedOptions =>
                    {
                        sharedOptions.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                        sharedOptions.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
                    })
                .AddCookie()
                .AddOpenIdConnect(option =>
                    {
                        option.ClientId = Configuration["AzureAD:ClientId"];
                        option.Authority = String.Format(Configuration["AzureAd:Instance"], Configuration["AzureAd:TenantId"]);
                        option.SignedOutRedirectUri = Configuration["AzureAd:PostLogoutRedirectUri"];
                        option.Events = new OpenIdConnectEvents
                                            {
                                                OnRemoteFailure = OnAuthenticationFailed,
                                            };
                    });

            services.AddSingleton<IStockSymbolsRepository, StockSymbolsRepository>();
            services.AddTransient<IStockSymbolsService, StockSymbolsService>();
            services.AddTransient<ICsvReader, CsvReader>();
            string databaseEndpoint = Configuration.GetConnectionString("DatabaseEndpoint");
            string databaseName = Configuration.GetConnectionString("DatabaseName");
            string user = Configuration.GetConnectionString("User");
            string password = Configuration.GetConnectionString("Password");
            services.AddTransient<IInfluxContext, InfluxContext>(s => new InfluxContext(databaseEndpoint, databaseName, user, password));
            services.AddMvc();
            services.AddSwaggerGen(c =>
                {
                    c.SwaggerDoc("v1", new Info { Title = "StockMarket API", Version = "v1" });
                    string basePath = PlatformServices.Default.Application.ApplicationBasePath;
                    string xmlPath = Path.Combine(basePath, "StockMarket.WebApi.xml");
                    c.IncludeXmlComments(xmlPath);
                });
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseAuthentication();
            app.UseSwagger();

            app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Stock Market API V1");
                });
            app.UseMvc();
        }

        private async Task OnAuthenticationFailed(RemoteFailureContext context)
        {
            context.HandleResponse();
            await context.Response.WriteAsync(context.Failure.Message);
        }
    }
}