using CarAuction.Domain.Repositories;
using CarAuction.Infrastructure.Database;
using CarAuction.Infrastructure.Repositories.Database;
using CarAuction.Infrastructure.Repositories.InMemory;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace CarAuction.Infrastructure.Extensions
{
    public static class InfrastructureExtensions
    {
        public static IServiceCollection AddInfrastructure(
            this IServiceCollection services,
            IConfiguration configuration, 
            bool useInMemory = false)
        {
            if (useInMemory)
            {
                services.AddSingleton<IVehicleRepository, InMemoryVehicleRepository>();
                services.AddSingleton<IAuctionRepository, InMemoryAuctionRepository>();
            }
            else
            {
                services.AddDbContext<AuctionDbContext>(options =>
                    options.UseSqlServer(
                        configuration.GetConnectionString("DefaultConnection")));

                services.AddScoped<IVehicleRepository, VehicleRepository>();
                services.AddScoped<IAuctionRepository, AuctionRepository>();
            }

            return services;
        }
    }
}
