using CarAuction.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace CarAuction.Infrastructure.Database.Configurations
{
    public class BidConfiguration : IEntityTypeConfiguration<Bid>
    {
        public void Configure(EntityTypeBuilder<Bid> builder)
        {
            builder.ToTable("Bids");
            builder.HasKey(b => b.Id);
            builder.Property(v => v.Id).ValueGeneratedNever();

            builder.Property(b => b.AuctionId);

            builder.Property(b => b.Amount)
                .HasColumnType("decimal(18,2)");

            builder.HasIndex(b => b.AuctionId);
        }
    }
}
