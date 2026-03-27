using System;
using System.Collections.Generic;
using System.Text;

namespace CarAuction.Application.Exceptions
{
    public class DuplicateVehicleException : Exception
    {
        public DuplicateVehicleException(Guid id)
            : base($"Vehicle {id} already exists") { }
    }
}
