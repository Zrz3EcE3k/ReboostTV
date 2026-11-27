using System.Text.Json;
using System.Text.Json.Serialization;
using ReboostTV.Core.Models;

namespace ReboostTV.Core.Security;

public sealed record ConfigLoadResult(
    bool IsValid,
    ReboostConfig Config,
    string? ErrorMessage = null
);

public static class ConfigValidator
{
    private const long MaxFileSizeBytes = 32 * 1024; // 32 KB limit proti DoS
    private const int SupportedSchemaVersion = 1;

    private static readonly JsonSerializerOptions StrictOptions = new()
    {
        PropertyNameCaseInsensitive = false,
        ReadCommentHandling = JsonCommentHandling.Disallow,
        AllowTrailingCommas = false,
        NumberHandling = JsonNumberHandling.Strict,
        MaxDepth = 4,
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow
    };

    public static ConfigLoadResult LoadFromFile(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
        {
            return new ConfigLoadResult(false, ReboostConfig.SafeDefault(), "Soubor config.json nebyl nalezen.");
        }

        var fileInfo = new FileInfo(filePath);
        if (fileInfo.Length == 0 || fileInfo.Length > MaxFileSizeBytes)
        {
            return new ConfigLoadResult(false, ReboostConfig.SafeDefault(), "Soubor config.json je prázdný nebo překračuje limit 32 KB.");
        }

        try
        {
            string json = File.ReadAllText(filePath);
            return LoadFromJson(json);
        }
        catch (Exception ex)
        {
            return new ConfigLoadResult(false, ReboostConfig.SafeDefault(), $"Chyba při čtení souboru: {ex.Message}");
        }
    }

    public static ConfigLoadResult LoadFromJson(string json)
    {
        if (string.IsNullOrWhiteSpace(json) || json.Length > MaxFileSizeBytes)
        {
            return new ConfigLoadResult(false, ReboostConfig.SafeDefault(), "Neplatná velikost JSON vstupu.");
        }

        try
        {
            var config = JsonSerializer.Deserialize<ReboostConfig>(json, StrictOptions);
            if (config is null)
            {
                return new ConfigLoadResult(false, ReboostConfig.SafeDefault(), "JSON konfigurace je prázdná.");
            }

            if (config.SchemaVersion != SupportedSchemaVersion)
            {
                return new ConfigLoadResult(false, ReboostConfig.SafeDefault(), $"Nepodporovaná verze schématu: {config.SchemaVersion}.");
            }

            if (!Enum.IsDefined(config.DnsMode) || !Enum.IsDefined(config.Launcher))
            {
                return new ConfigLoadResult(false, ReboostConfig.SafeDefault(), "Neplatná hodnota v nastavení DNS nebo Launcheru.");
            }

            return new ConfigLoadResult(true, config);
        }
        catch (JsonException ex)
        {
            return new ConfigLoadResult(false, ReboostConfig.SafeDefault(), $"Konfigurace byla zamítnuta (nepovolené pole nebo neplatný formát): {ex.Message}");
        }
    }
}