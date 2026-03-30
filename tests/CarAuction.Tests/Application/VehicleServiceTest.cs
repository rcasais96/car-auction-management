using CarAuction.Application.DTOs.Vehicles;
using CarAuction.Application.Exceptions;
using CarAuction.Application.Services;
using CarAuction.Application.Services.Interfaces;
using CarAuction.Domain;
using CarAuction.Domain.Entities;
using CarAuction.Domain.Repositories;
using FluentAssertions;
using Moq;

namespace CarAuction.Tests.Application
{
    public class VehicleServiceTests
    {
        private readonly Mock<IVehicleRepository> _vehicleRepositoryMock;
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;

        private readonly IVehicleService _sut;

        public VehicleServiceTests()
        {
            _vehicleRepositoryMock = new Mock<IVehicleRepository>();
            _unitOfWorkMock = new Mock<IUnitOfWork>();

            _unitOfWorkMock
           .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
           .ReturnsAsync(0);

        

            _sut = new VehicleService(_vehicleRepositoryMock.Object, _unitOfWorkMock.Object);
        }

        // ─── Helpers ────────────────────────────────────────────────────────────

        private static CreateVehicleRequest ValidSedanRequest(Guid? id = null) => new()
        {
            Id = id,
            Type = VehicleType.Sedan,
            Manufacturer = "Toyota",
            Model = "Camry",
            Year = 2024,
            StartingBid = 5000m,
            NumberOfDoors = 4
        };

        private void SetupAddAsync()
        {
            _vehicleRepositoryMock
                .Setup(x => x.AddAsync(It.IsAny<Vehicle>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
        }

        // ─── AddVehicleAsync ────────────────────────────────────────────────────

        [Fact]
        public async Task AddVehicleAsync_WithValidSedan_ShouldAddAndReturnDto()
        {
            SetupAddAsync();

            var result = await _sut.AddVehicleAsync(ValidSedanRequest());

            result.Should().NotBeNull();
            result.Id.Should().NotBe(Guid.Empty);
            result.Type.Should().Be(VehicleType.Sedan);
            result.Manufacturer.Should().Be("Toyota");
            result.Model.Should().Be("Camry");
            result.Year.Should().Be(2024);

            _vehicleRepositoryMock.Verify(
                x => x.AddAsync(It.IsAny<Sedan>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task AddVehicleAsync_WithValidSuv_ShouldAddAndReturnDto()
        {
            SetupAddAsync();

            var request = new CreateVehicleRequest
            {
                Type = VehicleType.Suv,
                Manufacturer = "Toyota",
                Model = "RAV4",
                Year = 2024,
                StartingBid = 7000m,
                NumberOfSeats = 7
            };

            var result = await _sut.AddVehicleAsync(request);

            result.Type.Should().Be(VehicleType.Suv);
            _vehicleRepositoryMock.Verify(
                x => x.AddAsync(It.IsAny<Suv>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task AddVehicleAsync_WithValidTruck_ShouldAddAndReturnDto()
        {
            SetupAddAsync();

            var request = new CreateVehicleRequest
            {
                Type = VehicleType.Truck,
                Manufacturer = "Ford",
                Model = "F-150",
                Year = 2024,
                StartingBid = 10000m,
                LoadCapacity = 1500m
            };

            var result = await _sut.AddVehicleAsync(request);

            result.Type.Should().Be(VehicleType.Truck);
            _vehicleRepositoryMock.Verify(
                x => x.AddAsync(It.IsAny<Truck>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task AddVehicleAsync_WithValidHatchback_ShouldAddAndReturnDto()
        {
            SetupAddAsync();

            var request = new CreateVehicleRequest
            {
                Type = VehicleType.Hatchback,
                Manufacturer = "VW",
                Model = "Golf",
                Year = 2024,
                StartingBid = 4000m,
                NumberOfDoors = 5
            };

            var result = await _sut.AddVehicleAsync(request);

            result.Type.Should().Be(VehicleType.Hatchback);
            _vehicleRepositoryMock.Verify(
                x => x.AddAsync(It.IsAny<Hatchback>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        // ─── ID Duplication Validation ───────────────────────────────────────────

        [Fact]
        public async Task AddVehicleAsync_WithExistingId_ShouldThrowDuplicateVehicleException()
        {
            var existingId = Guid.NewGuid();

            _vehicleRepositoryMock
                .Setup(x => x.ExistsAsync(existingId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var act = async () => await _sut.AddVehicleAsync(ValidSedanRequest(existingId));

            await act.Should().ThrowAsync<DuplicateVehicleException>()
                .Where(e => e.VehicleId == existingId);

            _vehicleRepositoryMock.Verify(
                x => x.AddAsync(It.IsAny<Vehicle>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public async Task AddVehicleAsync_WithNewExternalId_ShouldAdd()
        {
            var newId = Guid.NewGuid();

            _vehicleRepositoryMock
                .Setup(x => x.ExistsAsync(newId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            SetupAddAsync();

            var result = await _sut.AddVehicleAsync(ValidSedanRequest(newId));

            result.Id.Should().Be(newId);

            _vehicleRepositoryMock.Verify(
                x => x.ExistsAsync(newId, It.IsAny<CancellationToken>()),
                Times.Once);

            _vehicleRepositoryMock.Verify(
                x => x.AddAsync(It.IsAny<Vehicle>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task AddVehicleAsync_WithoutId_ShouldGenerateIdWithoutDuplicateCheck()
        {
            SetupAddAsync();

            var result = await _sut.AddVehicleAsync(ValidSedanRequest());

            result.Id.Should().NotBe(Guid.Empty);

            _vehicleRepositoryMock.Verify(
                x => x.ExistsAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
                Times.Never);

            _vehicleRepositoryMock.Verify(
                x => x.AddAsync(It.IsAny<Vehicle>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        // ─── SearchVehiclesAsync ─────────────────────────────────────────────────

        private void SetupSearch(IEnumerable<Vehicle> vehicles)
        {
            _vehicleRepositoryMock
                .Setup(x => x.SearchAsync(It.IsAny<VehicleSearchCriteria>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(vehicles);
        }

        [Fact]
        public async Task SearchVehiclesAsync_WithNoFilters_ShouldReturnAllVehicles()
        {
            var vehicles = new List<Vehicle>
            {
                new Sedan("Toyota", "Camry", 2024, 5000m, 4),
                new Suv("Honda", "CR-V", 2023, 6000m, 5),
                new Truck("Ford", "F-150", 2024, 10000m, 1000m)
            };

            SetupSearch(vehicles);

            var result = await _sut.SearchVehiclesAsync(new VehicleSearchCriteriaDTO());

            result.Should().HaveCount(3);
        }

        [Fact]
        public async Task SearchVehiclesAsync_WithNoMatches_ShouldReturnEmptyList()
        {
            SetupSearch(Enumerable.Empty<Vehicle>());

            var result = await _sut.SearchVehiclesAsync(
                new VehicleSearchCriteriaDTO { Manufacturer = "NonExistent" });

            result.Should().BeEmpty();
            result.Should().NotBeNull();
        }

        [Fact]
        public async Task SearchVehiclesAsync_FilterByType_ShouldPassCriteriaToRepository()
        {
            SetupSearch(new List<Vehicle>
            {
                new Sedan("Toyota", "Camry", 2024, 5000m, 4),
                new Sedan("Honda", "Accord", 2023, 5500m, 4)
            });

            var result = await _sut.SearchVehiclesAsync(
                new VehicleSearchCriteriaDTO { Type = VehicleType.Sedan });

            result.Should().HaveCount(2);
            result.Should().AllSatisfy(v => v.Type.Should().Be(VehicleType.Sedan));

            // verifica que os critérios foram passados ao repositório
            _vehicleRepositoryMock.Verify(
                x => x.SearchAsync(
                    It.Is<VehicleSearchCriteria>(c => c.Type == VehicleType.Sedan),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task SearchVehiclesAsync_FilterByManufacturer_ShouldPassCriteriaToRepository()
        {
            SetupSearch(new List<Vehicle>
            {
                new Sedan("Toyota", "Camry", 2024, 5000m, 4),
                new Sedan("Toyota", "Corolla", 2023, 4500m, 4)
            });

            var result = await _sut.SearchVehiclesAsync(
                new VehicleSearchCriteriaDTO { Manufacturer = "Toyota" });

            result.Should().HaveCount(2);

            _vehicleRepositoryMock.Verify(
                x => x.SearchAsync(
                    It.Is<VehicleSearchCriteria>(c => c.Manufacturer == "Toyota"),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task SearchVehiclesAsync_FilterByYear_ShouldPassCriteriaToRepository()
        {
            SetupSearch(new List<Vehicle>
            {
                new Sedan("Toyota", "Camry", 2024, 5000m, 4),
                new Sedan("Honda", "Accord", 2024, 5500m, 4)
            });

            var result = await _sut.SearchVehiclesAsync(
                new VehicleSearchCriteriaDTO { Year = 2024 });

            result.Should().HaveCount(2);

            _vehicleRepositoryMock.Verify(
                x => x.SearchAsync(
                    It.Is<VehicleSearchCriteria>(c => c.Year == 2024),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        // ─── GetByIdAsync ────────────────────────────────────────────────────────

        [Fact]
        public async Task GetByIdAsync_WhenVehicleExists_ShouldReturnDto()
        {
            var vehicle = new Sedan("Toyota", "Camry", 2024, 5000m, 4);

            _vehicleRepositoryMock
                .Setup(x => x.GetByIdAsync(vehicle.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(vehicle);

            var result = await _sut.GetByIdAsync(vehicle.Id);

            result.Id.Should().Be(vehicle.Id);
            result.Manufacturer.Should().Be("Toyota");
        }

        [Fact]
        public async Task GetByIdAsync_WhenVehicleNotFound_ShouldThrowVehicleNotFoundException()
        {
            var id = Guid.NewGuid();

            _vehicleRepositoryMock
                .Setup(x => x.GetByIdAsync(id, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Vehicle?)null);

            var act = async () => await _sut.GetByIdAsync(id);

            await act.Should().ThrowAsync<VehicleNotFoundException>()
                .Where(e => e.VehicleId == id);
        }

        // ─── CancellationToken ───────────────────────────────────────────────────

        [Fact]
        public async Task AddVehicleAsync_WhenCancelled_ShouldPropagateCancellation()
        {
            var cts = new CancellationTokenSource();
            cts.Cancel();

            _vehicleRepositoryMock
                .Setup(x => x.AddAsync(It.IsAny<Vehicle>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new OperationCanceledException());

            var act = async () => await _sut.AddVehicleAsync(ValidSedanRequest(), cts.Token);

            await act.Should().ThrowAsync<OperationCanceledException>();
        }
    }
}