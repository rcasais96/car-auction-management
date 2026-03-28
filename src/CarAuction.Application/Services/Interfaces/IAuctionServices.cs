using CarAuction.Application.DTOs.Auctions;
using System;
using System.Collections.Generic;
using System.Text;

namespace CarAuction.Application.Services.Interfaces
{
    public interface IAuctionService
    {
        Task<AuctionDTO> GetByIdAsync(Guid auctionId, CancellationToken cancellationToken = default);
        Task<AuctionDTO> CreateAuctionAsync(CreateAuctionRequest request, CancellationToken cancellationToken = default);
        Task<AuctionDTO> StartAuctionAsync(Guid auctionId, CancellationToken cancellationToken = default);
        Task<BidDTO> PlaceBidAsync(Guid auctionId, PlaceBidRequest request, CancellationToken cancellationToken = default);
        Task CloseAuctionAsync(Guid auctionId, CancellationToken cancellationToken = default);
    }
}
