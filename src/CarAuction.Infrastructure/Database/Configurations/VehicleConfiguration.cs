using CarAuction.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace CarAuction.Infrastructure.Database.Configurations
{
    // VehicleConfiguration.cs
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

            builder.Property(v => v.Type)
                .HasConversion<string>()
                .HasMaxLength(20);

            builder.HasDiscriminator(v => v.Type)
                .HasValue<Sedan>(VehicleType.Sedan)
                .HasValue<Hatchback>(VehicleType.Hatchback)
                .HasValue<Suv>(VehicleType.Suv)
                .HasValue<Truck>(VehicleType.Truck);
        }
    }

    // SedanConfiguration.cs
    public class SedanConfiguration : IEntityTypeConfiguration<Sedan>
    {
        public void Configure(EntityTypeBuilder<Sedan> builder)
        {
            builder.Property(s => s.NumberOfDoors);
        }
    }

    // HatchbackConfiguration.cs
    public class HatchbackConfiguration : IEntityTypeConfiguration<Hatchback>
    {
        public void Configure(EntityTypeBuilder<Hatchback> builder)
        {
            builder.Property(h => h.NumberOfDoors);
        }
    }

    // SuvConfiguration.cs
    public class SuvConfiguration : IEntityTypeConfiguration<Suv>
    {
        public void Configure(EntityTypeBuilder<Suv> builder)
        {
            builder.Property(s => s.NumberOfSeats);
        }
    }

    // TruckConfiguration.cs
    public class TruckConfiguration : IEntityTypeConfiguration<Truck>
    {
        public void Configure(EntityTypeBuilder<Truck> builder)
        {
            builder.Property(t => t.LoadCapacity)
                .HasColumnType("decimal(18,2)");
        }
    }

}
