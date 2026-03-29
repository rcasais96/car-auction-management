using CarAuction.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace CarAuction.Application.DTOs.Vehicles
{
    public class VehicleSearchCriteriaDTO
    {
        public string? Manufacturer { get; init; }
        public string? Model { get; init; }
        public int? Year { get; init; }
        public VehicleType? Type { get; init; }

    }
}
