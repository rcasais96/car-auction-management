using System;
using System.Collections.Generic;
using System.Text;

namespace CarAuction.Domain.Entities
{
    public class Hatchback : Vehicle
    {
        public int NumberOfDoors { get; }
        public override VehicleType Type => VehicleType.Hatchback;

        public Hatchback(string manufacturer, string model, int year, decimal startingBid, int numberOfDoors, Guid? id = null)
            : base(manufacturer, model, year, startingBid,id)
        {
            if (numberOfDoors <= 0)
                throw new ArgumentOutOfRangeException(nameof(numberOfDoors), "Number of doors must be greater than zero");

            NumberOfDoors = numberOfDoors;
        }
    }
}
