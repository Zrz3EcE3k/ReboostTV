using ReboostTV.Core.Models;

namespace ReboostTV.Core.Catalog;

public static class KnownTargets
{
    /// <summary>
    /// Bezpečný seznam balastu na Android TV / Google TV / Xiaomi / Fire TV.
    /// Nikdy nevypíná jádro systému, Google Play Services ani síť.
    /// </summary>
    public static readonly IReadOnlyList<string> SafeBloatwarePackages =
    [
        "com.google.android.videos",             // Google TV / Play Movies
        "com.google.android.music",              // Staré Play Music
        "com.google.android.youtube.tvmusic",    // YouTube Music
        "com.google.android.play.games",         // Play Games
        "com.google.android.feedback",           // Odesílání zpětné vazby
        "com.google.android.tvrecommendations",  // Reklamní doporučení na ploše
        "com.mitv.tvhome.atv",                   // Xiaomi reklamy/tipy
        "com.xiaomi.mitv.shop",                  // Xiaomi Shop
        "com.amazon.avod",                       // Prime Video (pokud uživatel nechce)
        "com.netflix.partner.activation"         // Partner activation balast
    ];

    /// <summary>
    /// Pevně dané Private DNS servery (Android 9+ DNS-over-TLS).
    /// </summary>
    public static string? GetPrivateDnsHostname(DnsMode mode) => mode switch
    {
        DnsMode.Adblock => "dns.adguard-dns.com",
        DnsMode.Family => "family.adguard-dns.com",
        _ => null
    };

    /// <summary>
    /// Názvy lokálních ověřených APK souborů ve složce vedle ReboostTV.exe.
    /// </summary>
    public static class LocalApkNames
    {
        public const string TurboHud = "ReboostTV-Turbo.apk";
        public const string Projectivy = "ProjectivyLauncher.apk";
        public const string FLauncher = "FLauncher.apk";
        public const string AuroraStore = "AuroraStore.apk";
    }

    public const string TurboPackageName = "com.reboosttv.turbo";
}