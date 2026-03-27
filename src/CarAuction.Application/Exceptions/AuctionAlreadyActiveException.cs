using System;
using System.Collections.Generic;
using System.Text;

namespace CarAuction.Application.Exceptions
{
    public class AuctionAlreadyActiveException : Exception
    {
        public AuctionAlreadyActiveException()
            : base($"There is a current live auction for the vehicle") { }
    }
}
