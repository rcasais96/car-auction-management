using CarAuction.Application.Services;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace CarAuction.Application.Extensions
{
    public static class ApplicationExtensions
    {
        public static IServiceCollection AddApplication(
            this IServiceCollection services)
        {
            services.AddScoped<VehicleService>();
            services.AddScoped<AuctionService>();
            return services;
        }
    }
}
