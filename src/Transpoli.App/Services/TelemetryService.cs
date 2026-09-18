using System.Timers;
using Transpoli.App.Models;

namespace Transpoli.App.Services;

public sealed class TelemetryService : IDisposable
{
    private readonly System.Timers.Timer _timer = new(1000);
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
        Current = Current with { Connected = true };
        Updated?.Invoke(this, Current);
        _timer.Start();
    }

    private void OnTick(object? sender, ElapsedEventArgs e)
    {
        _speed = Math.Clamp(_speed + (_random.NextDouble() - 0.5) * 3, 0, 90);
        _rpm = (int)Math.Clamp(900 + _speed * 6.2 + (_random.NextDouble() - 0.5) * 90, 800, 1900);
        _fuel = Math.Max(0, _fuel - (_speed > 1 ? 0.05 : 0));
        _odometer += _speed / 3600;
        _driven += _speed / 3600;
        _driving += _speed > 1 ? TimeSpan.FromSeconds(1) : TimeSpan.Zero;
        var eta = DateTime.Now.AddHours(Math.Max(0, (1048 - _driven) / Math.Max(1, _speed)));

        Current = Current with
        {
            Connected = true,
            SpeedKmh = Math.Round(_speed, 0),
            Rpm = _rpm,
            FuelLiters = Math.Round(_fuel, 1),
            OdometerKm = Math.Round(_odometer, 1),
            TripDrivenKm = Math.Round(_driven, 1),
            DrivingTime = _driving,
            EstimatedArrival = eta
        };
        Updated?.Invoke(this, Current);
    }

    private static TelemetrySnapshot CreateInitial() => new(
        Connected: false,
        TruckName: "Aguardando ETS2",
        SpeedKmh: 0,
        Rpm: 0,
        Gear: "N",
        CruiseControl: false,
        EngineOn: false,
        FuelLiters: 984,
        FuelCapacityLiters: 1200,
        OdometerKm: 128420,
        Cargo: "Nenhuma carga",
        CargoWeightKg: 0,
        Origin: "—",
        Destination: "—",
        TripTotalKm: 0,
        TripDrivenKm: 0,
        DrivingTime: TimeSpan.Zero,
        RestTime: TimeSpan.Zero,
        EstimatedArrival: DateTime.Now);

    public void Dispose() => _timer.Dispose();
}
