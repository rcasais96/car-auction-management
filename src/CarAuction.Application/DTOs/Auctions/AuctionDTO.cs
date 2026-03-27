using CarAuction.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace CarAuction.Application.DTOs.Auctions
{
    public class AuctionDTO
    {
        public Guid Id { get; init; }
        public Guid VehicleId { get; init; }
        public decimal StartingBid { get; init; }
        public decimal CurrentHighestBid { get; init; }
        public AuctionStatus Status { get; init; }
        public DateTime? StartedAt { get; init; }
        public DateTime? ClosedAt { get; init; }
        public DateTime CreatedAt { get; init; }
        public IEnumerable<BidDTO> Bids { get; init; } = Enumerable.Empty<BidDTO>();
    }
}
