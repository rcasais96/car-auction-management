using System;
using System.Collections.Generic;
using System.Text;

namespace CarAuction.Application.Exceptions
{
    public class VehicleAlreadyInActiveAuctionException : Exception
    {
        public VehicleAlreadyInActiveAuctionException(Guid vehicleId)
            : base($"There is a current live auction for the vehicle {vehicleId}") { }
    }
}
