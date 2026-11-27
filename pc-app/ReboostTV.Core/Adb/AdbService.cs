using System.Diagnostics;
using System.Net;
using System.Text.RegularExpressions;
using ReboostTV.Core.Catalog;
using ReboostTV.Core.Models;
using System.Net.Sockets; 
using System.Net.NetworkInformation;

namespace ReboostTV.Core.Adb;

public sealed record AdbResult(bool Success, string Output, string Error);

public sealed class AdbService
{
    private readonly string _adbPath;
    private static readonly Regex SafeTextRegex = new(@"^[a-zA-Z0-9\.\-_@!#\$%\^&\*\(\)\+\=\?\s]+$", RegexOptions.Compiled);

    public AdbService(string adbPath)
    {
        _adbPath = adbPath;
    }

    public async Task<AdbResult> RunAsync(IEnumerable<string> args, int timeoutMs = 15000, CancellationToken ct = default)
    {
        if (!File.Exists(_adbPath))
        {
            return new AdbResult(false, string.Empty, $"Soubor adb.exe nebyl nalezen v: {_adbPath}");
        }

        using var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = _adbPath,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            }
        };

        foreach (var arg in args)
        {
            process.StartInfo.ArgumentList.Add(arg);
        }

        try
        {
            process.Start();
            using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
            timeoutCts.CancelAfter(timeoutMs);

            var outputTask = process.StandardOutput.ReadToEndAsync(timeoutCts.Token);
            var errorTask = process.StandardError.ReadToEndAsync(timeoutCts.Token);

            await process.WaitForExitAsync(timeoutCts.Token);

            return new AdbResult(
                process.ExitCode == 0,
                (await outputTask).Trim(),
                (await errorTask).Trim()
            );
        }
        catch (OperationCanceledException)
        {
            try { process.Kill(entireProcessTree: true); } catch { /* ignore */ }
            return new AdbResult(false, string.Empty, "ADB příkaz vypršel (timeout).");
        }
        catch (Exception ex)
        {
            return new AdbResult(false, string.Empty, ex.Message);
        }
    }

    public Task<AdbResult> ConnectAsync(string ipAddress, int port = 5555)
    {
        if (!IPAddress.TryParse(ipAddress.Trim(), out var parsedIp))
        {
            return Task.FromResult(new AdbResult(false, string.Empty, "Neplatná IP adresa televize."));
        }

        return RunAsync(["connect", $"{parsedIp}:{port}"]);
    }

    public Task<AdbResult> ShellAsync(string targetDevice, params string[] shellArgs)
    {
        var args = new List<string> { "-s", targetDevice, "shell" };
        args.AddRange(shellArgs);
        return RunAsync(args);
    }

    // --- 1-CLICK BOOST AKCE ---

    public async Task<List<string>> ApplyBoostAsync(string device, ReboostConfig config, string apkFolder)
    {
        var log = new List<string>();

        if (config.DebloatBloatware)
        {
            int removed = 0;
            foreach (var pkg in KnownTargets.SafeBloatwarePackages)
            {
                var res = await ShellAsync(device, "pm", "uninstall", "-k", "--user", "0", pkg);
                if (res.Success && res.Output.Contains("Success", StringComparison.OrdinalIgnoreCase))
                {
                    removed++;
                }
            }
            log.Add($"Debloat dokončen: odstraněno {removed} položek balastu.");
        }

        if (config.DeepCacheClean)
        {
            await ShellAsync(device, "pm", "trim-caches", "999G");
            log.Add("Skrytá cache systému vyčištěna.");
        }

        if (config.FastAnimations)
        {
            await ShellAsync(device, "settings", "put", "global", "window_animation_scale", "0.5");
            await ShellAsync(device, "settings", "put", "global", "transition_animation_scale", "0.5");
            await ShellAsync(device, "settings", "put", "global", "animator_duration_scale", "0.5");
            log.Add("Animace zrychleny na 0.5x.");
        }

        if (config.SilentMenu)
        {
            await ShellAsync(device, "settings", "put", "system", "sound_effects_enabled", "0");
            log.Add("Zvuky klikání v menu vypnuty.");
        }

        if (config.DisableSleep)
        {
            await ShellAsync(device, "settings", "put", "system", "screen_off_timeout", "2147483647");
            log.Add("Automatické usínání vypnuto.");
        }

        var dnsHost = KnownTargets.GetPrivateDnsHostname(config.DnsMode);
        if (dnsHost is not null)
        {
            await ShellAsync(device, "settings", "put", "global", "private_dns_mode", "hostname");
            await ShellAsync(device, "settings", "put", "global", "private_dns_specifier", dnsHost);
            log.Add($"Nastaveno Private DNS: {dnsHost}");
        }

        if (config.InstallTurboHud)
        {
            var turboApk = Path.Combine(apkFolder, KnownTargets.LocalApkNames.TurboHud);
            if (File.Exists(turboApk))
            {
                var inst = await RunAsync(["-s", device, "install", "-r", turboApk], timeoutMs: 45000);
                if (inst.Success)
                {
                    await ShellAsync(device, "appops", "set", KnownTargets.TurboPackageName, "SYSTEM_ALERT_WINDOW", "allow");
                    await ShellAsync(device, "appops", "set", KnownTargets.TurboPackageName, "GET_USAGE_STATS", "allow");
                    log.Add("ReboostTV Turbo HUD nainstalován a oprávnění povolena.");
                }
            }
            else
            {
                log.Add("ReboostTV-Turbo.apk nebyl nalezen vedle aplikace (přeskočeno).");
            }
        }

        return log;
    }

    // --- RENTGEN TV & PC OVLADAČ ---

    public Task<AdbResult> KillAllBackgroundAsync(string device) =>
        ShellAsync(device, "am", "kill-all");

    public Task<AdbResult> SendKeyEventAsync(string device, int keyCode) =>
        ShellAsync(device, "input", "keyevent", keyCode.ToString());

    public Task<AdbResult> SendTextAsync(string device, string text)
    {
        if (string.IsNullOrEmpty(text) || !SafeTextRegex.IsMatch(text))
        {
            return Task.FromResult(new AdbResult(false, string.Empty, "Text obsahuje nepodporované znaky."));
        }

        // ADB input text vyžaduje nahradit mezery za %s
        string escaped = text.Replace(" ", "%s");
        return ShellAsync(device, "input", "text", escaped);
    }

    public Task<AdbResult> RunWifiPingTestAsync(string device) =>
        ShellAsync(device, "ping", "-c", "4", "1.1.1.1");

    public Task<AdbResult> GetMemoryInfoAsync(string device) =>
        ShellAsync(device, "cat", "/proc/meminfo");

    public Task<AdbResult> RebootAsync(string device) =>
        RunAsync(["-s", device, "reboot"]);
        /// <summary>
    /// Paralelně proskenuje lokální podsíť (port 5555) a vrátí IP adresy nalezených Android TV.
    /// </summary>
    public async Task<List<string>> ScanNetworkForTvsAsync(int port = 5555, int timeoutMs = 350)
    {
        var foundIps = new System.Collections.Concurrent.ConcurrentBag<string>();
        var localIp = GetLocalIpv4Address();
        if (string.IsNullOrEmpty(localIp)) return [];

        var parts = localIp.Split('.');
        if (parts.Length != 4) return [];
        string subnet = $"{parts[0]}.{parts[1]}.{parts[2]}";

        var tasks = Enumerable.Range(1, 254).Select(async i =>
        {
            string ip = $"{subnet}.{i}";
            try
            {
                using var client = new TcpClient();
                var connectTask = client.ConnectAsync(ip, port);
                if (await Task.WhenAny(connectTask, Task.Delay(timeoutMs)) == connectTask && client.Connected)
                {
                    foundIps.Add(ip);
                }
            }
            catch { /* port zavřený nebo nedostupný */ }
        });

        await Task.WhenAll(tasks);
        return foundIps.OrderBy(ip => ip).ToList();
    }

    private static string? GetLocalIpv4Address()
    {
        foreach (var ni in NetworkInterface.GetAllNetworkInterfaces())
        {
            if (ni.OperationalStatus != OperationalStatus.Up ||
                ni.NetworkInterfaceType == NetworkInterfaceType.Loopback) continue;

            foreach (var ua in ni.GetIPProperties().UnicastAddresses)
            {
                if (ua.Address.AddressFamily == AddressFamily.InterNetwork)
                    return ua.Address.ToString();
            }
        }
        return null;
    }
}