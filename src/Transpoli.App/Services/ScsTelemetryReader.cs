using System.IO.MemoryMappedFiles;
using System.Text;

namespace Transpoli.App.Services;

/// <summary>
/// Reader for the SCSTelemetry shared-memory map produced by the
/// RenCloud SCS Telemetry SDK plugin.
/// Map: Local\\SCSTelemetry, size 32 KiB, revision 12 layout.
/// </summary>
internal sealed class ScsTelemetryReader : IDisposable
{
    private const string MapName = "Local\\SCSTelemetry";
    private const int MapSize = 32 * 1024;

    private const int SdkActiveOffset = 0;
    private const int TimeOffset = 8;

    private const int GearOffset = 500 + 0;
    private const int GearDashboardOffset = 504;

    private const int FuelCapacityOffset = 700 + 4;
    private const int CargoMassOffset = 700 + 40;

    private const int SpeedOffset = 700 + 64;
    private const int RpmOffset = 704;
    private const int CruiseControlSpeedOffset = 700 + 84;
    private const int FuelOffset = 700 + 104;
    private const int FuelRangeOffset = 700 + 124;
    private const int OdometerOffset = 700 + 176;
    private const int RouteDistanceOffset = 700 + 180;
    private const int RouteTimeOffset = 700 + 184;

    // Fifth zone starts at 1500. The fields below follow the bool layout in the SDK map.
    private const int EngineEnabledOffset = 1500 + 16;
    private const int CruiseControlOffset = 1500 + 29;
    private const int ParkBrakeOffset = 1500 + 0;

    private const int TruckNameOffset = 2300 + (64 * 3);
    private const int CargoOffset = 2300 + (64 * 4);
    private const int CityDstOffset = 2300 + (64 * 5);
    private const int CitySrcOffset = 2300 + (64 * 10);

    private MemoryMappedFile? _map;
    private MemoryMappedViewAccessor? _view;

    public ScsTelemetryData? TryRead()
    {
        if (!TryOpen())
            return null;

        try
        {
            var sdkActive = _view!.ReadByte(SdkActiveOffset) != 0;
            if (!sdkActive)
                return null;

            var timestamp = _view.ReadInt64(TimeOffset);
            if (timestamp == 0)
                return null;

            var speedMs = _view.ReadSingle(SpeedOffset);
            var rpm = _view.ReadSingle(RpmOffset);
            var fuel = _view.ReadSingle(FuelOffset);
            var fuelCapacity = _view.ReadSingle(FuelCapacityOffset);
            var odometer = _view.ReadSingle(OdometerOffset);
            var routeDistance = _view.ReadSingle(RouteDistanceOffset);
            var routeTime = _view.ReadSingle(RouteTimeOffset);

            return new ScsTelemetryData(
                Connected: true,
                TruckName: ReadString(TruckNameOffset),
                SpeedKmh: speedMs * 3.6,
                Rpm: rpm,
                Gear: FormatGear(_view.ReadInt32(GearDashboardOffset), _view.ReadInt32(GearOffset)),
                CruiseControl: _view.ReadByte(CruiseControlOffset) != 0,
                EngineOn: _view.ReadByte(EngineEnabledOffset) != 0,
                FuelLiters: fuel,
                FuelCapacityLiters: fuelCapacity,
                OdometerKm: odometer,
                Cargo: ReadString(CargoOffset),
                CargoWeightKg: _view.ReadSingle(CargoMassOffset),
                Origin: ReadString(CitySrcOffset),
                Destination: ReadString(CityDstOffset),
                RouteDistanceKm: routeDistance,
                RouteRemainingKm: routeDistance,
                RouteTimeSeconds: routeTime);
        }
        catch
        {
            return null;
        }
    }

    private bool TryOpen()
    {
        if (_view is not null)
            return true;

        try
        {
            _map = MemoryMappedFile.OpenExisting(MapName, MemoryMappedFileRights.Read);
            _view = _map.CreateViewAccessor(0, MapSize, MemoryMappedFileAccess.Read);
            return true;
        }
        catch (FileNotFoundException)
        {
            DisposeMap();
            return false;
        }
        catch (UnauthorizedAccessException)
        {
            DisposeMap();
            return false;
        }
    }

    private string ReadString(int offset)
    {
        Span<byte> buffer = stackalloc byte[64];
        _view!.ReadArray(offset, buffer.ToArray(), 0, buffer.Length);
        var length = buffer.IndexOf((byte)0);
        if (length < 0)
            length = buffer.Length;

        return Encoding.UTF8.GetString(buffer[..length]).Trim();
    }

    private static string FormatGear(int dashboardGear, int gear)
    {
        var value = dashboardGear != 0 ? dashboardGear : gear;
        return value switch
        {
            0 => "N",
            < 0 => $"R{Math.Abs(value)}",
            _ => value.ToString()
        };
    }

    private void DisposeMap()
    {
        _view?.Dispose();
        _view = null;
        _map?.Dispose();
        _map = null;
    }

    public void Dispose() => DisposeMap();
}
