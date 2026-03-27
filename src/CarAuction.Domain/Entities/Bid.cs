using System;
using System.Collections.Generic;
using System.Text;

namespace CarAuction.Domain.Entities
{
    public class Bid
    {
        public Guid Id { get; }
        public Guid AuctionId { get; }
        public Guid BidderId { get; }
        public decimal Amount { get; }
        public DateTime CreatedAt { get; }

        public Bid(Guid auctionId, Guid bidderId, decimal amount)
        {
            if(amount <= 0)
                throw new ArgumentOutOfRangeException(nameof(amount), "Bid amount must be greater than zero");

            if(bidderId == Guid.Empty)
                throw new ArgumentException("Bidder ID is required", nameof(bidderId));

            if(auctionId == Guid.Empty)
                throw new ArgumentException("Auction ID is required", nameof(auctionId));

            Id = Guid.NewGuid();
            AuctionId = auctionId;
            BidderId = bidderId;
            Amount = amount;
            CreatedAt = DateTime.UtcNow;
        }
    }
}
