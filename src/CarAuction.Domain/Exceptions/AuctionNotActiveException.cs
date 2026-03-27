using System;
using System.Collections.Generic;
using System.Text;

namespace CarAuction.Domain.Exceptions
{

    public class AuctionNotActiveException : Exception
    {
        public AuctionNotActiveException()
            : base("Auction is not active") { }
    }
}
