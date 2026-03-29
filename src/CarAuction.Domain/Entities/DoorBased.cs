namespace CarAuction.Domain.Entities
{
    public abstract class DoorBasedVehicle : Vehicle
    {
        public int NumberOfDoors { get; }

        protected DoorBasedVehicle(
            string manufacturer,
            string model,
            int year,
            decimal startingBid,
            int numberOfDoors,
            Guid? id = null)
            : base(manufacturer, model, year, startingBid, id)
        {
            if (numberOfDoors <= 0)
                throw new ArgumentOutOfRangeException(nameof(numberOfDoors),
                    "Number of doors must be greater than zero");

            NumberOfDoors = numberOfDoors;
        }
    }
}
