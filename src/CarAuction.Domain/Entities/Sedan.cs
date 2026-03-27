namespace CarAuction.Domain.Entities
{
    public class Sedan : DoorBasedVehicle
    {
        public override VehicleType Type => VehicleType.Sedan;

        public Sedan(string manufacturer, string model, int year, decimal startingBid, int numberOfDoors, Guid? id = null) 
            : base(manufacturer, model, year, startingBid, numberOfDoors,id)
        {
            if (numberOfDoors <= 0)
                throw new ArgumentOutOfRangeException(nameof(numberOfDoors), "Number of doors must be greater than zero");

        }
    }
}
