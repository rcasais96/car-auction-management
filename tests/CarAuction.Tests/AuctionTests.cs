using CarAuction.Domain.Entities;
using CarAuction.Domain.Exceptions;
using FluentAssertions;

namespace CarAuction.Tests
{
    public class AuctionTests
    {
        // ─── helpers ────────────────────────────────────────────────────────────

        // factory method — evita repetição na criação de auctions
        private static Auction CreateAuction(decimal startingBid = 1000m)
            => new(Guid.NewGuid(), startingBid);

        // ─── Constructor ────────────────────────────────────────────────────────

        /// <summary>
        /// Verifica que ao criar um Auction com dados válidos, todas as propriedades ficam com os valores corretos
        /// </summary>
        [Fact] // teste simples sem parâmetros
        public void Constructor_WithValidData_ShouldCreateAuction()
        {
            // Arrange
            var vehicleId = Guid.NewGuid();
            var startingBid = 1000m;

            // Act
            var auction = new Auction(vehicleId, startingBid);

            // Assert — FluentAssertions torna as asserções mais legíveis
            auction.Id.Should().NotBe(Guid.Empty);
            auction.VehicleId.Should().Be(vehicleId);
            auction.StartingBid.Should().Be(startingBid);
            auction.CurrentHighestBid.Should().Be(startingBid);
            auction.Status.Should().Be(AuctionStatus.Scheduled);
            auction.Bids.Should().BeEmpty();
            auction.StartedAt.Should().BeNull();
            auction.ClosedAt.Should().BeNull();
        }

  

        // Theory — corre o mesmo teste com múltiplos valores
        // cada InlineData é uma execução separada
        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(-100)]
        public void Constructor_WithInvalidStartingBid_ShouldThrowArgumentOutOfRangeException(
            decimal invalidBid)
        {
            // Act
            var act = () => new Auction(Guid.NewGuid(), invalidBid);

            // Assert
            act.Should().Throw<ArgumentOutOfRangeException>();
        }

        // ─── Start ──────────────────────────────────────────────────────────────

        [Fact]
        public void Start_WhenScheduled_ShouldSetStatusToActive()
        {
            // Arrange
            var auction = CreateAuction();

            // Act
            auction.Start();

            // Assert
            auction.Status.Should().Be(AuctionStatus.Active);
            auction.StartedAt.Should().NotBeNull();
            auction.StartedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        }

        [Fact]
        public void Start_WhenAlreadyActive_ShouldBeIdempotent()
        {
            // Arrange
            var auction = CreateAuction();
            auction.Start();
            var startedAt = auction.StartedAt; // guarda o timestamp original

            // Act — chama Start() uma segunda vez
            auction.Start();

            // Assert — estado não muda, timestamp não muda
            auction.Status.Should().Be(AuctionStatus.Active);
            auction.StartedAt.Should().Be(startedAt); // timestamp original mantido
        }

        [Fact]
        public void Start_WhenClosed_ShouldThrowAuctionAlreadyCloseException()
        {
            // Arrange
            var auction = CreateAuction();
            auction.Start();
            auction.Close();

            // Act
            var act = () => auction.Start();

            // Assert
            act.Should().Throw<AuctionAlreadyCloseException>();
        }

        // ─── Close ──────────────────────────────────────────────────────────────

        [Fact]
        public void Close_WhenActive_ShouldSetStatusToClosed()
        {
            // Arrange
            var auction = CreateAuction();
            auction.Start();

            // Act
            auction.Close();

            // Assert
            auction.Status.Should().Be(AuctionStatus.Closed);
            auction.ClosedAt.Should().NotBeNull();
            auction.ClosedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        }

        [Fact]
        public void Close_WhenAlreadyClosed_ShouldBeIdempotent()
        {
            // Arrange
            var auction = CreateAuction();
            auction.Start();
            auction.Close();
            var closedAt = auction.ClosedAt; // guarda timestamp original

            // Act — chama Close() uma segunda vez
            auction.Close();

            // Assert — estado não muda, timestamp não muda
            auction.Status.Should().Be(AuctionStatus.Closed);
            auction.ClosedAt.Should().Be(closedAt);
        }

        [Fact]
        public void Close_WhenScheduled_ShouldThrowAuctionNotStartedException()
        {
            // Arrange
            var auction = CreateAuction();

            // Act
            var act = () => auction.Close();

            // Assert
            act.Should().Throw<AuctionNotStartedException>();
        }

        // ─── PlaceBid ───────────────────────────────────────────────────────────

        [Fact]
        public void PlaceBid_WhenActive_ShouldAddBidAndUpdateHighestBid()
        {
            // Arrange
            var auction = CreateAuction(startingBid: 1000m);
            auction.Start();
            var bidderId = Guid.NewGuid();

            // Act
            auction.PlaceBid(bidderId, 1500m);

            // Assert
            auction.Bids.Should().HaveCount(1);
            auction.CurrentHighestBid.Should().Be(1500m);
            auction.Bids.First().Amount.Should().Be(1500m);
            auction.Bids.First().BidderId.Should().Be(bidderId);
        }

        [Fact]
        public void PlaceBid_MultipleBids_ShouldTrackAllBidsAndUpdateHighest()
        {
            // Arrange
            var auction = CreateAuction(startingBid: 1000m);
            auction.Start();

            // Act
            auction.PlaceBid(Guid.NewGuid(), 1500m);
            auction.PlaceBid(Guid.NewGuid(), 2000m);
            auction.PlaceBid(Guid.NewGuid(), 2500m);

            // Assert
            auction.Bids.Should().HaveCount(3);
            auction.CurrentHighestBid.Should().Be(2500m);
        }

        [Theory]
        [InlineData(1000, 500)]   // bid menor que starting bid
        [InlineData(1000, 1000)]  // bid igual ao atual
        [InlineData(1500, 1200)]  // bid menor que bid existente
        public void PlaceBid_WithAmountNotHigherThanCurrent_ShouldThrowInvalidBidException(
            decimal currentHighest, decimal newBid)
        {
            // Arrange
            var auction = CreateAuction(startingBid: currentHighest);
            auction.Start();

            // Act
            var act = () => auction.PlaceBid(Guid.NewGuid(), newBid);

            // Assert
            act.Should().Throw<InvalidBidException>();
        }

        [Fact]
        public void PlaceBid_WhenNotActive_ShouldThrowAuctionNotActiveException()
        {
            // Arrange — leilão ainda Scheduled
            var auction = CreateAuction();

            // Act
            var act = () => auction.PlaceBid(Guid.NewGuid(), 1500m);

            // Assert
            act.Should().Throw<AuctionNotActiveException>();
        }

        [Fact]
        public void PlaceBid_WhenClosed_ShouldThrowAuctionNotActiveException()
        {
            // Arrange
            var auction = CreateAuction();
            auction.Start();
            auction.Close();

            // Act
            var act = () => auction.PlaceBid(Guid.NewGuid(), 1500m);

            // Assert
            act.Should().Throw<AuctionNotActiveException>();
        }

        // ─── Concorrência ───────────────────────────────────────────────────────

        [Fact]
        public void PlaceBid_ConcurrentBids_ShouldOnlyAcceptHigherBids()
        {
            // Arrange
            var auction = CreateAuction(startingBid: 1000m);
            auction.Start();

            // Act — simula bids concorrentes
            // em memória não há threading issues, mas testamos a lógica
            auction.PlaceBid(Guid.NewGuid(), 1500m);

            var act = () => auction.PlaceBid(Guid.NewGuid(), 1200m); // bid mais baixo

            // Assert
            act.Should().Throw<InvalidBidException>();
            auction.CurrentHighestBid.Should().Be(1500m); // estado não corrompido
            auction.Bids.Should().HaveCount(1); // só o bid válido
        }

        // ─── Imutabilidade ──────────────────────────────────────────────────────

        [Fact]
        public void Bids_ShouldBeReadOnly_CannotBeModifiedExternally()
        {
            // Arrange
            var auction = CreateAuction();

            // Act & Assert — tenta modificar a coleção de fora
            // IReadOnlyCollection não tem Add() — isto é verificado em compilação
            // mas verificamos que a interface é readonly
            auction.Bids.Should().BeAssignableTo<IReadOnlyCollection<Bid>>();
        }
    }

}
