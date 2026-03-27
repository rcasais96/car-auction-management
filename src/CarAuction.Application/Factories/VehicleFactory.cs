using CarAuction.Application.DTOs.Vehicles;
using CarAuction.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace CarAuction.Application.Factories
{
    public static class VehicleFactory
    {
        public static Vehicle Create(CreateVehicleRequest request) => request.Type switch
        {
            VehicleType.Sedan => new Sedan(
                request.Manufacturer, request.Model,
                request.Year, request.StartingBid,
                request.NumberOfDoors ?? throw new ArgumentException("NumberOfDoors is required for Sedan"),
                request.Id),

            VehicleType.Hatchback => new Hatchback(
                request.Manufacturer, request.Model,
                request.Year, request.StartingBid,
                request.NumberOfDoors ?? throw new ArgumentException("NumberOfDoors is required for Hatchback"),
                request.Id),

            VehicleType.Suv => new Suv(
                request.Manufacturer, request.Model,
                request.Year, request.StartingBid,
                request.NumberOfSeats ?? throw new ArgumentException("NumberOfSeats is required for SUV"),
                request.Id),

            VehicleType.Truck => new Truck(
                request.Manufacturer, request.Model,
                request.Year, request.StartingBid,
                request.LoadCapacity ?? throw new ArgumentException("LoadCapacity is required for Truck"),
                request.Id),

            _ => throw new ArgumentException($"Invalid vehicle type: {request.Type}")
        };
    }
}
