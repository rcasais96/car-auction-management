using CarAuction.Domain.Entities;
using CarAuction.Domain.Repositories;
using CarAuction.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace CarAuction.Infrastructure.Repositories.Database
{
    public class AuctionRepository : IAuctionRepository
    {

        private readonly AuctionDbContext _context;

        public AuctionRepository(AuctionDbContext context)
        {
            _context = context;
        }

        public async Task<Auction?> GetByIdAsync(Guid id, CancellationToken ct = default)
            => await _context.Auctions
                .Include(a => a.Bids)
                .Include(a => a.Vehicle)
                .FirstOrDefaultAsync(a => a.Id == id, ct);

        public async Task AddAsync(Auction auction, CancellationToken ct = default)
        {
            await _context.Auctions.AddAsync(auction, ct);
        }

        public async Task<bool> HasActiveAuctionAsync(Guid vehicleId, CancellationToken ct = default)
            => await _context.Auctions
                .AnyAsync(a => a.VehicleId == vehicleId
                            && a.Status == AuctionStatus.Active, ct);
    }
}
