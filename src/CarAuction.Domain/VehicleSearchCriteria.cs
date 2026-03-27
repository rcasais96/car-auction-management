using CarAuction.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace CarAuction.Domain
{
    public class VehicleSearchCriteria
    {
        public string? Manufacturer { get;  init; }
        public string? Model { get; init; }
        public int? Year { get; init; }
        public VehicleType? Type { get; init; }

    }
}
