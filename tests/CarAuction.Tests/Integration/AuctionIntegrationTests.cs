using CarAuction.Application.DTOs.Auctions;
using CarAuction.Application.DTOs.Vehicles;
using CarAuction.Application.Exceptions;
using CarAuction.Application.Services;
using CarAuction.Application.Services.Interfaces;
using CarAuction.Domain.Entities;
using CarAuction.Domain.Exceptions;
using CarAuction.Infrastructure.Cache;
using CarAuction.Infrastructure.Database;
using CarAuction.Infrastructure.Repositories.InMemory;
using FluentAssertions;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Moq;

namespace CarAuction.Tests.Integration
{
    public class AuctionIntegrationTests
    {
        private readonly InMemoryVehicleRepository _vehicleRepo = new();
        private readonly InMemoryAuctionRepository _auctionRepo = new();

        private readonly IVehicleService _vehicleService;
        private readonly IAuctionService _auctionService;

        public AuctionIntegrationTests()
        {
            var mockL1 = new Mock<IMemoryCache>();
            var mockL2 = new Mock<IDistributedCache>();


            _vehicleService = new VehicleService(_vehicleRepo, new InMemoryUnitOfWork());
            _auctionService = new AuctionService(_auctionRepo, _vehicleRepo, new InMemoryUnitOfWork());
        }

        private async Task<VehicleDTO> AddDefaultVehicleAsync()
            => await _vehicleService.AddVehicleAsync(new CreateVehicleRequest
            {
                Type = VehicleType.Sedan,
                Manufacturer = "BMW",
                Model = "Series 3",
                Year = 2020,
                StartingBid = 25000m,
                NumberOfDoors = 4
            });

        // ─── Fluxo completo ──────────────────────────────────────────────────────

        // testa o fluxo completo descrito no enunciado
        // add vehicle → create auction → start → bid → close
        [Fact]
        public async Task FullAuctionFlow_ShouldWorkCorrectly()
        {
            // 1. adicionar veículo
            var vehicle = await AddDefaultVehicleAsync();
            vehicle.Id.Should().NotBe(Guid.Empty);

            // 2. criar leilão
            var auction = await _auctionService.CreateAuctionAsync(
                new CreateAuctionRequest { VehicleId = vehicle.Id });

            auction.Status.Should().Be(AuctionStatus.Scheduled);
            auction.VehicleId.Should().Be(vehicle.Id);

            // 3. iniciar leilão
            var started = await _auctionService.StartAuctionAsync(auction.Id);
            started.Status.Should().Be(AuctionStatus.Active);

            // 4. colocar bids
            var bid1 = await _auctionService.PlaceBidAsync(auction.Id,
                new PlaceBidRequest { BidderId = Guid.NewGuid(), Amount = 30000m });

            var bid2 = await _auctionService.PlaceBidAsync(auction.Id,
                new PlaceBidRequest { BidderId = Guid.NewGuid(), Amount = 35000m });

            bid1.Amount.Should().Be(30000m);
            bid2.Amount.Should().Be(35000m);

            // 5. fechar leilão
            await _auctionService.CloseAuctionAsync(auction.Id);

            var closed = await _auctionService.GetByIdAsync(auction.Id);
            closed.Status.Should().Be(AuctionStatus.Closed);
            closed.CurrentHighestBid.Should().Be(35000m);
            closed.Bids.Should().HaveCount(2);
        }

        // ─── Enunciado 4a — duplicado ────────────────────────────────────────────

        [Fact]
        public async Task AddVehicle_WithDuplicateExternalId_ShouldThrowDuplicateVehicleException()
        {
            var externalId = Guid.NewGuid();

            await _vehicleService.AddVehicleAsync(new CreateVehicleRequest
            {
                Id = externalId,
                Type = VehicleType.Sedan,
                Manufacturer = "BMW",
                Model = "Series 3",
                Year = 2020,
                StartingBid = 25000m,
                NumberOfDoors = 4
            });

            var act = async () => await _vehicleService.AddVehicleAsync(new CreateVehicleRequest
            {
                Id = externalId,
                Type = VehicleType.Sedan,
                Manufacturer = "Audi",
                Model = "A4",
                Year = 2021,
                StartingBid = 30000m,
                NumberOfDoors = 4
            });

            await act.Should().ThrowAsync<DuplicateVehicleException>()
                .Where(e => e.VehicleId == externalId);
        }

        // ─── Enunciado 4b — validação de veículo e leilão ───────────────────────

        [Fact]
        public async Task CreateAuction_WithNonExistentVehicle_ShouldThrowVehicleNotFoundException()
        {
            var act = async () => await _auctionService.CreateAuctionAsync(
                new CreateAuctionRequest { VehicleId = Guid.NewGuid() });

            await act.Should().ThrowAsync<VehicleNotFoundException>();
        }

        [Fact]
        public async Task CreateAuction_WhenVehicleAlreadyInActiveAuction_ShouldThrow()
        {
            var vehicle = await AddDefaultVehicleAsync();

            var auction = await _auctionService.CreateAuctionAsync(
                new CreateAuctionRequest { VehicleId = vehicle.Id });

            await _auctionService.StartAuctionAsync(auction.Id);

            var act = async () => await _auctionService.CreateAuctionAsync(
                new CreateAuctionRequest { VehicleId = vehicle.Id });

            await act.Should().ThrowAsync<VehicleAlreadyInActiveAuctionException>();
        }

        [Fact]
        public async Task CreateAuction_AfterPreviousAuctionClosed_ShouldAllowNewAuction()
        {
            var vehicle = await AddDefaultVehicleAsync();

            var auction1 = await _auctionService.CreateAuctionAsync(
                new CreateAuctionRequest { VehicleId = vehicle.Id });

            await _auctionService.StartAuctionAsync(auction1.Id);
            await _auctionService.CloseAuctionAsync(auction1.Id);

            var auction2 = await _auctionService.CreateAuctionAsync(
                new CreateAuctionRequest { VehicleId = vehicle.Id });

            auction2.Id.Should().NotBe(auction1.Id);
            auction2.Status.Should().Be(AuctionStatus.Scheduled);
        }

        // ─── Enunciado 4c — validação de bid ─────────────────────────────────────

        [Fact]
        public async Task PlaceBid_WhenAuctionNotStarted_ShouldThrowAuctionNotActiveException()
        {
            var vehicle = await AddDefaultVehicleAsync();

            var auction = await _auctionService.CreateAuctionAsync(
                new CreateAuctionRequest { VehicleId = vehicle.Id });

            var act = async () => await _auctionService.PlaceBidAsync(auction.Id,
                new PlaceBidRequest { BidderId = Guid.NewGuid(), Amount = 30000m });

            await act.Should().ThrowAsync<AuctionNotActiveException>();
        }

        [Fact]
        public async Task PlaceBid_WithLowerAmount_ShouldThrowInvalidBidException()
        {
            var vehicle = await AddDefaultVehicleAsync();

            var auction = await _auctionService.CreateAuctionAsync(
                new CreateAuctionRequest { VehicleId = vehicle.Id });

            await _auctionService.StartAuctionAsync(auction.Id);

            await _auctionService.PlaceBidAsync(auction.Id,
                new PlaceBidRequest { BidderId = Guid.NewGuid(), Amount = 30000m });

            var act = async () => await _auctionService.PlaceBidAsync(auction.Id,
                new PlaceBidRequest { BidderId = Guid.NewGuid(), Amount = 25000m });

            await act.Should().ThrowAsync<InvalidBidException>();
        }

        // ─── Enunciado 2 — pesquisa ──────────────────────────────────────────────

        [Fact]
        public async Task SearchVehicles_ByType_ShouldReturnCorrectResults()
        {
            await _vehicleService.AddVehicleAsync(new CreateVehicleRequest
            {
                Type = VehicleType.Sedan,
                Manufacturer = "BMW",
                Model = "Series 3",
                Year = 2020,
                StartingBid = 25000m,
                NumberOfDoors = 4
            });

            await _vehicleService.AddVehicleAsync(new CreateVehicleRequest
            {
                Type = VehicleType.Suv,
                Manufacturer = "Toyota",
                Model = "RAV4",
                Year = 2022,
                StartingBid = 35000m,
                NumberOfSeats = 5
            });

            var sedans = await _vehicleService.SearchVehiclesAsync(
                new VehicleSearchCriteriaDTO { Type = VehicleType.Sedan });

            sedans.Should().HaveCount(1);
            sedans.First().Type.Should().Be(VehicleType.Sedan);

            var all = await _vehicleService.SearchVehiclesAsync(
                new VehicleSearchCriteriaDTO());

            all.Should().HaveCount(2);
        }
    }
}