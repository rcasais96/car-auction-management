using System;
using System.Collections.Generic;
using System.Text;

namespace CarAuction.Domain.Entities
{
    public class Suv : Vehicle
    {
        public int NumberOfSeats { get; init; }

        private Suv() { }

        public Suv(string manufacturer, string model, int year, decimal startingBid, int numberOfSeats, Guid? id = null)
            : base(manufacturer, model, year, startingBid,id)
        {
            if (numberOfSeats <= 0)
                throw new ArgumentOutOfRangeException(nameof(numberOfSeats), "Number of seats must be greater than zero");

            NumberOfSeats = numberOfSeats;
            Type = VehicleType.Suv;
        }
    }
}
