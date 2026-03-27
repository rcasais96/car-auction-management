using CarAuction.Application.DTOs.Vehicles;
using CarAuction.Application.Exceptions;
using CarAuction.Application.Factories;
using CarAuction.Application.Utils;
using CarAuction.Domain.Exceptions;
using CarAuction.Domain.Repositories;
using System.Collections.Concurrent;
using System.Threading;

namespace CarAuction.Application.Services
{
    public class VehicleService 
    {
        private readonly IVehicleRepository _vehicleRepository;
        private static readonly ConcurrentDictionary<Guid, SemaphoreSlim> _vehicleLocks = new();
        
        private static SemaphoreSlim GetVehicleLock(Guid? vehicleId)
        {
            if (!vehicleId.HasValue || vehicleId.Value == Guid.Empty)
                return new SemaphoreSlim(1, 1);
            return _vehicleLocks.GetOrAdd(vehicleId.Value, _ => new SemaphoreSlim(1, 1));
        }

        public VehicleService(IVehicleRepository vehicleRepository)
        {
            _vehicleRepository = vehicleRepository;
        }

        public async Task<VehicleDTO> GetByIdAsync(Guid vehicleId, CancellationToken cancellationToken = default)
        {
            var vehicle = await _vehicleRepository.GetByIdAsync(vehicleId, cancellationToken)
            ?? throw new VehicleNotFoundException();

            return Mapper.MapToResponse(vehicle);
        }


        public async Task<VehicleDTO> AddVehicleAsync(CreateVehicleRequest request, CancellationToken cancellationToken = default)
        {
            var vehicleLock = GetVehicleLock(request.Id ?? Guid.NewGuid());

            await vehicleLock.WaitAsync(TimeSpan.FromSeconds(5),cancellationToken);
            try
            {

                if (request.Id.HasValue && request.Id.Value != Guid.Empty)
                {
                    if (await _vehicleRepository.ExistsAsync(request.Id.Value, cancellationToken))
                        throw new DuplicateVehicleException(request.Id.Value);
                }

                var vehicle = VehicleFactory.Create(request);

                await _vehicleRepository.AddAsync(vehicle, cancellationToken);

                return Mapper.MapToResponse(vehicle);
            }
            finally
            {
                vehicleLock.Release();
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
