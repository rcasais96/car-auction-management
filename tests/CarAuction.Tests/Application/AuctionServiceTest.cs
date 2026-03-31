using CarAuction.Application.DTOs.Auctions;
using CarAuction.Application.Exceptions;
using CarAuction.Application.Services;
using CarAuction.Application.Services.Interfaces;
using CarAuction.Domain.Entities;
using CarAuction.Domain.Exceptions;
using CarAuction.Domain.Repositories;
using FluentAssertions;
using Moq;

namespace CarAuction.Tests.Application
{
    public class AuctionServiceTests
    {
        private readonly Mock<IAuctionRepository> _auctionRepositoryMock;
        private readonly Mock<IVehicleRepository> _vehicleRepositoryMock;
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;

        private readonly IAuctionService _sut;

        public AuctionServiceTests()
        {
            _auctionRepositoryMock = new Mock<IAuctionRepository>();
            _vehicleRepositoryMock = new Mock<IVehicleRepository>();
            _unitOfWorkMock = new Mock<IUnitOfWork>();

            _unitOfWorkMock
        .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
        .ReturnsAsync(0);

            _sut = new AuctionService(
                _auctionRepositoryMock.Object,
                _vehicleRepositoryMock.Object,
                _unitOfWorkMock.Object);
        }

        private static Vehicle CreateVehicle(Guid? id = null)
            => new Sedan("Toyota", "Camry", 2024, 5000m, 4, id);

        private static Auction CreateActiveAuction()
        {
            var auction = new Auction(Guid.NewGuid(), 1000m);
            auction.Start();
            return auction;
        }

        private void SetupVehicle(Guid vehicleId, Vehicle? vehicle)
        {
            _vehicleRepositoryMock
                .Setup(x => x.GetByIdAsync(vehicleId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(vehicle);
        }

        private void SetupAuction(Guid auctionId, Auction? auction)
        {
            _auctionRepositoryMock
                .Setup(x => x.GetByIdAsync(auctionId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(auction);
        }

        private void SetupHasActiveAuction(Guid vehicleId, bool hasActive)
        {
            _auctionRepositoryMock
                .Setup(x => x.HasActiveAuctionAsync(vehicleId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(hasActive);
        }

        private void SetupAddAuction()
        {
            _auctionRepositoryMock
                .Setup(x => x.AddAsync(It.IsAny<Auction>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
        }

        /// <summary>
        /// Verifica que criar um leilão com veículo existente retorna um leilão Scheduled e persiste no repositório.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task CreateAuctionAsync_WithValidVehicle_ShouldCreateScheduledAuction()
        {
            var vehicleId = Guid.NewGuid();
            var vehicle = CreateVehicle(vehicleId);

            SetupVehicle(vehicleId, vehicle);
            SetupHasActiveAuction(vehicleId, false);
            SetupAddAuction();

            var result = await _sut.CreateAuctionAsync(
                new CreateAuctionRequest { VehicleId = vehicleId });

            result.Should().NotBeNull();
            result.Id.Should().NotBe(Guid.Empty);
            result.VehicleId.Should().Be(vehicleId);
            result.Status.Should().Be(AuctionStatus.Scheduled);

            _auctionRepositoryMock.Verify(
                x => x.AddAsync(It.IsAny<Auction>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        /// <summary>
        /// Verifica que criar um leilão para um veículo inexistente lança VehicleNotFoundException sem aceder ao repositório de leilões.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task CreateAuctionAsync_WithNonExistentVehicle_ShouldThrowVehicleNotFoundException()
        {
            var vehicleId = Guid.NewGuid();

            SetupVehicle(vehicleId, null);

            var act = async () => await _sut.CreateAuctionAsync(
                new CreateAuctionRequest { VehicleId = vehicleId });

            await act.Should().ThrowAsync<VehicleNotFoundException>()
                .Where(e => e.VehicleId == vehicleId);

            _auctionRepositoryMock.Verify(
                x => x.HasActiveAuctionAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
                Times.Never);

            _auctionRepositoryMock.Verify(
                x => x.AddAsync(It.IsAny<Auction>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }

        /// <summary>
        /// Verifica que criar um leilão para um veículo já em leilão ativo lança VehicleAlreadyInActiveAuctionException sem persistir.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task CreateAuctionAsync_WithVehicleAlreadyInActiveAuction_ShouldThrowException()
        {
            var vehicleId = Guid.NewGuid();
            var vehicle = CreateVehicle(vehicleId);

            SetupVehicle(vehicleId, vehicle);
            SetupHasActiveAuction(vehicleId, true);

            var act = async () => await _sut.CreateAuctionAsync(
                new CreateAuctionRequest { VehicleId = vehicleId });

            await act.Should().ThrowAsync<VehicleAlreadyInActiveAuctionException>()
                .Where(e => e.VehicleId == vehicleId);

            _auctionRepositoryMock.Verify(
                x => x.AddAsync(It.IsAny<Auction>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }

        /// <summary>
        /// Verifica que é possível criar um novo leilão para um veículo cujo leilão anterior foi encerrado.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task CreateAuctionAsync_AfterPreviousAuctionClosed_ShouldAllowNewAuction()
        {
            var vehicleId = Guid.NewGuid();
            var vehicle = CreateVehicle(vehicleId);

            SetupVehicle(vehicleId, vehicle);
            SetupHasActiveAuction(vehicleId, false);
            SetupAddAuction();

            var result = await _sut.CreateAuctionAsync(
                new CreateAuctionRequest { VehicleId = vehicleId });

            result.Should().NotBeNull();
            result.VehicleId.Should().Be(vehicleId);
        }

        /// <summary>
        /// Verifica que iniciar um leilão agendado retorna o leilão com status Active e StartedAt preenchido.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task StartAuctionAsync_WhenScheduled_ShouldReturnActiveAuction()
        {
            var auctionId = Guid.NewGuid();
            var auction = new Auction(Guid.NewGuid(), 1000m);

            SetupAuction(auctionId, auction);

            var result = await _sut.StartAuctionAsync(auctionId);

            result.Status.Should().Be(AuctionStatus.Active);
            result.StartedAt.Should().NotBeNull();
            auction.Status.Should().Be(AuctionStatus.Active);
        }

        /// <summary>
        /// Verifica que iniciar um leilão já ativo não lança exceção.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task StartAuctionAsync_WhenAlreadyActive_ShouldBeIdempotent()
        {
            var auctionId = Guid.NewGuid();
            var auction = new Auction(Guid.NewGuid(), 1000m);
            auction.Start();

            SetupAuction(auctionId, auction);

            var act = async () => await _sut.StartAuctionAsync(auctionId);

            await act.Should().NotThrowAsync();
        }

        /// <summary>
        /// Verifica que tentar iniciar um leilão inexistente lança AuctionNotFoundException.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task StartAuctionAsync_WhenNotFound_ShouldThrowAuctionNotFoundException()
        {
            var auctionId = Guid.NewGuid();

            SetupAuction(auctionId, null);

            var act = async () => await _sut.StartAuctionAsync(auctionId);

            await act.Should().ThrowAsync<AuctionNotFoundException>()
                .Where(e => e.AuctionId == auctionId);
        }

        /// <summary>
        /// Verifica que tentar iniciar um leilão já fechado lança AuctionAlreadyCloseException.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task StartAuctionAsync_WhenClosed_ShouldThrowAuctionAlreadyCloseException()
        {
            var auctionId = Guid.NewGuid();
            var auction = CreateActiveAuction();
            auction.Close();

            SetupAuction(auctionId, auction);

            var act = async () => await _sut.StartAuctionAsync(auctionId);

            await act.Should().ThrowAsync<AuctionAlreadyCloseException>();
        }

        /// <summary>
        /// Verifica que fechar um leilão ativo altera o status para Closed e define o ClosedAt.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task CloseAuctionAsync_WhenActive_ShouldReturnClosedAuction()
        {
            var auctionId = Guid.NewGuid();
            var auction = CreateActiveAuction();

            SetupAuction(auctionId, auction);

            await _sut.CloseAuctionAsync(auctionId);

            auction.Status.Should().Be(AuctionStatus.Closed);
            auction.ClosedAt.Should().NotBeNull();
        }

        /// <summary>
        /// Verifica que fechar um leilão já fechado não lança exceção.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task CloseAuctionAsync_WhenAlreadyClosed_ShouldBeIdempotent()
        {
            var auctionId = Guid.NewGuid();
            var auction = CreateActiveAuction();
            auction.Close();

            SetupAuction(auctionId, auction);

            var act = async () => await _sut.CloseAuctionAsync(auctionId);

            await act.Should().NotThrowAsync();
        }

        /// <summary>
        /// Verifica que tentar fechar um leilão não iniciado lança AuctionNotStartedException.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task CloseAuctionAsync_WhenScheduled_ShouldThrowAuctionNotStartedException()
        {
            var auctionId = Guid.NewGuid();
            var auction = new Auction(Guid.NewGuid(), 1000m);

            SetupAuction(auctionId, auction);

            var act = async () => await _sut.CloseAuctionAsync(auctionId);

            await act.Should().ThrowAsync<AuctionNotStartedException>();
        }

        /// <summary>
        /// Verifica que tentar fechar um leilão inexistente lança AuctionNotFoundException.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task CloseAuctionAsync_WhenNotFound_ShouldThrowAuctionNotFoundException()
        {
            var auctionId = Guid.NewGuid();

            SetupAuction(auctionId, null);

            var act = async () => await _sut.CloseAuctionAsync(auctionId);

            await act.Should().ThrowAsync<AuctionNotFoundException>();
        }

        /// <summary>
        /// Verifica que um lance válido num leilão ativo retorna o DTO do lance e atualiza o lance mais alto.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task PlaceBidAsync_WithValidBid_ShouldReturnBidDTO()
        {
            var auctionId = Guid.NewGuid();
            var bidderId = Guid.NewGuid();
            var auction = CreateActiveAuction();

            SetupAuction(auctionId, auction);

            var result = await _sut.PlaceBidAsync(auctionId,
                new PlaceBidRequest { BidderId = bidderId, Amount = 1500m });

            result.Should().NotBeNull();
            result.Amount.Should().Be(1500m);
            result.BidderId.Should().Be(bidderId);
            auction.CurrentHighestBid.Should().Be(1500m);
        }

        /// <summary>
        /// Verifica que múltiplos lances são todos registados e o lance mais alto é corretamente atualizado.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task PlaceBidAsync_WithMultipleBids_ShouldTrackAllBids()
        {
            var auctionId = Guid.NewGuid();
            var auction = CreateActiveAuction();

            SetupAuction(auctionId, auction);

            await _sut.PlaceBidAsync(auctionId,
                new PlaceBidRequest { BidderId = Guid.NewGuid(), Amount = 1200m });

            await _sut.PlaceBidAsync(auctionId,
                new PlaceBidRequest { BidderId = Guid.NewGuid(), Amount = 1500m });

            var result = await _sut.PlaceBidAsync(auctionId,
                new PlaceBidRequest { BidderId = Guid.NewGuid(), Amount = 2000m });

            result.Amount.Should().Be(2000m);
            auction.CurrentHighestBid.Should().Be(2000m);
            auction.Bids.Should().HaveCount(3);
        }

        /// <summary>
        /// Verifica que tentar dar um lance num leilão inexistente lança AuctionNotFoundException.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task PlaceBidAsync_WhenAuctionNotFound_ShouldThrowAuctionNotFoundException()
        {
            var auctionId = Guid.NewGuid();

            SetupAuction(auctionId, null);

            var act = async () => await _sut.PlaceBidAsync(auctionId,
                new PlaceBidRequest { BidderId = Guid.NewGuid(), Amount = 1500m });

            await act.Should().ThrowAsync<AuctionNotFoundException>();
        }

        /// <summary>
        /// Verifica que tentar dar um lance num leilão agendado lança AuctionNotActiveException.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task PlaceBidAsync_WhenScheduled_ShouldThrowAuctionNotActiveException()
        {
            var auctionId = Guid.NewGuid();
            var auction = new Auction(Guid.NewGuid(), 1000m);

            SetupAuction(auctionId, auction);

            var act = async () => await _sut.PlaceBidAsync(auctionId,
                new PlaceBidRequest { BidderId = Guid.NewGuid(), Amount = 1500m });

            await act.Should().ThrowAsync<AuctionNotActiveException>();
        }

        /// <summary>
        /// Verifica que tentar dar um lance num leilão fechado lança AuctionNotActiveException.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task PlaceBidAsync_WhenClosed_ShouldThrowAuctionNotActiveException()
        {
            var auctionId = Guid.NewGuid();
            var auction = CreateActiveAuction();
            auction.Close();

            SetupAuction(auctionId, auction);

            var act = async () => await _sut.PlaceBidAsync(auctionId,
                new PlaceBidRequest { BidderId = Guid.NewGuid(), Amount = 1500m });

            await act.Should().ThrowAsync<AuctionNotActiveException>();
        }

        /// <summary>
        /// Verifica que um lance com valor igual ou inferior ao lance atual lança InvalidBidException com os dados corretos.
        /// </summary>
        /// <returns></returns>
        [Theory]
        [InlineData(1000)]
        [InlineData(999)]
        [InlineData(500)]
        [InlineData(0)]
        [InlineData(-100)]
        public async Task PlaceBidAsync_WithInvalidAmount_ShouldThrowInvalidBidException(
            decimal invalidAmount)
        {
            var auctionId = Guid.NewGuid();
            var auction = CreateActiveAuction();

            SetupAuction(auctionId, auction);

            var act = async () => await _sut.PlaceBidAsync(auctionId,
                new PlaceBidRequest { BidderId = Guid.NewGuid(), Amount = invalidAmount });

            await act.Should().ThrowAsync<InvalidBidException>()
                .Where(e => e.BidAmount == invalidAmount && e.CurrentHighestBid == 1000m);
        }

        /// <summary>
        /// Verifica que obter um leilão existente retorna o DTO com os dados corretos.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task GetByIdAsync_WhenExists_ShouldReturnAuctionDTO()
        {
            var auctionId = Guid.NewGuid();
            var auction = new Auction(Guid.NewGuid(), 1000m);

            SetupAuction(auctionId, auction);

            var result = await _sut.GetByIdAsync(auctionId);

            result.Should().NotBeNull();
            result.StartingBid.Should().Be(1000m);
        }

        /// <summary>
        /// Verifica que obter um leilão inexistente lança AuctionNotFoundException com o ID correto.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task GetByIdAsync_WhenNotFound_ShouldThrowAuctionNotFoundException()
        {
            var auctionId = Guid.NewGuid();

            SetupAuction(auctionId, null);

            var act = async () => await _sut.GetByIdAsync(auctionId);

            await act.Should().ThrowAsync<AuctionNotFoundException>()
                .Where(e => e.AuctionId == auctionId);
        }
    }
}
