using System;
using CarAuction.Domain.Entities;
using FluentAssertions;

namespace CarAuction.Tests.Domain
{
    public class VehicleTests
    {
        // ─── Sedan ──────────────────────────────────────────────────────────────

        [Fact]
        public void Sedan_WithValidData_ShouldCreate()
        {
            // Arrange & Act
            var sedan = new Sedan(
                manufacturer: "Toyota",
                model: "Camry",
                year: 2024,
                startingBid: 5000m,
                numberOfDoors: 4
            );

            // Assert
            sedan.Id.Should().NotBe(Guid.Empty);
            sedan.Manufacturer.Should().Be("Toyota");
            sedan.Model.Should().Be("Camry");
            sedan.Year.Should().Be(2024);
            sedan.StartingBid.Should().Be(5000m);
            sedan.NumberOfDoors.Should().Be(4);
            sedan.Type.Should().Be(VehicleType.Sedan);
        }

        [Fact]
        public void Sedan_WithExternalId_ShouldUseProvidedId()
        {
            // Arrange
            var externalId = Guid.NewGuid();

            // Act
            var sedan = new Sedan(
                manufacturer: "Honda",
                model: "Accord",
                year: 2023,
                startingBid: 6000m,
                numberOfDoors: 4,
                id: externalId
            );

            // Assert
            sedan.Id.Should().Be(externalId);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void Sedan_WithInvalidNumberOfDoors_ShouldThrowArgumentException(int invalidDoors)
        {
            // Act
            var act = () => new Sedan("Toyota", "Camry", 2024, 5000m, invalidDoors);

            // Assert
            act.Should().Throw<ArgumentOutOfRangeException>()
                .WithMessage("*Number of doors must be greater than zero*");
        }



        // ─── SUV ────────────────────────────────────────────────────────────────

        [Fact]
        public void Suv_WithValidData_ShouldCreate()
        {
            // Act
            var suv = new Suv(
                manufacturer: "Toyota",
                model: "RAV4",
                year: 2024,
                startingBid: 7000m,
                numberOfSeats: 7
            );

            // Assert
            suv.Id.Should().NotBe(Guid.Empty);
            suv.StartingBid.Should().Be(7000m);
            suv.NumberOfSeats.Should().Be(7);
            suv.Type.Should().Be(VehicleType.Suv);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void Suv_WithInvalidNumberOfSeats_ShouldThrowArgumentOutOfRangeException(int invalidSeats)
        {
            // Act
            var act = () => new Suv("Toyota", "RAV4", 2024, 7000m, invalidSeats);

            // Assert
            act.Should().Throw<ArgumentOutOfRangeException>()
                .WithMessage("*Number of seats must be greater than zero*");
        }

        // ─── Truck ──────────────────────────────────────────────────────────────

        [Fact]
        public void Truck_WithValidData_ShouldCreate()
        {
            // Act
            var truck = new Truck(
                manufacturer: "Ford",
                model: "F-150",
                year: 2024,
                startingBid: 10000m,
                loadCapacity: 1500.5m
            );

            // Assert
            truck.Id.Should().NotBe(Guid.Empty);
            truck.StartingBid.Should().Be(10000m);
            truck.LoadCapacity.Should().Be(1500.5m);
            truck.Type.Should().Be(VehicleType.Truck);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(-100.5)]
        public void Truck_WithInvalidLoadCapacity_ShouldThrowArgumentOutOfRangeException(decimal invalidCapacity)
        {
            // Act
            var act = () => new Truck("Ford", "F-150", 2024, 10000m, invalidCapacity);

            // Assert
            act.Should().Throw<ArgumentOutOfRangeException>()
                .WithMessage("*Load capacity must be greater than zero*");
        }

        // ─── Base Vehicle Validation ────────────────────────────────────────────

        [Theory]
        [InlineData("", "Model", 2024)]
        [InlineData(null, "Model", 2024)]
        [InlineData("   ", "Model", 2024)]
        public void Vehicle_WithInvalidManufacturer_ShouldThrowArgumentException(
            string invalidManufacturer, string model, int year)
        {
            // Act
            var act = () => new Sedan(invalidManufacturer, model, year, 5000m, 4);

            // Assert
            act.Should().Throw<ArgumentException>()
                .WithParameterName("manufacturer");
        }

        [Theory]
        [InlineData("Toyota", "", 2024)]
        [InlineData("Toyota", null, 2024)]
        [InlineData("Toyota", "   ", 2024)]
        public void Vehicle_WithInvalidModel_ShouldThrowArgumentException(
            string manufacturer, string invalidModel, int year)
        {
            // Act
            var act = () => new Sedan(manufacturer, invalidModel, year, 5000m, 4);

            // Assert
            act.Should().Throw<ArgumentException>()
                .WithParameterName("model");
        }

        [Theory]
        [InlineData(1885)] // antes do primeiro carro
        [InlineData(0)]
        [InlineData(-1)]
        public void Vehicle_WithInvalidYear_ShouldThrowArgumentOutOfRangeException(int invalidYear)
        {
            // Act
            var act = () => new Sedan("Toyota", "Camry", invalidYear, 5000m, 4);

            // Assert
            act.Should().Throw<ArgumentOutOfRangeException>()
                .WithParameterName("year");
        }

        [Fact]
        public void Vehicle_WithFutureYear_ShouldThrowArgumentOutOfRangeException()
        {
            // Arrange
            var futureYear = DateTime.UtcNow.Year + 2;

            // Act
            var act = () => new Sedan("Toyota", "Camry", futureYear, 5000m, 4);

            // Assert
            act.Should().Throw<ArgumentOutOfRangeException>()
                .WithParameterName("year");
        }


        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(-100)]
        [InlineData(-0.01)]
        public void Vehicle_WithInvalidStartingBid_ShouldThrowArgumentOutOfRangeException(decimal invalidBid)
        {
            // Act
            var act = () => new Sedan("Toyota", "Camry", 2024, invalidBid, 4);

            // Assert
            act.Should().Throw<ArgumentOutOfRangeException>()
                .WithParameterName("startingBid")
                .WithMessage("*Starting bid must be greater than zero*");
        }


    }
}