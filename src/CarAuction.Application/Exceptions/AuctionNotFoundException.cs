using System;
using System.Collections.Generic;
using System.Text;

namespace CarAuction.Application.Exceptions
{
    public class AuctionNotFoundException : Exception
    {
        public Guid AuctionId { get; }

        public AuctionNotFoundException(Guid auctionId)
            : base($"Auction with ID {auctionId} not found")
        {
            AuctionId = auctionId;
        }
    }
}
