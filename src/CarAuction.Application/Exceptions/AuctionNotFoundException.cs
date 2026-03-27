using System;
using System.Collections.Generic;
using System.Text;

namespace CarAuction.Application.Exceptions
{
    public class AuctionNotFoundException : Exception
    {
        public AuctionNotFoundException(Guid id)
            : base($"Auction {id} not found") { }
    }
}
