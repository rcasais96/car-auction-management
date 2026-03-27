using CarAuction.Domain.Repositories;
using CarAuction.Infrastructure.Repositories.InMemory;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace CarAuction.Infrastructure.Extensions
{
    public static class InfrastructureExtensions
    {
        public static IServiceCollection AddInfrastructure(
            this IServiceCollection services)
        {
            services.AddSingleton<IVehicleRepository, InMemoryVehicleRepository>();
            services.AddSingleton<IAuctionRepository, InMemoryAuctionRepository>();
            return services;
        }
    }
}
