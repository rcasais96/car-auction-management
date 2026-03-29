using CarAuction.Application.DTOs.Auctions;
using CarAuction.Application.DTOs.Vehicles;
using CarAuction.Application.Services;
using CarAuction.Application.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace CarAuction.Api.Controllers
{

    [ApiController]
    [Route("api/vehicles")]
    public class VehiclesController : ControllerBase
    {
        private readonly IVehicleService _vehicleService;

        public VehiclesController(IVehicleService vehicleService)
        {
            _vehicleService = vehicleService;
        }

        [HttpPost]
        public async Task<IActionResult> CreateVehicle([FromBody] CreateVehicleRequest request, CancellationToken cancellationToken)
        {
            var vehicle = await _vehicleService.AddVehicleAsync(request, cancellationToken);
            return Ok(vehicle);
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
        {
            var vehicle = await _vehicleService.GetByIdAsync(id, cancellationToken);
            return Ok(vehicle);
        }

        [HttpGet]
        public async Task<IActionResult> Search([FromQuery] VehicleSearchCriteriaDTO model, CancellationToken cancellationToken)
        {
            var vehicles = await _vehicleService.SearchVehiclesAsync(model, cancellationToken);
            return Ok(vehicles);
        }
    }
}
