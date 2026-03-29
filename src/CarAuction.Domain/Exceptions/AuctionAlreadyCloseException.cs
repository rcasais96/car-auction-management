namespace CarAuction.Domain.Exceptions
{
    public class AuctionAlreadyCloseException : Exception
    {
        public AuctionAlreadyCloseException()
            : base("Cannot start an auction that is already closed") { }
    }
}
