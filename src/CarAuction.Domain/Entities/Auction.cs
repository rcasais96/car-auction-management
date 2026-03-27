using CarAuction.Domain.Exceptions;
using System;
using System.Collections.Generic;
using System.Text;

namespace CarAuction.Domain.Entities
{
    public class Auction
    {
        public Guid Id { get; }
        public Guid VehicleId { get; }
        public decimal StartingBid { get; }
        public DateTime? StartedAt { get; private set; }
        public DateTime? ClosedAt { get; private set; }
        public AuctionStatus Status { get; private set; }
        public DateTime CreatedAt { get; }
        public DateTime UpdatedAt { get; private set; }
        public decimal CurrentHighestBid { get; private set; }

        private readonly List<Bid> _bids = new();
        public IReadOnlyCollection<Bid> Bids => _bids.AsReadOnly();

        private Auction() { }

        public Auction(Guid vehicleId, decimal startingBid)
        {
            if (vehicleId == Guid.Empty)
                throw new VehicleIdRequiredException();

            if (startingBid <= 0)
                throw new ArgumentOutOfRangeException(nameof(startingBid),
                    "Starting bid must be greater than zero");

            Id = Guid.NewGuid();
            VehicleId = vehicleId;
            StartingBid = startingBid;
            CurrentHighestBid = startingBid;
            Status = AuctionStatus.Scheduled;
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }

        public void Start()
        {
            if (Status == AuctionStatus.Active)
                return;

            if (Status == AuctionStatus.Closed)
                throw new AuctionAlreadyCloseException();

            Status = AuctionStatus.Active;
            StartedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }

        public void Close()
        {
            if (Status == AuctionStatus.Closed)
                return; 

            if (Status == AuctionStatus.Scheduled)
                throw new AuctionNotStartedException();

            Status = AuctionStatus.Closed;
            ClosedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }

        public void PlaceBid(Guid bidderId, decimal amount)
        {
            if (Status != AuctionStatus.Active)
                throw new AuctionNotActiveException();

            if (amount <= CurrentHighestBid)
                throw new InvalidBidException(amount, CurrentHighestBid);

            var bid = new Bid(Id, bidderId, amount);
            _bids.Add(bid);
            CurrentHighestBid = amount;
            UpdatedAt = DateTime.UtcNow;
        }
    }
}
