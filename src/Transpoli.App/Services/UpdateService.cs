using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text.Json;

namespace Transpoli.App.Services;

public sealed record UpdateInfo(bool Available, Version CurrentVersion, Version LatestVersion, string DownloadUrl, string ReleaseNotes);

public sealed class UpdateService
{
    private const string LatestReleaseApi = "https://api.github.com/repos/felipeandrade08/TransPoli/releases/latest";
    private const string AssetName = "Transpoli-win-x64.zip";
    private readonly HttpClient _http = new();

    public UpdateService()
    {
        _http.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("Transpoli", CurrentVersion.ToString()));
        _http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.github+json"));
    }

    public Version CurrentVersion =>
        Assembly.GetExecutingAssembly().GetName().Version ?? new Version(0, 1, 0);

    public async Task<UpdateInfo> CheckAsync(CancellationToken cancellationToken = default)
    {
        using var response = await _http.GetAsync(LatestReleaseApi, cancellationToken);
        response.EnsureSuccessStatusCode();

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var json = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);

        var root = json.RootElement;
        var tag = root.GetProperty("tag_name").GetString()?.TrimStart('v') ?? "0.0.0";

        if (!Version.TryParse(tag, out var latest))
            throw new InvalidOperationException($"Versão de release inválida: {tag}");

        var notes = root.TryGetProperty("body", out var body)
            ? body.GetString() ?? string.Empty
            : string.Empty;

        var download = string.Empty;

        if (root.TryGetProperty("assets", out var assets))
        {
            foreach (var asset in assets.EnumerateArray())
            {
                if (string.Equals(asset.GetProperty("name").GetString(), AssetName, StringComparison.OrdinalIgnoreCase))
                {
                    download = asset.GetProperty("browser_download_url").GetString() ?? string.Empty;
                    break;
                }
            }
        }

        return new UpdateInfo(
            latest > CurrentVersion,
            CurrentVersion,
            latest,
            download,
            notes);
    }

    public bool StartUpdate(UpdateInfo info)
    {
        if (!info.Available || string.IsNullOrWhiteSpace(info.DownloadUrl))
            return false;

        var updater = Path.Combine(AppContext.BaseDirectory, "Transpoli.Updater.exe");
        if (!File.Exists(updater))
            return false;

        var target = AppContext.BaseDirectory.TrimEnd(Path.DirectorySeparatorChar);
        var app = Environment.ProcessPath ?? Path.Combine(target, "Transpoli.exe");

        Process.Start(new ProcessStartInfo
        {
            FileName = updater,
            Arguments = $"--pid {Environment.ProcessId} --url \"{info.DownloadUrl}\" --target \"{target}\" --app \"{app}\"",
            UseShellExecute = true,
            WorkingDirectory = target
        });

        return true;
    }
}
