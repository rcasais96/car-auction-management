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
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace CarAuction.Tests.Integration
{
    public class ConcurrencyTests
    {
        private readonly InMemoryVehicleRepository _vehicleRepo = new();
        private readonly InMemoryAuctionRepository _auctionRepo = new();
        private readonly IVehicleService _vehicleService;
        private readonly IAuctionService _auctionService;

        public ConcurrencyTests()
        {
            var mockL1 = new Mock<IMemoryCache>();
            var mockL2 = new Mock<IDistributedCache>();


            _vehicleService = new VehicleService(_vehicleRepo, new InMemoryUnitOfWork());
            _auctionService = new AuctionService(_auctionRepo, _vehicleRepo, new InMemoryUnitOfWork());
        }
        // testa que dois leilões não são criados simultaneamente para o mesmo veículo quando já está num leilão ativo
        [Fact]
        public async Task CreateAuction_Concurrent_ShouldOnlyCreateOne()
        {
            var vehicle = await _vehicleService.AddVehicleAsync(new CreateVehicleRequest
            {
                Type = VehicleType.Sedan,
                Manufacturer = "BMW",
                Model = "Series 3",
                Year = 2020,
                StartingBid = 25000m,
                NumberOfDoors = 4
            });

            //  inicia o leilão primeiro — só aí é Active
            var firstAuction = await _auctionService.CreateAuctionAsync(
                new CreateAuctionRequest { VehicleId = vehicle.Id });

            await _auctionService.StartAuctionAsync(firstAuction.Id);

            // agora tenta criar 10 leilões simultâneos — veículo já tem Active
            var exceptions = new ConcurrentBag<Exception>();
            var results = new ConcurrentBag<AuctionDTO>();

            await Parallel.ForEachAsync(
                Enumerable.Range(0, 10),
                async (_, ct) =>
                {
                    try
                    {
                        var auction = await _auctionService.CreateAuctionAsync(
                            new CreateAuctionRequest { VehicleId = vehicle.Id }, ct);
                        results.Add(auction);
                    }
                    catch (Exception ex)
                    {
                        exceptions.Add(ex);
                    }
                });

            // Assert — nenhum leilão criado — todos falharam
            results.Should().BeEmpty();
            exceptions.Should().HaveCount(10);
            exceptions.Should().AllSatisfy(e =>
                e.Should().BeOfType<VehicleAlreadyInActiveAuctionException>());
        }

        // testa que bids concorrentes não corrompem o estado
        [Fact]
        public async Task PlaceBid_Concurrent_ShouldMaintainCorrectHighestBid()
        {
            // Arrange
            var vehicle = await _vehicleService.AddVehicleAsync(new CreateVehicleRequest
            {
                Type = VehicleType.Sedan,
                Manufacturer = "BMW",
                Model = "Series 3",
                Year = 2020,
                StartingBid = 1000m,
                NumberOfDoors = 4
            });

            var auction = await _auctionService.CreateAuctionAsync(
                new CreateAuctionRequest { VehicleId = vehicle.Id });

            await _auctionService.StartAuctionAsync(auction.Id);

            var successfulBids = new ConcurrentBag<decimal>();
            var amounts = Enumerable.Range(1, 20)
                .Select(i => 1000m + (i * 100m)) // 1100, 1200, ..., 3000
                .ToList();

            // Act — 20 threads tentam fazer bid simultaneamente
            await Parallel.ForEachAsync(
                amounts,
                async (amount, ct) =>
                {
                    try
                    {
                        var bid = await _auctionService.PlaceBidAsync(auction.Id,
                            new PlaceBidRequest
                            {
                                BidderId = Guid.NewGuid(),
                                Amount = amount
                            }, ct);
                        successfulBids.Add(bid.Amount);
                    }
                    catch (InvalidBidException)
                    {
                        // esperado — bid mais baixo rejeitado
                    }
                });

            // Assert — estado não corrompido
            var finalAuction = await _auctionService.GetByIdAsync(auction.Id);

            // CurrentHighestBid deve ser o valor mais alto dos bids aceites
            finalAuction.CurrentHighestBid.Should().Be(
                successfulBids.Any() ? successfulBids.Max() : 1000m);

            // todos os bids no histórico são válidos — cada um maior que o anterior
            var bidList = finalAuction.Bids.OrderBy(b => b.Amount).ToList();
            for (int i = 1; i < bidList.Count; i++)
            {
                bidList[i].Amount.Should().BeGreaterThan(bidList[i - 1].Amount);
            }
        }

        // testa que veículos com Id externo não são duplicados sob concorrência
        [Fact]
        public async Task AddVehicle_ConcurrentWithSameExternalId_ShouldOnlyAddOne()
        {
            // Arrange
            var externalId = Guid.NewGuid();
            var request = new CreateVehicleRequest
            {
                Id = externalId,
                Type = VehicleType.Sedan,
                Manufacturer = "BMW",
                Model = "Series 3",
                Year = 2020,
                StartingBid = 25000m,
                NumberOfDoors = 4
            };

            var exceptions = new ConcurrentBag<Exception>();
            var results = new ConcurrentBag<VehicleDTO>();

            // Act — 10 threads tentam adicionar o mesmo veículo
            await Parallel.ForEachAsync(
                Enumerable.Range(0, 10),
                async (_, ct) =>
                {
                    try
                    {
                        var vehicle = await _vehicleService.AddVehicleAsync(request, ct);
                        results.Add(vehicle);
                    }
                    catch (Exception ex)
                    {
                        exceptions.Add(ex);
                    }
                });

            // Assert
            results.Should().HaveCount(1);
            exceptions.Should().HaveCount(9);
            exceptions.Should().AllSatisfy(e =>
                e.Should().BeOfType<DuplicateVehicleException>());
        }
    }
}
