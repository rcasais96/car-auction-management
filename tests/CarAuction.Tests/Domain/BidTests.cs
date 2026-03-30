using CarAuction.Domain.Entities;
using FluentAssertions;

public class BidTests
{
    /// <summary>
    /// Verifica que o construtor cria um lance com os dados corretos e define o CreatedAt.
    /// </summary>
    /// <returns></returns>
    [Fact]
    public void Constructor_WithValidData_ShouldCreateBid()
    {
        var bid = new Bid(Guid.NewGuid(), Guid.NewGuid(), 1500m);

        bid.Id.Should().NotBe(Guid.Empty);
        bid.Amount.Should().Be(1500m);
        bid.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    /// <summary>
    /// Verifica que o construtor lança ArgumentOutOfRangeException quando o valor do lance é zero ou negativo.
    /// </summary>
    /// <returns></returns>
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Constructor_WithInvalidAmount_ShouldThrowArgumentOutOfRangeException(decimal amount)
    {
        var act = () => new Bid(Guid.NewGuid(), Guid.NewGuid(), amount);
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    /// <summary>
    /// Verifica que o construtor lança ArgumentException quando o BidderId está vazio.
    /// </summary>
    /// <returns></returns>
    [Fact]
    public void Constructor_WithEmptyBidderId_ShouldThrowArgumentException()
    {
        var act = () => new Bid(Guid.NewGuid(), Guid.Empty, 1500m);
        act.Should().Throw<ArgumentException>()
            .WithMessage("*bidderId*");
    }

    /// <summary>
    /// Verifica que o construtor lança ArgumentException quando o AuctionId está vazio.
    /// </summary>
    /// <returns></returns>
    [Fact]
    public void Constructor_WithEmptyAuctionId_ShouldThrowArgumentException()
    {
        var act = () => new Bid(Guid.Empty, Guid.NewGuid(), 1500m);
        act.Should().Throw<ArgumentException>()
            .WithMessage("*auctionId*");
    }
}
