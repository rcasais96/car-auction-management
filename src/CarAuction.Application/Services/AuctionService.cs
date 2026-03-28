using CarAuction.Application.DTOs.Auctions;
using CarAuction.Application.Exceptions;
using CarAuction.Application.Services.Interfaces;
using CarAuction.Application.Utils;
using CarAuction.Domain.Entities;
using CarAuction.Domain.Exceptions;
using CarAuction.Domain.Repositories;
using System.Collections.Concurrent;

namespace CarAuction.Application.Services
{
    public class AuctionService : IAuctionService
    {
        private readonly IAuctionRepository _auctionRepository;
        private readonly IVehicleRepository _vehicleRepository;

        private static readonly SemaphoreSlim _createAuctionLock = new(1, 1);
        private static readonly ConcurrentDictionary<Guid, SemaphoreSlim> _auctionLocks = new();
        private static SemaphoreSlim GetAuctionLock(Guid auctionId)
        => _auctionLocks.GetOrAdd(auctionId, _ => new SemaphoreSlim(1, 1));

        public AuctionService(
            IAuctionRepository auctionRepository,
            IVehicleRepository vehicleRepository)
        {
            _auctionRepository = auctionRepository;
            _vehicleRepository = vehicleRepository;
        }

        public async Task<AuctionDTO> GetByIdAsync(Guid auctionId, CancellationToken cancellationToken = default)
        {
            var auction = await _auctionRepository.GetByIdAsync(auctionId, cancellationToken)
                ?? throw new AuctionNotFoundException(auctionId);

            return Mapper.MapToResponse(auction);
        }

        public async Task<AuctionDTO> CreateAuctionAsync(CreateAuctionRequest request, CancellationToken cancellationToken = default)
        {
            var vehicle = await _vehicleRepository.GetByIdAsync(request.VehicleId, cancellationToken)
                ?? throw new VehicleNotFoundException(request.VehicleId);

            await _createAuctionLock.WaitAsync(cancellationToken);
            try
            {
                if (await _auctionRepository.HasActiveAuctionAsync(request.VehicleId, cancellationToken))
                    throw new VehicleAlreadyInActiveAuctionException(request.VehicleId);

                var auction = new Auction(vehicle.Id, vehicle.StartingBid);
                await _auctionRepository.AddAsync(auction, cancellationToken);

                return Mapper.MapToResponse(auction);
            }
            finally
            {
                _createAuctionLock.Release();
            }
        }

        public async Task<AuctionDTO> StartAuctionAsync(Guid auctionId, CancellationToken cancellationToken = default)
        {
            var auction = await _auctionRepository.GetByIdAsync(auctionId, cancellationToken)
                ?? throw new AuctionNotFoundException(auctionId);

            auction.Start();

            return Mapper.MapToResponse(auction);
        }

        public async Task<BidDTO> PlaceBidAsync(Guid auctionId, PlaceBidRequest model, CancellationToken cancellationToken = default)
        {
            var auctionLock = GetAuctionLock(auctionId);
            await auctionLock.WaitAsync(cancellationToken);
            try
            {
                var auction = await _auctionRepository.GetByIdAsync(auctionId, cancellationToken)
                 ?? throw new AuctionNotFoundException(auctionId);

                auction.PlaceBid(model.BidderId, model.Amount);
                return Mapper.MapToResponse(auction.Bids.Last());
            }
            finally
            {
                auctionLock.Release();
            }

        }

        public async Task CloseAuctionAsync(Guid auctionId, CancellationToken cancellationToken = default)
        {
            var auction = await _auctionRepository.GetByIdAsync(auctionId, cancellationToken)
                ?? throw new AuctionNotFoundException(auctionId);

            auction.Close();
        }
    }
}
