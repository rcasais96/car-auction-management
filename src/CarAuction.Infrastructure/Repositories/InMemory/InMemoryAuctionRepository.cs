using CarAuction.Domain.Entities;
using CarAuction.Domain.Repositories;
using System.Collections.Concurrent;

namespace CarAuction.Infrastructure.Repositories.InMemory
{
    public class InMemoryAuctionRepository : IAuctionRepository
    {
        private readonly ConcurrentDictionary<Guid, Auction> _auctions = new();

        public Task<Auction?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            _auctions.TryGetValue(id, out var auction);
            return Task.FromResult(auction);
        }

        public Task AddAsync(Auction auction, CancellationToken cancellationToken = default)
        {
            _auctions[auction.Id] = auction;
            return Task.CompletedTask;
        }

        public Task<bool> HasActiveAuctionAsync(Guid vehicleId, CancellationToken cancellationToken = default)
        {
            var snapshot = _auctions.Values.ToArray();

            var hasActive = snapshot
                .Any(a => a.VehicleId == vehicleId && a.Status == AuctionStatus.Active);

            return Task.FromResult(hasActive);
        }
    }
}
