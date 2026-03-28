using CarAuction.Application.DTOs.Auctions;
using CarAuction.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace CarAuction.Api.Controllers
{
    [ApiController]
    [Route("api/auctions")]
    public class AuctionsController : ControllerBase
    {
        private readonly AuctionService _auctionService;

        public AuctionsController(AuctionService auctionService)
        {
            _auctionService = auctionService;
        }

        [HttpPost]
        public async Task<IActionResult> CreateAuction([FromBody] CreateAuctionRequest request, CancellationToken cancellationToken)
        {
            var auction = await _auctionService.CreateAuctionAsync(request, cancellationToken);
            return Ok(auction);
        }

        [HttpPost("{id:guid}/start")]
        public async Task<IActionResult> StartAuction(Guid id, CancellationToken cancellationToken)
        {
            var auction = await _auctionService.StartAuctionAsync(id, cancellationToken);
            return Ok(auction);
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
        {
            var auction = await _auctionService.GetByIdAsync(id, cancellationToken);
            return Ok(auction);
        }

        [HttpPost("{id:guid}/bids")]
        public async Task<IActionResult> PlaceBid(Guid id, [FromBody] PlaceBidRequest request, CancellationToken cancellationToken)
        {
            var bid = await _auctionService.PlaceBidAsync(id, request, cancellationToken);
            return Ok(bid);
        }

        [HttpPost("{id:guid}/close")]
        public async Task<IActionResult> CloseAuction(Guid id, CancellationToken cancellationToken)
        {
            await _auctionService.CloseAuctionAsync(id, cancellationToken);
            return NoContent();
        }
    }
}
