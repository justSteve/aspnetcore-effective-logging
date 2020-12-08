using System;
using System.Data;
using System.Data.SqlClient;
using System.IdentityModel.Tokens.Jwt;
using BookClub.Data;
using BookClub.Logic;
using Flogger.Serilog;
using Flogger.Serilog.Middleware;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Serilog.Events;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace BookClub.API
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
            services.AddScoped<IDbConnection, SqlConnection>(p =>
                new SqlConnection(Configuration.GetConnectionString("BookClubDb")));
            services.AddScoped<IBookRepository, BookRepository>();
            services.AddScoped<IBookLogic, BookLogic>();
            services.AddTransient<IConfigureOptions<SwaggerGenOptions>, SwaggerConfig>();

            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();
            services.AddAuthentication("Bearer")
                .AddJwtBearer(options =>
                {
                    options.Authority = Configuration.GetValue<string>("Security:Authority");
                    options.Audience = Configuration.GetValue<string>("Security:Audience");
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        NameClaimType = "name",
                        RoleClaimType = "role"

                    };
                });

            services.AddAuthorization();

            services.AddSwaggerGen();  // configured in SwaggerConfig by transient dependency above

            services.AddMvc();
        }

        public void Configure(IApplicationBuilder app)
        {
            app.UseCustomApiExceptionHandler(options =>
            {
                options.AddResponseDetails = UpdateApiErrorResponse;
                options.DetermineLogLevel = DetermineLogLevel;
            });
            app.UseHsts();
            app.UseStaticFiles();
            app.UseSwagger();
            app.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint("/swagger/v1/swagger.json", "Book Club API");
                options.OAuthClientId(Configuration.GetValue<string>("Security:ClientId"));  
                options.OAuthClientSecret(Configuration.GetValue<string>("Security:ClientSecret"));
                options.OAuthAppName("Book Club API");
                options.OAuthUsePkce();
            });
            app.UseAuthentication();

            app.UseHttpsRedirection();
            app.UseRouting();
            app.UseAuthorization();
            app.UseCustomSerilogRequestLogging(GetType().Assembly.GetName());
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers().RequireAuthorization();
            });
        }

        private LogEventLevel DetermineLogLevel(Exception ex)
        {
            if (ex.Message.StartsWith("cannot open database", StringComparison.InvariantCultureIgnoreCase) ||
                ex.Message.StartsWith("a network-related", StringComparison.InvariantCultureIgnoreCase))
            {
                return LogEventLevel.Fatal;
            }
            return LogEventLevel.Error;
        }

        private void UpdateApiErrorResponse(HttpContext context, Exception ex, ApiError error)
        {
            if (ex.GetType().Name == nameof(SqlException))
            {
                error.Detail = "Exception was a database exception!";
            }
            //error.Links = "https://gethelpformyerror.com/";
        }
    }
}
