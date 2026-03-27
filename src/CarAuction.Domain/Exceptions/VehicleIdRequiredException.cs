using System;
using System.Collections.Generic;
using System.Text;

namespace CarAuction.Domain.Exceptions
{
    public class VehicleIdRequiredException : Exception
    {
        public VehicleIdRequiredException()
            : base("VehicleId is required") { }
    }
}
