namespace CarAuction.Application.Exceptions
{
    public class DuplicateVehicleException : Exception
    {
        public Guid VehicleId { get; }

        public DuplicateVehicleException(Guid vehicleId)
            : base($"Vehicle with ID {vehicleId} already exists in inventory")
        {
            VehicleId = vehicleId;
        }
    }
}
