using CarAuction.Domain;
using CarAuction.Domain.Entities;
using CarAuction.Domain.Repositories;
using CarAuction.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace CarAuction.Infrastructure.Repositories.Database
{

    public class VehicleRepository : IVehicleRepository
    {
        private readonly AuctionDbContext _context;

        public VehicleRepository(AuctionDbContext context)
        {
            _context = context;
        }

        public async Task<Vehicle?> GetByIdAsync(Guid id, CancellationToken ct = default)
            => await _context.Vehicles.FindAsync(new object[] { id }, ct);

        public async Task<bool> ExistsAsync(Guid id, CancellationToken ct = default)
            => await _context.Vehicles.AnyAsync(v => v.Id == id, ct);

        public async Task AddAsync(Vehicle vehicle, CancellationToken ct = default)
        {
            await _context.Vehicles.AddAsync(vehicle, ct);
        }

        public async Task<IEnumerable<Vehicle>> SearchAsync(
            VehicleSearchCriteria criteria, CancellationToken ct = default)
        {
            var query = _context.Vehicles.AsQueryable();

            if (criteria.Type is not null)
                query = query.Where(v => v.Type == criteria.Type);

            if (!string.IsNullOrWhiteSpace(criteria.Manufacturer))
                query = query.Where(v => v.Manufacturer.Contains(criteria.Manufacturer));

            if (!string.IsNullOrWhiteSpace(criteria.Model))
                query = query.Where(v => v.Model.Contains(criteria.Model));

            if (criteria.Year is not null)
                query = query.Where(v => v.Year == criteria.Year);

            return await query.ToListAsync(ct);
        }
    }
}
