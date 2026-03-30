using CarAuction.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace CarAuction.Infrastructure.Database.Configurations
{
    public class AuctionConfiguration : IEntityTypeConfiguration<Auction>
    {
        public void Configure(EntityTypeBuilder<Auction> builder)
        {
            builder.ToTable("Auctions");
            builder.HasKey(a => a.Id);

            builder.Property(a => a.StartingBid)
                .HasColumnType("decimal(18,2)");

            builder.Property(a => a.CurrentHighestBid)
                .HasColumnType("decimal(18,2)");

            builder.Property(a => a.Status)
                .HasConversion<string>()
                .HasMaxLength(20);

            builder.HasOne<Vehicle>()
                .WithMany()
                .HasForeignKey(a => a.VehicleId)
                .OnDelete(DeleteBehavior.Restrict);

            // ← liga o Navigation ao HasMany para evitar shadow property
            builder.HasMany(a => a.Bids)
                .WithOne()
                .HasForeignKey(b => b.AuctionId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Navigation(a => a.Bids)
                .HasField("_bids")
                .AutoInclude(false);

            builder.HasIndex(a => new { a.VehicleId, a.Status });
        }
    }
}
