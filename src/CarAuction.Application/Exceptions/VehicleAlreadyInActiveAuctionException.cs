namespace CarAuction.Application.Exceptions
{
    public class VehicleAlreadyInActiveAuctionException : Exception
    {
        public Guid VehicleId { get; }

        public VehicleAlreadyInActiveAuctionException(Guid vehicleId)
            : base($"Vehicle {vehicleId} already has an active auction. Only one auction can be active per vehicle at a time.")
        {
            VehicleId = vehicleId;
        }
    }
}
