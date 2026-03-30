

using CarAuction.Application.DTOs.Vehicles;
using CarAuction.Application.Exceptions;
using CarAuction.Application.Services;
using CarAuction.Application.Services.Interfaces;
using CarAuction.Domain.Entities;
using CarAuction.Infrastructure.Cache;
using CarAuction.Infrastructure.Database;
using CarAuction.Infrastructure.Repositories.InMemory;
using FluentAssertions;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Moq;

namespace CarAuction.Tests.Integration
{
    public class VehicleIntegrationTests
    {
        private readonly InMemoryVehicleRepository _vehicleRepo = new();
        private readonly IVehicleService _vehicleService;

        public VehicleIntegrationTests()
        {
            var mockL1 = new Mock<IMemoryCache>();
            var mockL2 = new Mock<IDistributedCache>();

            _vehicleService = new VehicleService(_vehicleRepo, new InMemoryUnitOfWork());
        }


        private static CreateVehicleRequest DefaultSedanRequest(Guid? id = null) => new()
        {
            Id = id,
            Type = VehicleType.Sedan,
            Manufacturer = "BMW",
            Model = "Series 3",
            Year = 2020,
            StartingBid = 25000m,
            NumberOfDoors = 4
        };


        [Fact]
        public async Task AddVehicle_WithoutExternalId_ShouldGenerateId()
        {
            var result = await _vehicleService.AddVehicleAsync(DefaultSedanRequest());

            result.Id.Should().NotBe(Guid.Empty);
            result.Manufacturer.Should().Be("BMW");
            result.Type.Should().Be(VehicleType.Sedan);
        }

        [Fact]
        public async Task AddVehicle_WithExternalId_ShouldUseProvidedId()
        {
            var externalId = Guid.NewGuid();

            var result = await _vehicleService.AddVehicleAsync(
                DefaultSedanRequest(externalId));

            result.Id.Should().Be(externalId);
        }


        [Fact]
        public async Task AddVehicle_WithDuplicateExternalId_ShouldThrowDuplicateVehicleException()
        {
            var externalId = Guid.NewGuid();

            await _vehicleService.AddVehicleAsync(DefaultSedanRequest(externalId));

            var act = async () => await _vehicleService.AddVehicleAsync(
                DefaultSedanRequest(externalId));

            await act.Should().ThrowAsync<DuplicateVehicleException>()
                .Where(e => e.VehicleId == externalId);
        }

        [Fact]
        public async Task AddVehicle_WithoutExternalId_ShouldNeverThrowDuplicate()
        {
            var vehicle1 = await _vehicleService.AddVehicleAsync(DefaultSedanRequest());
            var vehicle2 = await _vehicleService.AddVehicleAsync(DefaultSedanRequest());

            vehicle1.Id.Should().NotBe(vehicle2.Id);
        }


        [Fact]
        public async Task Search_ByType_ShouldReturnOnlyMatchingType()
        {
            await _vehicleService.AddVehicleAsync(DefaultSedanRequest());
            await _vehicleService.AddVehicleAsync(new CreateVehicleRequest
            {
                Type = VehicleType.Suv,
                Manufacturer = "Toyota",
                Model = "RAV4",
                Year = 2022,
                StartingBid = 35000m,
                NumberOfSeats = 5
            });

            var result = await _vehicleService.SearchVehiclesAsync(
                new VehicleSearchCriteriaDTO { Type = VehicleType.Sedan });

            result.Should().HaveCount(1);
            result.First().Type.Should().Be(VehicleType.Sedan);
        }

        [Fact]
        public async Task Search_ByManufacturer_ShouldReturnOnlyMatchingManufacturer()
        {
            await _vehicleService.AddVehicleAsync(DefaultSedanRequest());
            await _vehicleService.AddVehicleAsync(new CreateVehicleRequest
            {
                Type = VehicleType.Sedan,
                Manufacturer = "Audi",
                Model = "A4",
                Year = 2021,
                StartingBid = 30000m,
                NumberOfDoors = 4
            });

            var result = await _vehicleService.SearchVehiclesAsync(
                new VehicleSearchCriteriaDTO { Manufacturer = "BMW" });

            result.Should().HaveCount(1);
            result.First().Manufacturer.Should().Be("BMW");
        }

        [Fact]
        public async Task Search_ByModel_ShouldReturnOnlyMatchingModel()
        {
            await _vehicleService.AddVehicleAsync(DefaultSedanRequest());
            await _vehicleService.AddVehicleAsync(new CreateVehicleRequest
            {
                Type = VehicleType.Sedan,
                Manufacturer = "BMW",
                Model = "Series 5",
                Year = 2021,
                StartingBid = 35000m,
                NumberOfDoors = 4
            });

            var result = await _vehicleService.SearchVehiclesAsync(
                new VehicleSearchCriteriaDTO { Model = "Series 3" });

            result.Should().HaveCount(1);
            result.First().Model.Should().Be("Series 3");
        }

        [Fact]
        public async Task Search_ByYear_ShouldReturnOnlyMatchingYear()
        {
            await _vehicleService.AddVehicleAsync(DefaultSedanRequest());
            await _vehicleService.AddVehicleAsync(new CreateVehicleRequest
            {
                Type = VehicleType.Sedan,
                Manufacturer = "Audi",
                Model = "A4",
                Year = 2021,
                StartingBid = 30000m,
                NumberOfDoors = 4
            });

            var result = await _vehicleService.SearchVehiclesAsync(
                new VehicleSearchCriteriaDTO { Year = 2020 });

            result.Should().HaveCount(1);
            result.First().Year.Should().Be(2020);
        }

        [Fact]
        public async Task Search_WithNoMatches_ShouldReturnEmptyList()
        {
            await _vehicleService.AddVehicleAsync(DefaultSedanRequest());

            var result = await _vehicleService.SearchVehiclesAsync(
                new VehicleSearchCriteriaDTO { Manufacturer = "NonExistent" });

            result.Should().BeEmpty();
            result.Should().NotBeNull();
        }



        [Fact]
        public async Task GetById_WhenExists_ShouldReturnVehicle()
        {
            var added = await _vehicleService.AddVehicleAsync(DefaultSedanRequest());

            var result = await _vehicleService.GetByIdAsync(added.Id);

            result.Id.Should().Be(added.Id);
            result.Manufacturer.Should().Be("BMW");
        }

        [Fact]
        public async Task GetById_WhenNotFound_ShouldThrowVehicleNotFoundException()
        {
            var act = async () => await _vehicleService.GetByIdAsync(Guid.NewGuid());

            await act.Should().ThrowAsync<VehicleNotFoundException>();
        }
    }
}