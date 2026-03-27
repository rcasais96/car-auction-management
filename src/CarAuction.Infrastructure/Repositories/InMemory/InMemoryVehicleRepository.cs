using CarAuction.Domain;
using CarAuction.Domain.Entities;
using CarAuction.Domain.Repositories;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace CarAuction.Infrastructure.Repositories.InMemory
{
    public class InMemoryVehicleRepository : IVehicleRepository
    {
        private readonly ConcurrentDictionary<Guid, Vehicle> _vehicles = new();

        public Task<Vehicle?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            _vehicles.TryGetValue(id, out var vehicle);
            return Task.FromResult(vehicle);
        }

        public Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(_vehicles.ContainsKey(id));
        }

        public Task AddAsync(Vehicle vehicle, CancellationToken cancellationToken = default)
        {
            _vehicles[vehicle.Id] = vehicle;
            return Task.CompletedTask;
        }

        public Task<IEnumerable<Vehicle>> SearchAsync(VehicleSearchCriteria criteria, CancellationToken cancellationToken = default)
        {
            var snapshot = _vehicles.Values.ToArray();
            var query = snapshot.AsEnumerable();

            if (criteria.Type is not null)
                    query = query.Where(v => v.Type == criteria.Type);

            if (!string.IsNullOrWhiteSpace(criteria.Manufacturer))
                query = query.Where(v => v.Manufacturer.Contains(
                    criteria.Manufacturer,
                    StringComparison.OrdinalIgnoreCase));

            if (!string.IsNullOrWhiteSpace(criteria.Model))
                query = query.Where(v => v.Model.Contains(
                    criteria.Model,
                    StringComparison.OrdinalIgnoreCase));

            if (criteria.Year is not null)
                query = query.Where(v => v.Year == criteria.Year);

            return Task.FromResult(query.ToList().AsEnumerable());
        }
    }
}
