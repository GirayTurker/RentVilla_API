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
            //services.AddDbContext<AppDBContext>(options =>
            //{
            //    options.UseSqlServer(config.GetConnectionString("DefaultSQLConnection"));
            //});

            services.AddDbContext<AppDBContext>(options =>
            {
                options.UseSqlServer(config.GetConnectionString("IYSsoftSQLConnection"));
            });

            services.AddScoped<ITokenService, TokenServices>();
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());




            //USE THROUGHOUT THE APPLICATION
            services.AddSingleton<ILoggerDev, LoggerDev>();

            var serviceProvider = services.BuildServiceProvider();

            var loggerDev = serviceProvider.GetService<ILoggerDev>();

            loggerDev.Log("===================================", "info");
            loggerDev.Log("======== RentVilla User API =======", "info");
            loggerDev.Log("===================================\n", "info");


            return services;
        }
    }
}
