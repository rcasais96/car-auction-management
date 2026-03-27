using CarAuction.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace CarAuction.Application.DTOs.Vehicles
{
    public class VehicleDTO
    {
        public Guid Id { get; init; }
        public string Manufacturer { get; init; } = string.Empty;
        public string Model { get; init; } = string.Empty;
        public int Year { get; init; }
        public decimal StartingBid { get; init; }
        public VehicleType Type { get; init; }


        public int? NumberOfDoors { get; init; }
        public int? NumberOfSeats { get; init; }
        public decimal? LoadCapacity { get; init; }
    }
}
