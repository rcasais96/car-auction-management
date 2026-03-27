using System;
using System.Collections.Generic;
using System.Text;

namespace CarAuction.Domain.Exceptions
{
    public class InvalidBidException : Exception
    {
        public InvalidBidException(decimal ammount, decimal CurrentHighestBid)
            : base($"Invalid Bid: current bid ({ammount}) must be higher than {CurrentHighestBid}") { }
    }
}
