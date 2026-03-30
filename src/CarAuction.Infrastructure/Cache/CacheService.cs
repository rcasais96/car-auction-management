using CarAuction.Application.Services.Interfaces;
using CarAuction.Infrastructure.Repositories.Cache;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace CarAuction.Infrastructure.Cache
{
    public class CacheService : ICacheService
    {
        private readonly IMemoryCache _l1;
        private readonly IDistributedCache _l2;

        private static readonly TimeSpan DefaultL1Expiry = TimeSpan.FromMinutes(5);
        private static readonly TimeSpan DefaultL2Expiry = TimeSpan.FromMinutes(30);
        private static readonly JsonSerializerOptions _jsonOptions = new()
        {
            Converters = { new VehicleJsonConverter() }
        };

        public CacheService(IMemoryCache l1, IDistributedCache l2)
        {
            _l1 = l1;
            _l2 = l2;
        }


        public async Task<T?> GetAsync<T>(string key, CancellationToken ct = default)
        {
            if (_l1.TryGetValue(key, out T? l1Value))
                return l1Value;

            var l2Value = await _l2.GetStringAsync(key, ct);
            if (l2Value is not null)
            {
                var value = JsonSerializer.Deserialize<T>(l2Value, _jsonOptions); // ← usa options
                _l1.Set(key, value, DefaultL1Expiry);
                return value;
            }

            return default;
        }

        public async Task SetAsync<T>(string key, T value, TimeSpan? expiry = null, CancellationToken ct = default)
        {
            var l1Expiry = expiry ?? DefaultL1Expiry;
            var l2Expiry = expiry ?? DefaultL2Expiry;

            _l1.Set(key, value, l1Expiry);

            var serialized = JsonSerializer.Serialize(value, _jsonOptions); // ← usa options
            await _l2.SetStringAsync(key, serialized, new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = l2Expiry
            }, ct);
        }
        public async Task RemoveAsync(string key, CancellationToken ct = default)
        {
            _l1.Remove(key);
            await _l2.RemoveAsync(key, ct);
        }
    }
}
