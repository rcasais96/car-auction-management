namespace CarAuction.Domain.Exceptions
{
    public class AuctionNotStartedException : Exception
    {
        public AuctionNotStartedException()
            : base("Cannot close an auction that has not been started")
        {
        }
    }
}
