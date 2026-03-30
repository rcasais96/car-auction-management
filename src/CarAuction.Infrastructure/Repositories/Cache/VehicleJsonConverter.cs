using CarAuction.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CarAuction.Infrastructure.Repositories.Cache
{
    public class VehicleJsonConverter : JsonConverter<Vehicle>
    {
        public override Vehicle? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            using var doc = JsonDocument.ParseValue(ref reader);
            var root = doc.RootElement;

            var type = root.GetProperty("$type").GetString();
            var id = Guid.Parse(root.GetProperty("Id").GetString()!);
            var manufacturer = root.GetProperty("Manufacturer").GetString()!;
            var model = root.GetProperty("Model").GetString()!;
            var year = root.GetProperty("Year").GetInt32();
            var startingBid = root.GetProperty("StartingBid").GetDecimal();

            return type switch
            {
                "Sedan" => new Sedan(
                    manufacturer, model, year, startingBid,
                    root.GetProperty("NumberOfDoors").GetInt32(), id),

                "Hatchback" => new Hatchback(
                    manufacturer, model, year, startingBid,
                    root.GetProperty("NumberOfDoors").GetInt32(), id),

                "Suv" => new Suv(
                    manufacturer, model, year, startingBid,
                    root.GetProperty("NumberOfSeats").GetInt32(), id),

                "Truck" => new Truck(
                    manufacturer, model, year, startingBid,
                    root.GetProperty("LoadCapacity").GetDecimal(), id),

                _ => throw new JsonException($"Unknown vehicle type: {type}")
            };
        }

        public override void Write(Utf8JsonWriter writer, Vehicle value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            writer.WriteString("$type", value.Type.ToString());
            writer.WriteString("Id", value.Id.ToString());
            writer.WriteString("Manufacturer", value.Manufacturer);
            writer.WriteString("Model", value.Model);
            writer.WriteNumber("Year", value.Year);
            writer.WriteNumber("StartingBid", value.StartingBid);

            switch (value)
            {
                case Sedan s:
                    writer.WriteNumber("NumberOfDoors", s.NumberOfDoors);
                    break;
                case Hatchback h:
                    writer.WriteNumber("NumberOfDoors", h.NumberOfDoors);
                    break;
                case Suv s:
                    writer.WriteNumber("NumberOfSeats", s.NumberOfSeats);
                    break;
                case Truck t:
                    writer.WriteNumber("LoadCapacity", t.LoadCapacity);
                    break;
            }

            writer.WriteEndObject();
        }
    }
}
