namespace Transpoli.App.Models;

public sealed record TripSession(
    DateTime StartedAt,
    DateTime? FinishedAt,
    double StartOdometerKm,
    double EndOdometerKm,
    string Origin,
    string Destination,
    string Cargo,
    bool IsActive)
{
    public double DistanceKm => Math.Max(0, (FinishedAt.HasValue ? EndOdometerKm : EndOdometerKm) - StartOdometerKm);
    public TimeSpan Duration => (FinishedAt ?? DateTime.Now) - StartedAt;
}
