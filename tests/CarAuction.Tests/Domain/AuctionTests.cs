using System;
using System.Linq;
using CarAuction.Domain.Entities;
using CarAuction.Domain.Exceptions;
using FluentAssertions;

namespace CarAuction.Tests.Domain
{
    public class AuctionTests
    {
        private static Auction CreateAuction(decimal startingBid = 1000m, Guid? vehicleId = null)
            => new(vehicleId ?? Guid.NewGuid(), startingBid);

        /// <summary>
        /// Verifica que o construtor cria um leilão com os dados corretos e estado inicial Scheduled.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public void Constructor_WithValidData_ShouldCreateAuction()
        {
            var vehicleId = Guid.NewGuid();
            var startingBid = 1000m;

            var auction = new Auction(vehicleId, startingBid);

            auction.Id.Should().NotBe(Guid.Empty);
            auction.VehicleId.Should().Be(vehicleId);
            auction.StartingBid.Should().Be(startingBid);
            auction.CurrentHighestBid.Should().Be(startingBid);
            auction.Status.Should().Be(AuctionStatus.Scheduled);
            auction.Bids.Should().BeEmpty();
            auction.StartedAt.Should().BeNull();
            auction.ClosedAt.Should().BeNull();
            auction.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
            auction.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        }

        /// <summary>
        /// Verifica que o construtor lança ArgumentOutOfRangeException quando o lance inicial é zero ou negativo.
        /// </summary>
        /// <returns></returns>
        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(-100)]
        [InlineData(-0.01)]
        public void Constructor_WithInvalidStartingBid_ShouldThrowArgumentOutOfRangeException(decimal invalidBid)
        {
            var act = () => new Auction(Guid.NewGuid(), invalidBid);

            act.Should().Throw<ArgumentOutOfRangeException>()
                .WithMessage("*Starting bid must be greater than zero*");
        }

        /// <summary>
        /// Verifica que o construtor lança ArgumentException quando o VehicleId está vazio.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public void Constructor_WithEmptyVehicleId_ShouldThrowVehicleIdRequiredException()
        {
            var act = () => new Auction(Guid.Empty, 1000m);

            act.Should().Throw<ArgumentException>()
                .WithMessage("*VehicleId*");
        }

        /// <summary>
        /// Verifica que iniciar um leilão agendado altera o status para Active e define o StartedAt.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public void Start_WhenScheduled_ShouldSetStatusToActive()
        {
            var auction = CreateAuction();

            auction.Start();

            auction.Status.Should().Be(AuctionStatus.Active);
            auction.StartedAt.Should().NotBeNull();
            auction.StartedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
            auction.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        }

        /// <summary>
        /// Verifica que iniciar um leilão já ativo não altera o estado nem os timestamps.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public void Start_WhenAlreadyActive_ShouldBeIdempotent()
        {
            var auction = CreateAuction();
            auction.Start();
            var startedAt = auction.StartedAt;
            var updatedAt = auction.UpdatedAt;

            auction.Start();

            auction.Status.Should().Be(AuctionStatus.Active);
            auction.StartedAt.Should().Be(startedAt);
            auction.UpdatedAt.Should().Be(updatedAt);
        }

        /// <summary>
        /// Verifica que tentar iniciar um leilão fechado lança AuctionAlreadyCloseException.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public void Start_WhenClosed_ShouldThrowAuctionAlreadyCloseException()
        {
            var auction = CreateAuction();
            auction.Start();
            auction.Close();

            var act = () => auction.Start();

            act.Should().Throw<AuctionAlreadyCloseException>();
        }

        /// <summary>
        /// Verifica que fechar um leilão ativo altera o status para Closed e define o ClosedAt.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public void Close_WhenActive_ShouldSetStatusToClosed()
        {
            var auction = CreateAuction();
            auction.Start();

            auction.Close();

            auction.Status.Should().Be(AuctionStatus.Closed);
            auction.ClosedAt.Should().NotBeNull();
            auction.ClosedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
            auction.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        }

        /// <summary>
        /// Verifica que fechar um leilão já fechado não altera o estado nem os timestamps.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public void Close_WhenAlreadyClosed_ShouldBeIdempotent()
        {
            var auction = CreateAuction();
            auction.Start();
            auction.Close();
            var closedAt = auction.ClosedAt;
            var updatedAt = auction.UpdatedAt;

            auction.Close();

            auction.Status.Should().Be(AuctionStatus.Closed);
            auction.ClosedAt.Should().Be(closedAt);
            auction.UpdatedAt.Should().Be(updatedAt);
        }

        /// <summary>
        /// Verifica que tentar fechar um leilão ainda não iniciado lança AuctionNotStartedException.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public void Close_WhenScheduled_ShouldThrowAuctionNotStartedException()
        {
            var auction = CreateAuction();

            var act = () => auction.Close();

            act.Should().Throw<AuctionNotStartedException>();
        }

        /// <summary>
        /// Verifica que um lance válido num leilão ativo é aceite e atualiza o lance mais alto.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public void PlaceBid_WhenActiveAndValidAmount_ShouldAddBid()
        {
            var auction = CreateAuction(startingBid: 1000m);
            auction.Start();
            var bidderId = Guid.NewGuid();
            var bidAmount = 1500m;

            auction.PlaceBid(bidderId, bidAmount);

            auction.CurrentHighestBid.Should().Be(bidAmount);
            auction.Bids.Should().HaveCount(1);
            auction.Bids.First().BidderId.Should().Be(bidderId);
            auction.Bids.First().Amount.Should().Be(bidAmount);
            auction.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        }

        /// <summary>
        /// Verifica que múltiplos lances são registados e o lance mais alto é corretamente mantido.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public void PlaceBid_WithMultipleBids_ShouldTrackHighestBid()
        {
            var auction = CreateAuction(startingBid: 1000m);
            auction.Start();

            auction.PlaceBid(Guid.NewGuid(), 1200m);
            auction.PlaceBid(Guid.NewGuid(), 1500m);
            auction.PlaceBid(Guid.NewGuid(), 2000m);

            auction.CurrentHighestBid.Should().Be(2000m);
            auction.Bids.Should().HaveCount(3);
            auction.Bids.Last().Amount.Should().Be(2000m);
        }

        /// <summary>
        /// Verifica que tentar dar um lance num leilão agendado (não iniciado) lança AuctionNotActiveException.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public void PlaceBid_WhenNotActive_ShouldThrowAuctionNotActiveException()
        {
            var auction = CreateAuction();

            var act = () => auction.PlaceBid(Guid.NewGuid(), 1500m);

            act.Should().Throw<AuctionNotActiveException>();
        }

        /// <summary>
        /// Verifica que tentar dar um lance num leilão fechado lança AuctionNotActiveException.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public void PlaceBid_WhenClosed_ShouldThrowAuctionNotActiveException()
        {
            var auction = CreateAuction();
            auction.Start();
            auction.Close();

            var act = () => auction.PlaceBid(Guid.NewGuid(), 1500m);

            act.Should().Throw<AuctionNotActiveException>();
        }

        /// <summary>
        /// Verifica que um lance com valor igual ou inferior ao lance atual lança InvalidBidException.
        /// </summary>
        /// <returns></returns>
        [Theory]
        [InlineData(1000)]
        [InlineData(999)]
        [InlineData(500)]
        [InlineData(0)]
        [InlineData(-100)]
        public void PlaceBid_WhenAmountNotHigherThanCurrent_ShouldThrowInvalidBidException(decimal invalidAmount)
        {
            var auction = CreateAuction(startingBid: 1000m);
            auction.Start();

            var act = () => auction.PlaceBid(Guid.NewGuid(), invalidAmount);

            act.Should().Throw<InvalidBidException>()
                .Which.BidAmount.Should().Be(invalidAmount);
        }

        /// <summary>
        /// Verifica que um lance igual ao lance anterior lança InvalidBidException com o lance atual correto.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public void PlaceBid_WhenAmountNotHigherThanPreviousBid_ShouldThrowInvalidBidException()
        {
            var auction = CreateAuction(startingBid: 1000m);
            auction.Start();
            auction.PlaceBid(Guid.NewGuid(), 1500m);

            var act = () => auction.PlaceBid(Guid.NewGuid(), 1500m);

            act.Should().Throw<InvalidBidException>()
                .Which.CurrentHighestBid.Should().Be(1500m);
        }

        /// <summary>
        /// Verifica que fechar um leilão preserva todos os lances registados.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public void Close_ShouldPreserveBids()
        {
            var auction = CreateAuction(startingBid: 1000m);
            auction.Start();
            auction.PlaceBid(Guid.NewGuid(), 1200m);
            auction.PlaceBid(Guid.NewGuid(), 1500m);

            auction.Close();

            auction.Bids.Should().HaveCount(2);
            auction.CurrentHighestBid.Should().Be(1500m);
        }
    }
}
