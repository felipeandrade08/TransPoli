using System.Timers;
using Transpoli.App.Models;

namespace Transpoli.App.Services;

public sealed class TelemetryService : IDisposable
{
    private readonly System.Timers.Timer _timer = new(250);
    private readonly ScsTelemetryReader _reader = new();
    private readonly Random _random = new();
    private double _speed = 82;
    private int _rpm = 1420;
    private double _fuel = 984;
    private double _odometer = 128420;
    private double _driven = 327;
    private TimeSpan _driving = TimeSpan.FromHours(3).Add(TimeSpan.FromMinutes(42));

    public TelemetrySnapshot Current { get; private set; } = CreateInitial();
    public event EventHandler<TelemetrySnapshot>? Updated;

    public TelemetryService()
    {
        _timer.Elapsed += OnTick;
        _timer.AutoReset = true;
    }

    public void Start()
    {
        Publish(_reader.TryRead() ?? Simulate());
        _timer.Start();
    }

    private void OnTick(object? sender, ElapsedEventArgs e)
    {
        var telemetry = _reader.TryRead();
        Publish(telemetry ?? Simulate());
    }

    private void Publish(ScsTelemetryData? data)
    {
        if (data is null)
            return;

        var snapshot = data.Connected
            ? new TelemetrySnapshot(
                Connected: true,
                TruckName: string.IsNullOrWhiteSpace(data.TruckName) ? "ETS2" : data.TruckName,
                SpeedKmh: Math.Round(data.SpeedKmh, 0),
                Rpm: (int)Math.Round(data.Rpm),
                Gear: data.Gear,
                CruiseControl: data.CruiseControl,
                EngineOn: data.EngineOn,
                FuelLiters: Math.Round(data.FuelLiters, 1),
                FuelCapacityLiters: Math.Round(data.FuelCapacityLiters, 1),
                OdometerKm: Math.Round(data.OdometerKm, 1),
                Cargo: string.IsNullOrWhiteSpace(data.Cargo) ? "Nenhuma carga" : data.Cargo,
                CargoWeightKg: Math.Round(data.CargoWeightKg, 0),
                Origin: data.Origin,
                Destination: data.Destination,
                TripTotalKm: Math.Round(data.RouteDistanceKm, 1),
                TripDrivenKm: Math.Round(Math.Max(0, data.RouteDistanceKm - data.RouteRemainingKm), 1),
                DrivingTime: _driving,
                RestTime: TimeSpan.Zero,
                EstimatedArrival: DateTime.Now.AddSeconds(Math.Max(0, data.RouteTimeSeconds)))
            : Simulate();

        Current = snapshot;
        Updated?.Invoke(this, Current);
    }

    private ScsTelemetryData Simulate()
    {
        _speed = Math.Clamp(_speed + (_random.NextDouble() - 0.5) * 3, 0, 90);
        _rpm = (int)Math.Clamp(900 + _speed * 6.2 + (_random.NextDouble() - 0.5) * 90, 800, 1900);
        _fuel = Math.Max(0, _fuel - (_speed > 1 ? 0.05 : 0));
        _odometer += _speed / 3600;
        _driven += _speed / 3600;
        _driving += _speed > 1 ? TimeSpan.FromSeconds(0.25) : TimeSpan.Zero;

        return new ScsTelemetryData(
            Connected: false,
            TruckName: "Modo demonstração",
            SpeedKmh: _speed,
            Rpm: _rpm,
            Gear: _speed < 1 ? "N" : "D",
            CruiseControl: false,
            EngineOn: true,
            FuelLiters: _fuel,
            FuelCapacityLiters: 1200,
            OdometerKm: _odometer,
            Cargo: "Carga de demonstração",
            CargoWeightKg: 20000,
            Origin: "Demonstração",
            Destination: "Demonstração",
            RouteDistanceKm: 1048,
            RouteRemainingKm: Math.Max(0, 1048 - _driven),
            RouteTimeSeconds: Math.Max(0, (1048 - _driven) / Math.Max(1, _speed) * 3600));
    }

    private static TelemetrySnapshot CreateInitial() => new(
        Connected: false,
        TruckName: "Aguardando ETS2",
        SpeedKmh: 0,
        Rpm: 0,
        Gear: "N",
        CruiseControl: false,
        EngineOn: false,
        FuelLiters: 0,
        FuelCapacityLiters: 0,
        OdometerKm: 0,
        Cargo: "Nenhuma carga",
        CargoWeightKg: 0,
        Origin: "—",
        Destination: "—",
        TripTotalKm: 0,
        TripDrivenKm: 0,
        DrivingTime: TimeSpan.Zero,
        RestTime: TimeSpan.Zero,
        EstimatedArrival: DateTime.Now);

    public void Dispose()
    {
        _timer.Dispose();
        _reader.Dispose();
    }
}

internal sealed record ScsTelemetryData(
    bool Connected,
    string TruckName,
    double SpeedKmh,
    double Rpm,
    string Gear,
    bool CruiseControl,
    bool EngineOn,
    double FuelLiters,
    double FuelCapacityLiters,
    double OdometerKm,
    string Cargo,
    double CargoWeightKg,
    string Origin,
    string Destination,
    double RouteDistanceKm,
    double RouteRemainingKm,
    double RouteTimeSeconds);
