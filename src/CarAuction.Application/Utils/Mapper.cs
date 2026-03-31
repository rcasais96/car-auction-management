using CarAuction.Application.DTOs.Auctions;
using CarAuction.Application.DTOs.Vehicles;
using CarAuction.Domain;
using CarAuction.Domain.Entities;

namespace CarAuction.Application.Utils
{
    static class Mapper
    {
        public static AuctionDTO MapToResponse(Auction auction) => new()
        {
            Id = auction.Id,
            VehicleId = auction.VehicleId,
            StartingBid = auction.StartingBid,
            CurrentHighestBid = auction.CurrentHighestBid,
            Status = auction.Status,
            StartedAt = auction.StartedAt,
            ClosedAt = auction.ClosedAt,
            CreatedAt = auction.CreatedAt,
            Bids = auction.Bids.Select(b => new BidDTO
            {
                Id = b.Id,
                BidderId = b.BidderId,
                Amount = b.Amount,
                CreatedAt = b.CreatedAt
            }),
            Vehicle = auction.Vehicle == null ? null : MapToResponse(auction.Vehicle)
        };

        public static BidDTO MapToResponse(Bid bid) => new()
        {
            Id = bid.Id,
            BidderId = bid.BidderId,
            Amount = bid.Amount,
            CreatedAt = bid.CreatedAt
        };

        public static VehicleDTO MapToResponse(Vehicle vehicle) => new()
        {
            Id = vehicle.Id,
            Manufacturer = vehicle.Manufacturer,
            Model = vehicle.Model,
            Year = vehicle.Year,
            StartingBid = vehicle.StartingBid,
            Type = vehicle.Type,
            NumberOfDoors = (vehicle as DoorBasedVehicle)?.NumberOfDoors,
            NumberOfSeats = (vehicle as Suv)?.NumberOfSeats,
            LoadCapacity = (vehicle as Truck)?.LoadCapacity
        };

        public static VehicleSearchCriteria MapToResponse(VehicleSearchCriteriaDTO search) => new()
        {
            Manufacturer =  search.Manufacturer,
            Model = search.Model,
            Year = search.Year,
            Type = search.Type

        };
    }
}
