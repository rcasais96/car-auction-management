using CarAuction.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace CarAuction.Infrastructure.Database.Configurations
{
    public class VehicleConfiguration : IEntityTypeConfiguration<Vehicle>
    {
        public void Configure(EntityTypeBuilder<Vehicle> builder)
        {
            builder.ToTable("Vehicles");
            builder.HasKey(v => v.Id);

            builder.Property(v => v.Manufacturer)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(v => v.Model)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(v => v.StartingBid)
                .HasColumnType("decimal(18,2)");

            // TPH — discriminator
            builder.HasDiscriminator(v => v.Type)
                .HasValue<Sedan>(VehicleType.Sedan)
                .HasValue<Hatchback>(VehicleType.Hatchback)
                .HasValue<Suv>(VehicleType.Suv)
                .HasValue<Truck>(VehicleType.Truck);

            // colunas específicas por tipo — nullable
            builder.Property<int?>("NumberOfDoors");
            builder.Property<int?>("NumberOfSeats");
            builder.Property<decimal?>("LoadCapacity")
                .HasColumnType("decimal(18,2)");
        }
    }
}
