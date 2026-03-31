using System;
using System.Collections.Generic;
using System.Text;

namespace CarAuction.Domain.Entities
{
    public class Bid
    {
        public Guid Id { get; init; }
        public Guid AuctionId { get; init; }
        public Guid BidderId { get; init; }
        public decimal Amount { get; init; }
        public DateTime CreatedAt { get; init; }

        private Bid() { }


        public Bid(Guid auctionId, Guid bidderId, decimal amount)
        {
            if(amount <= 0)
                throw new ArgumentOutOfRangeException(nameof(amount), "Bid amount must be greater than zero");

            if(bidderId == Guid.Empty)
                throw new ArgumentException("Bidder ID is required", nameof(bidderId));

            if(auctionId == Guid.Empty)
                throw new ArgumentException("Auction ID is required", nameof(auctionId));

            Id = Guid.CreateVersion7();
            AuctionId = auctionId;
            BidderId = bidderId;
            Amount = amount;
            CreatedAt = DateTime.UtcNow;
        }
    }
}
