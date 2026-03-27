using System;
using System.Collections.Generic;
using System.Text;

namespace CarAuction.Domain.Exceptions
{
    public class AuctionNotStartedException : Exception
    {
        public AuctionNotStartedException()
            : base("Cannot close an auction that has not started") { }
    }
}
