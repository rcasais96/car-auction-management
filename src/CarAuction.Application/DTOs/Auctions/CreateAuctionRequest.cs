using System;
using System.Collections.Generic;
using System.Text;

namespace CarAuction.Application.DTOs.Auctions
{
    public class CreateAuctionRequest
    {
        public Guid VehicleId { get; init; }
    }
}
