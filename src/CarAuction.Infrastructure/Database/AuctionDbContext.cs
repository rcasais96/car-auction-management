using CarAuction.Application.Services.Interfaces;
using CarAuction.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CarAuction.Infrastructure.Database
{
    public class AuctionDbContext : DbContext, IUnitOfWork
    {
        public AuctionDbContext(DbContextOptions<AuctionDbContext> options)
            : base(options) { }

        public DbSet<Vehicle> Vehicles { get; set; }
        public DbSet<Auction> Auctions { get; set; }
        public DbSet<Bid> Bids { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(
                typeof(AuctionDbContext).Assembly);
        }
    }
}
