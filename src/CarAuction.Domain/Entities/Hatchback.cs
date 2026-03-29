namespace CarAuction.Domain.Entities
{
    public class Hatchback : DoorBasedVehicle
    {
        public override VehicleType Type => VehicleType.Hatchback;

        public Hatchback(string manufacturer, string model, int year, decimal startingBid, int numberOfDoors, Guid? id = null)
            : base(manufacturer, model, year, startingBid,numberOfDoors, id)
        {
      

        }
    }
}
