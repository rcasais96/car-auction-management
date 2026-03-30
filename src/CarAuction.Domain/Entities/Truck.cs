using System;
using System.Collections.Generic;
using System.Text;

namespace CarAuction.Domain.Entities
{
    public class Truck : Vehicle
    {
        public decimal LoadCapacity { get; init; }
        
        private Truck() { }

        public Truck(string manufacturer, string model, int year, decimal startingBid, decimal loadCapacity, Guid? id = null)
            : base(manufacturer, model, year, startingBid, id)
        {
            if (loadCapacity <= 0)
                throw new ArgumentOutOfRangeException(nameof(loadCapacity), "Load capacity must be greater than zero");

            LoadCapacity = loadCapacity;
            Type = VehicleType.Truck;

        }
    }
}
