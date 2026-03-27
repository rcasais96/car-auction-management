using CarAuction.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace CarAuction.Domain
{
    public class VehicleSearchCriteria
    {
        public string? Manufacturer { get; set; }
        public string? Model { get; set; }
        public int? Year { get; set; }
        public VehicleType? Type { get; init; }

    }
}
