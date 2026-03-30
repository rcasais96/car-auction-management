namespace CarAuction.Domain.Entities
{
    public class Sedan : DoorBasedVehicle
    {

        private Sedan() { }


        public Sedan(string manufacturer, string model, int year, decimal startingBid, int numberOfDoors, Guid? id = null) 
            : base(manufacturer, model, year, startingBid, numberOfDoors,id)
        {

            Type = VehicleType.Sedan;
        }
    }
}
