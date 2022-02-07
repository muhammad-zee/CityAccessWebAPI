using ElmahCore.Mvc;
using ElmahCore.Sql;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using Web.API.Helper;
using Web.Data;
using Web.Data.Models;
using Web.DLL;
using Web.DLL.Generic_Repository;
using Web.Services.Concrete;
using Web.Services.Helper;
using Web.Services.Interfaces;

namespace Web.API
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<RAQ_DbContext>(options =>
              options.UseSqlServer(
                 Configuration.GetConnectionString("DefaultConnection")));

            services.AddControllers()
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.IgnoreNullValues = true;
                    options.JsonSerializerOptions.Converters.Add(new DateTimeConverter());
                });
            services.AddCors();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Version = "v1",
                    Title = "Routing And Queueing API",
                    // Description = "API with ASP.NET Core",
                    //Contact = new OpenApiContact()
                    //{
                    //    Name = "HRMS",
                    //    Url = new Uri("https://localhost:44390/")
                    //}
                });
                var securitySchema = new OpenApiSecurityScheme
                {
                    Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    }
                };
                c.AddSecurityDefinition("Bearer", securitySchema);

                var securityRequirement = new OpenApiSecurityRequirement();
                securityRequirement.Add(securitySchema, new[] { "Bearer" });
                c.AddSecurityRequirement(securityRequirement);
            });

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
             .AddJwtBearer(options =>
             {
                 options.TokenValidationParameters = new TokenValidationParameters
                 {
                     ValidateIssuer = true,
                     ValidateAudience = true,
                     ValidateLifetime = true,
                     ValidateIssuerSigningKey = true,
                     ValidIssuer = Configuration["Jwt:ValidAudience"],
                     ValidAudience = Configuration["Jwt:ValidAudience"],
                     IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["Jwt:Secret"]))
                 };
                 //options.Events = new JwtBearerEvents
                 //{
                 //    OnAuthenticationFailed = async (context) =>
                 //    {
                 //        context.Response = ;
                 //    }
                 //}
             });

            services.AddElmah<SqlErrorLog>(options =>
            {
                options.ConnectionString = Configuration.GetConnectionString("DefaultConnection");
                options.Path = new PathString("/elm");
                options.ApplicationName = "RoutingAndQueueingAPI";
            });

            // register the repositories
            services.AddDbContext<RAQ_DbContext>();
            services.AddScoped<DbContext>(sp => sp.GetService<RAQ_DbContext>());

            services.AddTransient(typeof(IUnitOfWork), typeof(UnitOfWork));

            services.AddHttpContextAccessor();
            services.AddTransient(typeof(IRepository<>), typeof(GenericRepository<>));

            //Register Services
            services.AddTransient(typeof(IJwtAuthService), typeof(AuthService));
            services.AddTransient(typeof(ICommunicationService), typeof(CommunicationService));
            services.AddTransient(typeof(IAdminService), typeof(AdminService));
            services.AddTransient(typeof(IScheduleService), typeof(ScheduleService));
            services.AddTransient(typeof(IFacilityService), typeof(FacilityService));
            services.AddTransient(typeof(ICallService), typeof(CallService));
            services.AddTransient(typeof(ISettingService), typeof(SettingsService));

            //Register Services Repositories

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddLog4Net();
            /*if (env.IsDevelopment())
            {*/
            app.UseDeveloperExceptionPage();
            app.UseSwagger();
            app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "RoutingAndQueueingAPI v1"));
            /*}*/



            app.UseSwaggerUI(c =>
            {
                // For Debug in Kestrel
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Web API V1");
                // To deploy on IIS
                c.SwaggerEndpoint("/mysite/swagger/v1/swagger.json", "Web API V1");
                c.RoutePrefix = string.Empty;
            });
            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthentication();

            app.UseAuthorization();

            //app.UseElmahIo();
            app.UseElmah();
            app.UseElmahExceptionPage();

            // global error handler
            app.UseMiddleware<ErrorHandlerMiddleware>();

            app.UseCors(x => x
              .SetIsOriginAllowed(origin => true)
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials());

            app.UseStaticFiles(new StaticFileOptions
            {
                ServeUnknownFileTypes = true,
            })
            .UseDirectoryBrowser()
            .UseRequestLocalization();

            //app.UseStaticFiles(new StaticFileOptions
            //{
            //    FileProvider = new PhysicalFileProvider(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "UserProfiles"))
            //});
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
