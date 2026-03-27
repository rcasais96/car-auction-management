using System;
using System.Collections.Generic;
using System.Text;

namespace CarAuction.Domain.Exceptions
{
    public class VehicleNotFoundException : Exception
    {
        public VehicleNotFoundException()
            : base("Vehicle not found") { }
    }
}
