namespace CarAuction.Domain.Entities
{
    public class Sedan : DoorBasedVehicle
    {
        public override VehicleType Type => VehicleType.Sedan;

        public Sedan(string manufacturer, string model, int year, decimal startingBid, int numberOfDoors, Guid? id = null) 
            : base(manufacturer, model, year, startingBid, numberOfDoors,id)
        {
         

        }
    }
}
