namespace CarAuction.Domain.Exceptions
{

    public class AuctionNotActiveException : Exception
    {
        public AuctionNotActiveException()
            : base("Cannot place bid on an auction that is not active")
        {
        }
    }
}
