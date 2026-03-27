using CarAuction.Application.DTOs.Auctions;
using CarAuction.Application.DTOs.Vehicles;
using CarAuction.Application.Services;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics.Tracing;

namespace CarAuction.Api.Controllers
{

    [ApiController]
    [Route("api/vehicles")]
    public class VehiclesController : ControllerBase
    {
        private readonly VehicleService _vehicleService;

        public VehiclesController(VehicleService vehicleService)
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
            var auction = await _vehicleService.GetByIdAsync(id, cancellationToken);
            return Ok(auction);
        }

        [HttpGet]
        public async Task<IActionResult> Search([FromQuery] VehicleSearchCriteriaDTO model, CancellationToken cancellationToken)
        {
            var auction = await _vehicleService.SearchVehiclesAsync(model, cancellationToken);
            return Ok(auction);
        }
    }
}
