using CarAuction.Domain.Entities;
using CarAuction.Domain.Repositories;
using System;
using System.Collections.Generic;
using System.Text;

namespace CarAuction.Infrastructure.Repositories
{
    public class AuctionRepository : IAuctionRepository
    {

        public Task AddAsync(Auction auction, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }



        public Task<Auction?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }



        public Task<bool> HasActiveAuctionAsync(Guid vehicleId, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }
}
