namespace Transpoli.App.Models;

public sealed record TelemetrySnapshot(
    bool Connected,
    string TruckName,
    double SpeedKmh,
    int Rpm,
    string Gear,
    bool CruiseControl,
    bool EngineOn,
    double FuelLiters,
    double FuelCapacityLiters,
    double FuelRangeKm,
    double OdometerKm,
    string Cargo,
    double CargoWeightKg,
    string Origin,
    string Destination,
    double TripTotalKm,
    double TripDrivenKm,
    TimeSpan DrivingTime,
    TimeSpan RestTime,
    DateTime EstimatedArrival);
