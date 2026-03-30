using CarAuction.Application.Services.Interfaces;
using CarAuction.Domain.Repositories;
using CarAuction.Infrastructure.Cache;
using CarAuction.Infrastructure.Database;
using CarAuction.Infrastructure.Repositories;
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
            // cache
            services.AddMemoryCache();
            services.AddStackExchangeRedisCache(options =>
                options.Configuration = configuration.GetConnectionString("Redis"));

            // ← regista o CacheService
            services.AddScoped<ICacheService, CacheService>();

            if (useInMemory)
            {
                services.AddSingleton<IVehicleRepository, InMemoryVehicleRepository>();
                services.AddSingleton<IAuctionRepository, InMemoryAuctionRepository>();
                services.AddSingleton<IUnitOfWork, InMemoryUnitOfWork>();
            }
            else
            {
                services.AddDbContext<AuctionDbContext>(options =>
                    options.UseSqlServer(
                        configuration.GetConnectionString("DefaultConnection")));

                services.AddScoped<VehicleRepository>();
                services.AddScoped<IVehicleRepository>(sp =>
                    new CachedVehicleRepository(
                        sp.GetRequiredService<VehicleRepository>(),
                        sp.GetRequiredService<ICacheService>()));

                services.AddScoped<IAuctionRepository, AuctionRepository>();
                services.AddScoped<IUnitOfWork>(sp =>
                    sp.GetRequiredService<AuctionDbContext>());
            }

            return services;
        }
    }
}
