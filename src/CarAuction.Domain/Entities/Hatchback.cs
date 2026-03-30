namespace CarAuction.Domain.Entities
{
    public class Hatchback : DoorBasedVehicle
    {

        private Hatchback() { }

        public Hatchback(string manufacturer, string model, int year, decimal startingBid, int numberOfDoors, Guid? id = null)
            : base(manufacturer, model, year, startingBid,numberOfDoors, id)
        {

            Type = VehicleType.Hatchback;
        }
    }
}
