using System;
using System.Collections.Generic;
using System.Text;

namespace CarAuction.Domain.Exceptions
{
    public class InvalidBidException : Exception
    {
        public decimal BidAmount { get; }
        public decimal CurrentHighestBid { get; }

        public InvalidBidException(decimal bidAmount, decimal currentHighestBid)
            : base($"Invalid bid: bid amount ({bidAmount}) must be higher than current highest bid ({currentHighestBid})")
        {
            BidAmount = bidAmount;
            CurrentHighestBid = currentHighestBid;
        }
    }
}
