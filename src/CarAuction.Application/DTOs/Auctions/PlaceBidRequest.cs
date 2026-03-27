using System;
using System.Collections.Generic;
using System.Text;

namespace CarAuction.Application.DTOs.Auctions
{
    public class PlaceBidRequest
    {
        public Guid BidderId { get; init; }
        public decimal Amount { get; init; }
    }
}
