using System.Globalization;
using System.Text.Json;

namespace BotAI;

internal sealed record PatchDefinition(
    string Name, string Signature, int Offset, byte?[] ExpectedOriginal, byte[] Patch);

internal readonly record struct BotOffsets(int GameState, int IsRoundOver, int BombState);

internal sealed record Gamedata(BotOffsets Offsets, IReadOnlyList<PatchDefinition> Patches)
{
    public static Gamedata Load(string path, string platform)
    {
        using var stream = File.OpenRead(path);
        using var document = JsonDocument.Parse(stream, new JsonDocumentOptions { AllowDuplicateProperties = false });
        var root = document.RootElement;
        if (!root.TryGetProperty("offsets", out var offsetData) || offsetData.ValueKind != JsonValueKind.Object)
            throw new InvalidDataException($"{path}: missing offsets object.");

        var offsets = new BotOffsets(
            ReadOffset("CCSBot::m_gameState"),
            ReadOffset("CSGameState::m_isRoundOver"),
            ReadOffset("CSGameState::m_bombState"));

        var definitions = new List<PatchDefinition>();
        foreach (var entry in root.EnumerateObject())
        {
            if (entry.NameEquals("offsets")) continue;

            string label = $"{path}: {entry.Name}/{platform}";
            try
            {
                definitions.Add(ReadPatch(entry, platform));
            }
            catch (Exception ex) when (ex is KeyNotFoundException or InvalidOperationException or FormatException)
            {
                throw new InvalidDataException($"{label}: {ex.Message}", ex);
            }
        }
        if (definitions.Count == 0)
            throw new InvalidDataException($"{path}: no patch definitions.");
        return new Gamedata(offsets, definitions);

        int ReadOffset(string name)
        {
            if (!offsetData.TryGetProperty(name, out var entry) || entry.ValueKind != JsonValueKind.Object ||
                !entry.TryGetProperty(platform, out var value) || value.ValueKind != JsonValueKind.Number ||
                !value.TryGetInt32(out int offset) || offset < 0)
                throw new InvalidDataException($"{path}: offsets/{name}/{platform} must be a nonnegative integer.");
            return offset;
        }
    }

    private static PatchDefinition ReadPatch(JsonProperty entry, string platform)
    {
        if (entry.Value.GetProperty("library").GetString() != "server")
            throw new FormatException("Patch library must be 'server'.");

        var variant = entry.Value.GetProperty(platform);
        foreach (var property in variant.EnumerateObject())
            if (property.Name is not ("signature" or "offset" or "expectedOriginal" or "patch"))
                throw new FormatException($"Unknown patch field '{property.Name}'.");

        var signature = ParsePattern(variant.GetProperty("signature").GetString(), allowWildcard: true);
        var expected = ParsePattern(variant.GetProperty("expectedOriginal").GetString(), allowWildcard: true);
        var patch = Array.ConvertAll(ParsePattern(variant.GetProperty("patch").GetString(), allowWildcard: false),
            static value => value!.Value);
        int offset = variant.GetProperty("offset").GetInt32();
        if (offset < 0 || offset >= signature.Length)
            throw new FormatException("Patch offset is outside the signature.");
        if (patch.Length != expected.Length)
            throw new FormatException("Patch and expected original must have the same length.");

        return new PatchDefinition(entry.Name, string.Join(" ", signature.Select(b => b?.ToString("X2") ?? "?")),
            offset, expected, patch);
    }

    private static byte?[] ParsePattern(string? pattern, bool allowWildcard)
    {
        var tokens = pattern?.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);
        if (tokens is not { Length: > 0 })
            throw new FormatException("Empty byte pattern.");

        var bytes = new byte?[tokens.Length];
        bool hasFixedByte = false;
        for (int i = 0; i < tokens.Length; i++)
        {
            string token = tokens[i];
            if (allowWildcard && token == "?") continue;
            if (token.Length != 2 || !byte.TryParse(token, NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture, out byte value))
                throw new FormatException($"Invalid byte token '{token}'.");

            bytes[i] = value;
            hasFixedByte = true;
        }
        if (!hasFixedByte)
            throw new FormatException("Byte pattern cannot contain only wildcards.");

        return bytes;
    }
}
