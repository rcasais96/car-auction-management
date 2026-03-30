using CarAuction.Application.Services;
using CarAuction.Application.Services.Interfaces;
using CarAuction.Domain;
using CarAuction.Domain.Entities;
using CarAuction.Domain.Repositories;

namespace CarAuction.Infrastructure.Repositories
{
    public class CachedVehicleRepository : IVehicleRepository
    {
        private readonly IVehicleRepository _inner;
        private readonly ICacheService _cache;

        private static string VehicleKey(Guid id) => $"vehicle:{id}";
        private static string SearchKey(VehicleSearchCriteria criteria)
            => $"vehicles:type={criteria.Type}:manufacturer={criteria.Manufacturer}:model={criteria.Model}:year={criteria.Year}";

        public CachedVehicleRepository(IVehicleRepository inner, ICacheService cache)
        {
            _inner = inner;
            _cache = cache;
        }

        public async Task<Vehicle?> GetByIdAsync(Guid id, CancellationToken ct = default)
        {
            var key = VehicleKey(id);

            var cached = await _cache.GetAsync<Vehicle>(key, ct);
            if (cached is not null)
                return cached;

            var vehicle = await _inner.GetByIdAsync(id, ct);
            if (vehicle is not null)
                await _cache.SetAsync(key, vehicle, ct: ct);

            return vehicle;
        }

        public async Task<IEnumerable<Vehicle>> SearchAsync(
            VehicleSearchCriteria criteria, CancellationToken ct = default)
        {
            var key = SearchKey(criteria);

            var cached = await _cache.GetAsync<IEnumerable<Vehicle>>(key, ct);
            if (cached is not null)
                return cached;

            var vehicles = await _inner.SearchAsync(criteria, ct);
            var list = vehicles.ToList();

            await _cache.SetAsync(key, list, TimeSpan.FromMinutes(5), ct);

            return list;
        }

        public async Task AddAsync(Vehicle vehicle, CancellationToken ct = default)
        {
            await _inner.AddAsync(vehicle, ct);
            // invalida search cache — dados mudaram
            // L1 e L2 expiram naturalmente por TTL
        }

        public async Task<bool> ExistsAsync(Guid id, CancellationToken ct = default)
            => await _inner.ExistsAsync(id, ct); // sempre vai à BD — precisa ser real
    }
}
