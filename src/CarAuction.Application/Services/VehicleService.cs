using CarAuction.Application.DTOs.Vehicles;
using CarAuction.Application.Exceptions;
using CarAuction.Application.Factories;
using CarAuction.Application.Services.Interfaces;
using CarAuction.Application.Utils;
using CarAuction.Domain.Repositories;
using System.Collections.Concurrent;

namespace CarAuction.Application.Services
{
    public class VehicleService : IVehicleService
    {
        private readonly IVehicleRepository _vehicleRepository;
        private static readonly ConcurrentDictionary<Guid, SemaphoreSlim> _vehicleLocks = new();

        public VehicleService(IVehicleRepository vehicleRepository)
        {
            _vehicleRepository = vehicleRepository;
        }

        public async Task<VehicleDTO> GetByIdAsync(Guid vehicleId, CancellationToken cancellationToken = default)
        {
            var vehicle = await _vehicleRepository.GetByIdAsync(vehicleId, cancellationToken)
            ?? throw new VehicleNotFoundException(vehicleId);

            return Mapper.MapToResponse(vehicle);
        }


        public async Task<VehicleDTO> AddVehicleAsync(CreateVehicleRequest request, CancellationToken cancellationToken = default)
        {
            if (request.Id.HasValue && request.Id.Value != Guid.Empty)
            {
                var vehicleLock = _vehicleLocks.GetOrAdd(request.Id.Value, _ => new SemaphoreSlim(1, 1));

                await vehicleLock.WaitAsync(cancellationToken);
                try
                {
                    if (await _vehicleRepository.ExistsAsync(request.Id.Value, cancellationToken))
                        throw new DuplicateVehicleException(request.Id.Value);

                    var vehicle = VehicleFactory.Create(request);
                    await _vehicleRepository.AddAsync(vehicle, cancellationToken);

                    return Mapper.MapToResponse(vehicle);
                }
                finally
                {
                    vehicleLock.Release();
                }
            }
            else
            {
 
                var vehicle = VehicleFactory.Create(request);
                await _vehicleRepository.AddAsync(vehicle, cancellationToken);

                return Mapper.MapToResponse(vehicle);
            }
        }
        public async Task<IEnumerable<VehicleDTO>> SearchVehiclesAsync(VehicleSearchCriteriaDTO criteria, CancellationToken cancellationToken = default)
        {
            var dto = Mapper.MapToResponse(criteria);

            var vehicles = await _vehicleRepository.SearchAsync(dto, cancellationToken);
            return vehicles.Select(Mapper.MapToResponse);
        }

    }
}
