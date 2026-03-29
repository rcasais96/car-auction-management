using CarAuction.Application.Services;
using CarAuction.Application.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace CarAuction.Application.Extensions
{
    public static class ApplicationExtensions
    {
        public static IServiceCollection AddApplication(
            this IServiceCollection services)
        {
            services.AddScoped<IVehicleService, VehicleService>();
            services.AddScoped<IAuctionService,AuctionService>();
            return services;
        }
    }
}
