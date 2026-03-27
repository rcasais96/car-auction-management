using System;
using System.Collections.Generic;
using System.Text;

namespace CarAuction.Application.Exceptions
{
    public class VehicleNotFoundException : Exception
    {
        public Guid? VehicleId { get; }

        public VehicleNotFoundException(Guid vehicleId)
            : base($"Vehicle with ID {vehicleId} not found")
        {
            VehicleId = vehicleId;
        }
    }
}
