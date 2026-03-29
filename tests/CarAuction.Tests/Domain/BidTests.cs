using CarAuction.Domain.Entities;
using FluentAssertions;

public class BidTests
{
    [Fact]
    public void Constructor_WithValidData_ShouldCreateBid()
    {
        var bid = new Bid(Guid.NewGuid(), Guid.NewGuid(), 1500m);

        bid.Id.Should().NotBe(Guid.Empty);
        bid.Amount.Should().Be(1500m);
        bid.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Constructor_WithInvalidAmount_ShouldThrowArgumentOutOfRangeException(decimal amount)
    {
        var act = () => new Bid(Guid.NewGuid(), Guid.NewGuid(), amount);
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void Constructor_WithEmptyBidderId_ShouldThrowArgumentException()
    {
        var act = () => new Bid(Guid.NewGuid(), Guid.Empty, 1500m);
        act.Should().Throw<ArgumentException>()
            .WithMessage("*bidderId*");
    }

    [Fact]
    public void Constructor_WithEmptyAuctionId_ShouldThrowArgumentException()
    {
        var act = () => new Bid(Guid.Empty, Guid.NewGuid(), 1500m);
        act.Should().Throw<ArgumentException>()
            .WithMessage("*auctionId*");
    }
}