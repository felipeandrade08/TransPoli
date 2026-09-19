using System.Text.Json;
using Transpoli.App.Models;

namespace Transpoli.App.Services;

public sealed class TripService
{
    private readonly string _filePath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "Transpoli", "trip.json");

    public TripSession? Current { get; private set; }

    public TripService()
    {
        Load();
    }

    public TripSession Start(TelemetrySnapshot telemetry)
    {
        Current = new TripSession(
            DateTime.Now,
            null,
            telemetry.OdometerKm,
            telemetry.OdometerKm,
            telemetry.Origin,
            telemetry.Destination,
            telemetry.Cargo,
            true);
        Save();
        return Current;
    }

    public TripSession? Update(TelemetrySnapshot telemetry)
    {
        if (Current is null || !Current.IsActive)
            return Current;

        Current = Current with { EndOdometerKm = telemetry.OdometerKm };
        Save();
        return Current;
    }

    public TripSession? Finish(TelemetrySnapshot telemetry)
    {
        if (Current is null || !Current.IsActive)
            return Current;

        Current = Current with
        {
            FinishedAt = DateTime.Now,
            EndOdometerKm = telemetry.OdometerKm,
            IsActive = false
        };
        Save();
        return Current;
    }

    private void Load()
    {
        try
        {
            if (File.Exists(_filePath))
                Current = JsonSerializer.Deserialize<TripSession>(File.ReadAllText(_filePath));
        }
        catch
        {
            Current = null;
        }
    }

    private void Save()
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(_filePath)!);
            File.WriteAllText(_filePath, JsonSerializer.Serialize(Current, new JsonSerializerOptions { WriteIndented = true }));
        }
        catch
        {
            // A telemetry UI must remain functional even if local persistence is unavailable.
        }
    }
}
