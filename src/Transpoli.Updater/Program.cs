using System.Diagnostics;
using System.IO.Compression;
using System.Net.Http.Headers;

var argsMap = ParseArgs(args);
if (!argsMap.TryGetValue("pid", out var pidText) || !int.TryParse(pidText, out var pid) ||
    !argsMap.TryGetValue("url", out var url) || !argsMap.TryGetValue("target", out var target) ||
    !argsMap.TryGetValue("app", out var app)) return 2;

try
{
    try
    {
        var process = Process.GetProcessById(pid);
        if (!process.HasExited) process.WaitForExit(15000);
    }
    catch (ArgumentException) { }

    using var http = new HttpClient();
    http.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("Transpoli-Updater", "1.0"));
    var tempZip = Path.Combine(Path.GetTempPath(), $"transpoli-{Guid.NewGuid():N}.zip");
    await using (var input = await http.GetStreamAsync(url))
    await using (var output = File.Create(tempZip))
        await input.CopyToAsync(output);

    var staging = Path.Combine(Path.GetTempPath(), $"transpoli-update-{Guid.NewGuid():N}");
    Directory.CreateDirectory(staging);
    ZipFile.ExtractToDirectory(tempZip, staging);

    var root = staging;
    var nested = Directory.GetDirectories(staging);
    if (nested.Length == 1 && !File.Exists(Path.Combine(staging, "Transpoli.exe"))) root = nested[0];

    foreach (var source in Directory.GetFiles(root, "*", SearchOption.AllDirectories))
    {
        var relative = Path.GetRelativePath(root, source);
        var destination = Path.Combine(target, relative);
        Directory.CreateDirectory(Path.GetDirectoryName(destination)!);
        File.Copy(source, destination, true);
    }

    File.Delete(tempZip);
    Directory.Delete(staging, true);
    Process.Start(new ProcessStartInfo { FileName = app, WorkingDirectory = target, UseShellExecute = true });
    return 0;
}
catch
{
    return 1;
}

static Dictionary<string,string> ParseArgs(string[] args)
{
    var map = new Dictionary<string,string>(StringComparer.OrdinalIgnoreCase);
    for (var i = 0; i + 1 < args.Length; i += 2)
        if (args[i].StartsWith("--")) map[args[i][2..]] = args[i + 1];
    return map;
}
