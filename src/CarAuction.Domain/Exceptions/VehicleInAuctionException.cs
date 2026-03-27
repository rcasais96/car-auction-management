using System;
using System.Collections.Generic;
using System.Text;

namespace CarAuction.Domain.Exceptions
{
    public class VehicleInAuctionException : Exception
    {
        public VehicleInAuctionException()
            : base("Vehicle already in an auction") { }
    }
}
