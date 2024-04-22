using Microsoft.EntityFrameworkCore;
using RentVilla_API.Data;
using RentVilla_API.Interfaces;
using RentVilla_API.Logger;
using RentVilla_API.Logger.LoogerInterfaces;
using RentVilla_API.Services;
namespace RentVilla_API.Extensions
{
    public static class AppServiceExtensions
    {
        public static IServiceCollection AppServices(this IServiceCollection services, IConfiguration config, IWebHostEnvironment webHostEnvironment) 
        {
            services.AddDbContext<AppDBContext>(options =>
            {
                options.UseSqlServer(config.GetConnectionString("DefaultSQLConnection"));
            });

            services.AddScoped<ITokenService, TokenServices>();
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
            

            if (webHostEnvironment.IsDevelopment()) 
            {
                //USE THROUGHOUT THE APPLICATION
                services.AddSingleton<ILoggerDev, LoggerDev>();

                var serviceProvider = services.BuildServiceProvider();

                var loggerDev = serviceProvider.GetService<ILoggerDev>();

                loggerDev.Log("\n=======================", "info");
                loggerDev.Log("========DEV ENV.=======", "info");
                loggerDev.Log("=======================\n", "info");
            }

            return services;
        }
    }
}
