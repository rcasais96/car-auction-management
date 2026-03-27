using CarAuction.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace CarAuction.Domain.Repositories
{
    public interface IVehicleRepository
    {
        Task<Vehicle?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default);
        Task AddAsync(Vehicle vehicle, CancellationToken cancellationToken = default);
        Task<IEnumerable<Vehicle>> SearchAsync(VehicleSearchCriteria criteria, CancellationToken cancellationToken = default);
    }


}
