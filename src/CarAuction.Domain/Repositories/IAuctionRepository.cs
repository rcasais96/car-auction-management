using CarAuction.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace CarAuction.Domain.Repositories
{
    public interface IAuctionRepository
    {
        Task<Auction?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task AddAsync(Auction auction, CancellationToken cancellationToken = default);
        Task<bool> HasActiveAuctionAsync(Guid vehicleId, CancellationToken cancellationToken = default);
    }
}
