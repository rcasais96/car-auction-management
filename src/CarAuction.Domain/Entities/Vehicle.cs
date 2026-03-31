namespace CarAuction.Domain.Entities
{
    public abstract class Vehicle
    {
        public Guid Id { get; init; }
        public string Manufacturer { get; init; } = string.Empty;
        public string Model { get; init; } = string.Empty;
        public int Year { get; init; }
        public decimal StartingBid { get; init; }
        public VehicleType Type { get; protected set; }

        protected Vehicle() { }

        protected Vehicle(
            string manufacturer,
            string model,
            int year,
            decimal startingBid,
            Guid? id = null)
        {
            if (string.IsNullOrWhiteSpace(manufacturer))
                throw new ArgumentException("Manufacturer is required", nameof(manufacturer));

            if (string.IsNullOrWhiteSpace(model))
                throw new ArgumentException("Model is required", nameof(model));

            if (year < 1886 || year > DateTime.UtcNow.Year + 1)
                throw new ArgumentOutOfRangeException(nameof(year), "Invalid year");

            if (startingBid <= 0)
                throw new ArgumentOutOfRangeException(nameof(startingBid), "Starting bid must be greater than zero");

            Id = id.HasValue && id.Value != Guid.Empty ? id.Value : Id = Guid.CreateVersion7();
            ;
            Manufacturer = manufacturer;
            Model = model;
            Year = year;
            StartingBid = startingBid;
        }
    }
}
