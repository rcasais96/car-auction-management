using System;
using System.Collections.Generic;
using System.Text;

namespace CarAuction.Domain.Exceptions
{
    public class AuctionAlreadyCloseException : Exception
    {
        public AuctionAlreadyCloseException()
            : base("Auction already closed") { }
    }
}
