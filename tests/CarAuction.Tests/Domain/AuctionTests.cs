using System;
using System.Linq;
using CarAuction.Domain.Entities;
using CarAuction.Domain.Exceptions;
using FluentAssertions;

namespace CarAuction.Tests.Domain
{
    public class AuctionTests
    {
        // ─── Helpers ────────────────────────────────────────────────────────────

        private static Auction CreateAuction(decimal startingBid = 1000m, Guid? vehicleId = null)
            => new(vehicleId ?? Guid.NewGuid(), startingBid);

        // ─── Constructor ────────────────────────────────────────────────────────

        [Fact]
        public void Constructor_WithValidData_ShouldCreateAuction()
        {
            // Arrange
            var vehicleId = Guid.NewGuid();
            var startingBid = 1000m;

            // Act
            var auction = new Auction(vehicleId, startingBid);

            // Assert
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

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(-100)]
        [InlineData(-0.01)]
        public void Constructor_WithInvalidStartingBid_ShouldThrowArgumentOutOfRangeException(decimal invalidBid)
        {
            // Act
            var act = () => new Auction(Guid.NewGuid(), invalidBid);

            // Assert
            act.Should().Throw<ArgumentOutOfRangeException>()
                .WithMessage("*Starting bid must be greater than zero*");
        }

        [Fact]
        public void Constructor_WithEmptyVehicleId_ShouldThrowVehicleIdRequiredException()
        {
            // Act
            var act = () => new Auction(Guid.Empty, 1000m);

            // Assert
            act.Should().Throw<ArgumentException>()
                .WithMessage("*VehicleId*");
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
            auction.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        }

        [Fact]
        public void Start_WhenAlreadyActive_ShouldBeIdempotent()
        {
            // Arrange
            var auction = CreateAuction();
            auction.Start();
            var startedAt = auction.StartedAt;
            var updatedAt = auction.UpdatedAt;

            // Act
            auction.Start();

            // Assert - não deve mudar nada
            auction.Status.Should().Be(AuctionStatus.Active);
            auction.StartedAt.Should().Be(startedAt);
            auction.UpdatedAt.Should().Be(updatedAt); // não atualiza timestamp
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
            auction.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        }

        [Fact]
        public void Close_WhenAlreadyClosed_ShouldBeIdempotent()
        {
            // Arrange
            var auction = CreateAuction();
            auction.Start();
            auction.Close();
            var closedAt = auction.ClosedAt;
            var updatedAt = auction.UpdatedAt;

            // Act
            auction.Close();

            // Assert - não deve mudar nada
            auction.Status.Should().Be(AuctionStatus.Closed);
            auction.ClosedAt.Should().Be(closedAt);
            auction.UpdatedAt.Should().Be(updatedAt);
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
        public void PlaceBid_WhenActiveAndValidAmount_ShouldAddBid()
        {
            // Arrange
            var auction = CreateAuction(startingBid: 1000m);
            auction.Start();
            var bidderId = Guid.NewGuid();
            var bidAmount = 1500m;

            // Act
            auction.PlaceBid(bidderId, bidAmount);

            // Assert
            auction.CurrentHighestBid.Should().Be(bidAmount);
            auction.Bids.Should().HaveCount(1);
            auction.Bids.First().BidderId.Should().Be(bidderId);
            auction.Bids.First().Amount.Should().Be(bidAmount);
            auction.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        }

        [Fact]
        public void PlaceBid_WithMultipleBids_ShouldTrackHighestBid()
        {
            // Arrange
            var auction = CreateAuction(startingBid: 1000m);
            auction.Start();

            // Act
            auction.PlaceBid(Guid.NewGuid(), 1200m);
            auction.PlaceBid(Guid.NewGuid(), 1500m);
            auction.PlaceBid(Guid.NewGuid(), 2000m);

            // Assert
            auction.CurrentHighestBid.Should().Be(2000m);
            auction.Bids.Should().HaveCount(3);
            auction.Bids.Last().Amount.Should().Be(2000m);
        }

        [Fact]
        public void PlaceBid_WhenNotActive_ShouldThrowAuctionNotActiveException()
        {
            // Arrange
            var auction = CreateAuction();
            // Não chama Start() - fica Scheduled

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

        [Theory]
        [InlineData(1000)] // igual ao starting bid
        [InlineData(999)]  // menor que starting bid
        [InlineData(500)]  // muito menor
        [InlineData(0)]    // zero
        [InlineData(-100)] // negativo
        public void PlaceBid_WhenAmountNotHigherThanCurrent_ShouldThrowInvalidBidException(decimal invalidAmount)
        {
            // Arrange
            var auction = CreateAuction(startingBid: 1000m);
            auction.Start();

            // Act
            var act = () => auction.PlaceBid(Guid.NewGuid(), invalidAmount);

            // Assert
            act.Should().Throw<InvalidBidException>()
                .Which.BidAmount.Should().Be(invalidAmount);
        }

        [Fact]
        public void PlaceBid_WhenAmountNotHigherThanPreviousBid_ShouldThrowInvalidBidException()
        {
            // Arrange
            var auction = CreateAuction(startingBid: 1000m);
            auction.Start();
            auction.PlaceBid(Guid.NewGuid(), 1500m);

            // Act - tenta dar lance igual ao anterior
            var act = () => auction.PlaceBid(Guid.NewGuid(), 1500m);

            // Assert
            act.Should().Throw<InvalidBidException>()
                .Which.CurrentHighestBid.Should().Be(1500m);
        }


        [Fact]
        public void Close_ShouldPreserveBids()
        {
            // Arrange
            var auction = CreateAuction(startingBid: 1000m);
            auction.Start();
            auction.PlaceBid(Guid.NewGuid(), 1200m);
            auction.PlaceBid(Guid.NewGuid(), 1500m);

            // Act
            auction.Close();

            // Assert
            auction.Bids.Should().HaveCount(2);
            auction.CurrentHighestBid.Should().Be(1500m);
        }
    }
}