using System;
using CarAuction.Domain.Entities;
using FluentAssertions;

namespace CarAuction.Tests.Domain
{
    public class VehicleTests
    {
        /// <summary>
        /// Verifica que um Sedan é criado com todos os dados corretos incluindo tipo e número de portas.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public void Sedan_WithValidData_ShouldCreate()
        {
            var sedan = new Sedan(
                manufacturer: "Toyota",
                model: "Camry",
                year: 2024,
                startingBid: 5000m,
                numberOfDoors: 4
            );

            sedan.Id.Should().NotBe(Guid.Empty);
            sedan.Manufacturer.Should().Be("Toyota");
            sedan.Model.Should().Be("Camry");
            sedan.Year.Should().Be(2024);
            sedan.StartingBid.Should().Be(5000m);
            sedan.NumberOfDoors.Should().Be(4);
            sedan.Type.Should().Be(VehicleType.Sedan);
        }

        /// <summary>
        /// Verifica que um Sedan utiliza o ID externo fornecido em vez de gerar um novo.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public void Sedan_WithExternalId_ShouldUseProvidedId()
        {
            var externalId = Guid.NewGuid();

            var sedan = new Sedan(
                manufacturer: "Honda",
                model: "Accord",
                year: 2023,
                startingBid: 6000m,
                numberOfDoors: 4,
                id: externalId
            );

            sedan.Id.Should().Be(externalId);
        }

        /// <summary>
        /// Verifica que criar um Sedan com número de portas inválido lança ArgumentOutOfRangeException.
        /// </summary>
        /// <returns></returns>
        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void Sedan_WithInvalidNumberOfDoors_ShouldThrowArgumentException(int invalidDoors)
        {
            var act = () => new Sedan("Toyota", "Camry", 2024, 5000m, invalidDoors);

            act.Should().Throw<ArgumentOutOfRangeException>()
                .WithMessage("*Number of doors must be greater than zero*");
        }

        /// <summary>
        /// Verifica que um SUV é criado com os dados corretos incluindo número de lugares.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public void Suv_WithValidData_ShouldCreate()
        {
            var suv = new Suv(
                manufacturer: "Toyota",
                model: "RAV4",
                year: 2024,
                startingBid: 7000m,
                numberOfSeats: 7
            );

            suv.Id.Should().NotBe(Guid.Empty);
            suv.StartingBid.Should().Be(7000m);
            suv.NumberOfSeats.Should().Be(7);
            suv.Type.Should().Be(VehicleType.Suv);
        }

        /// <summary>
        /// Verifica que criar um SUV com número de lugares inválido lança ArgumentOutOfRangeException.
        /// </summary>
        /// <returns></returns>
        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void Suv_WithInvalidNumberOfSeats_ShouldThrowArgumentOutOfRangeException(int invalidSeats)
        {
            var act = () => new Suv("Toyota", "RAV4", 2024, 7000m, invalidSeats);

            act.Should().Throw<ArgumentOutOfRangeException>()
                .WithMessage("*Number of seats must be greater than zero*");
        }

        /// <summary>
        /// Verifica que uma Camioneta é criada com os dados corretos incluindo capacidade de carga.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public void Truck_WithValidData_ShouldCreate()
        {
            var truck = new Truck(
                manufacturer: "Ford",
                model: "F-150",
                year: 2024,
                startingBid: 10000m,
                loadCapacity: 1500.5m
            );

            truck.Id.Should().NotBe(Guid.Empty);
            truck.StartingBid.Should().Be(10000m);
            truck.LoadCapacity.Should().Be(1500.5m);
            truck.Type.Should().Be(VehicleType.Truck);
        }

        /// <summary>
        /// Verifica que criar uma Camioneta com capacidade de carga inválida lança ArgumentOutOfRangeException.
        /// </summary>
        /// <returns></returns>
        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(-100.5)]
        public void Truck_WithInvalidLoadCapacity_ShouldThrowArgumentOutOfRangeException(decimal invalidCapacity)
        {
            var act = () => new Truck("Ford", "F-150", 2024, 10000m, invalidCapacity);

            act.Should().Throw<ArgumentOutOfRangeException>()
                .WithMessage("*Load capacity must be greater than zero*");
        }

        /// <summary>
        /// Verifica que criar um veículo com fabricante nulo, vazio ou só espaços lança ArgumentException.
        /// </summary>
        /// <returns></returns>
        [Theory]
        [InlineData("", "Model", 2024)]
        [InlineData(null, "Model", 2024)]
        [InlineData("   ", "Model", 2024)]
        public void Vehicle_WithInvalidManufacturer_ShouldThrowArgumentException(
            string invalidManufacturer, string model, int year)
        {
            var act = () => new Sedan(invalidManufacturer, model, year, 5000m, 4);

            act.Should().Throw<ArgumentException>()
                .WithParameterName("manufacturer");
        }

        /// <summary>
        /// Verifica que criar um veículo com modelo nulo, vazio ou só espaços lança ArgumentException.
        /// </summary>
        /// <returns></returns>
        [Theory]
        [InlineData("Toyota", "", 2024)]
        [InlineData("Toyota", null, 2024)]
        [InlineData("Toyota", "   ", 2024)]
        public void Vehicle_WithInvalidModel_ShouldThrowArgumentException(
            string manufacturer, string invalidModel, int year)
        {
            var act = () => new Sedan(manufacturer, invalidModel, year, 5000m, 4);

            act.Should().Throw<ArgumentException>()
                .WithParameterName("model");
        }

        /// <summary>
        /// Verifica que criar um veículo com ano inválido (antes de 1886, zero ou negativo) lança ArgumentOutOfRangeException.
        /// </summary>
        /// <returns></returns>
        [Theory]
        [InlineData(1885)]
        [InlineData(0)]
        [InlineData(-1)]
        public void Vehicle_WithInvalidYear_ShouldThrowArgumentOutOfRangeException(int invalidYear)
        {
            var act = () => new Sedan("Toyota", "Camry", invalidYear, 5000m, 4);

            act.Should().Throw<ArgumentOutOfRangeException>()
                .WithParameterName("year");
        }

        /// <summary>
        /// Verifica que criar um veículo com ano mais de um ano acima do atual lança ArgumentOutOfRangeException.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public void Vehicle_WithFutureYear_ShouldThrowArgumentOutOfRangeException()
        {
            var futureYear = DateTime.UtcNow.Year + 2;

            var act = () => new Sedan("Toyota", "Camry", futureYear, 5000m, 4);

            act.Should().Throw<ArgumentOutOfRangeException>()
                .WithParameterName("year");
        }

        /// <summary>
        /// Verifica que criar um veículo com lance inicial zero ou negativo lança ArgumentOutOfRangeException.
        /// </summary>
        /// <returns></returns>
        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(-100)]
        [InlineData(-0.01)]
        public void Vehicle_WithInvalidStartingBid_ShouldThrowArgumentOutOfRangeException(decimal invalidBid)
        {
            var act = () => new Sedan("Toyota", "Camry", 2024, invalidBid, 4);

            act.Should().Throw<ArgumentOutOfRangeException>()
                .WithParameterName("startingBid")
                .WithMessage("*Starting bid must be greater than zero*");
        }
    }
}
