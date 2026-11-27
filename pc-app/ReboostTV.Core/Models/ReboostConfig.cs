using System.Text.Json.Serialization;

namespace ReboostTV.Core.Models;

[JsonConverter(typeof(JsonStringEnumConverter<DnsMode>))]
public enum DnsMode
{
    None,
    Adblock,
    Family
}

[JsonConverter(typeof(JsonStringEnumConverter<LauncherChoice>))]
public enum LauncherChoice
{
    KeepCurrent,
    Projectivy,
    FLauncher
}

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed class ReboostConfig
{
    // Identifikátor verze schématu (pro budoucí kompatibilitu)
    [JsonPropertyName("schemaVersion")]
    public int SchemaVersion { get; init; } = 1;

    // 1-Click Boost přepínače (čisté booly, žádné cesty ani příkazy)
    [JsonPropertyName("debloatBloatware")]
    public bool DebloatBloatware { get; init; } = true;

    [JsonPropertyName("deepCacheClean")]
    public bool DeepCacheClean { get; init; } = true;

    [JsonPropertyName("fastAnimations")]
    public bool FastAnimations { get; init; } = true;

    [JsonPropertyName("silentMenu")]
    public bool SilentMenu { get; init; } = false;

    [JsonPropertyName("disableSleep")]
    public bool DisableSleep { get; init; } = false;

    // Pevné režimy z výčtu (enum)
    [JsonPropertyName("dnsMode")]
    public DnsMode DnsMode { get; init; } = DnsMode.Adblock;

    [JsonPropertyName("launcher")]
    public LauncherChoice Launcher { get; init; } = LauncherChoice.KeepCurrent;

    // Volitelné ověřené aplikace (cesty/balíčky jsou natvrdo v C# kódu, ne v JSONu)
    [JsonPropertyName("installAuroraStore")]
    public bool InstallAuroraStore { get; init; } = false;

    [JsonPropertyName("installTurboHud")]
    public bool InstallTurboHud { get; init; } = true;

    [JsonPropertyName("enableHudAutoHide")]
    public bool EnableHudAutoHide { get; init; } = true;

    /// <summary>
    /// Bezpečné výchozí nastavení (fallback), pokud je JSON poškozený nebo podvržený.
    /// Všechny zásahy do systému jsou vypnuté.
    /// </summary>
    public static ReboostConfig SafeDefault() => new()
    {
        SchemaVersion = 1,
        DebloatBloatware = false,
        DeepCacheClean = false,
        FastAnimations = false,
        SilentMenu = false,
        DisableSleep = false,
        DnsMode = DnsMode.None,
        Launcher = LauncherChoice.KeepCurrent,
        InstallAuroraStore = false,
        InstallTurboHud = false,
        EnableHudAutoHide = true
    };
}