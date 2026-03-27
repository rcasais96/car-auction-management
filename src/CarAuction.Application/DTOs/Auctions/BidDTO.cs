using System;
using System.Collections.Generic;
using System.Text;

namespace CarAuction.Application.DTOs.Auctions
{
    public class BidDTO
    {
        public Guid Id { get; init; }
        public Guid BidderId { get; init; }
        public decimal Amount { get; init; }
        public DateTime CreatedAt { get; init; }
    }
}
