using CarAuction.Application.DTOs.Vehicles;
using System;
using System.Collections.Generic;
using System.Text;

namespace CarAuction.Application.Services.Interfaces
{
    public interface IVehicleService
    {
        Task<VehicleDTO> GetByIdAsync(Guid vehicleId, CancellationToken cancellationToken = default);
        Task<VehicleDTO> AddVehicleAsync(CreateVehicleRequest request, CancellationToken cancellationToken = default);
        Task<IEnumerable<VehicleDTO>> SearchVehiclesAsync(VehicleSearchCriteriaDTO criteria, CancellationToken cancellationToken = default);
    }
}
