using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes;
using CounterStrikeSharp.API.Modules.Memory;
using CounterStrikeSharp.API.Modules.Utils;
using Common;
using Microsoft.Extensions.Logging;
using System.Runtime.InteropServices;

namespace BotAI;

internal sealed record PatchInfo(string Name, MemoryPatch Site, byte[] OriginalBytes);

[MinimumApiVersion(304)]
public class BotAI : BasePlugin
{
    public override string ModuleName => "Bot AI - Patches";
    public override string ModuleVersion => "1.8.15";
    public override string ModuleAuthor => "K4ryuu & Austin (updated by ed0ard & XBribo & unicbm & Misaka17032 & AmagiReina)";
    public override string ModuleDescription =>
        "Improve and fix bots' behavior comprehensively";

    private readonly List<PatchInfo> _appliedPatches = [];

    public override void Load(bool hotReload)
    {
        Logger.LogInformation("Bot AI Patches loading...");
        var gamedata = Gamedata.Load(Path.Combine(ModuleDirectory, "gamedata.json"), OperatingSystem.IsLinux() ? "linux" : "windows");
        var (offsets, patches) = gamedata;
        string serverPath = GameUtils.GetModulePath("server");
        ApplyPatches(patches, signature => NativeAPI.FindSignature(serverPath, signature));

        RegisterEventHandler<EventPlayerSpawn>((@event, info) =>
        {
            var player = @event.Userid;
            if (player?.IsValid != true || !player.IsBot) return HookResult.Continue;

            var pawn = player.PlayerPawn.Value;
            if (pawn?.IsValid != true
                || player.Team <= CsTeam.Spectator
                || !pawn.BotAllowActive)
                return HookResult.Continue;

            var gameRules = Utilities.GetGameRules();
            if (gameRules == null || gameRules.BombPlanted) return HookResult.Continue;

            UpdateBotBombState(pawn, player.PlayerName, offsets);
            return HookResult.Continue;
        });

        Logger.LogInformation($"Applied {_appliedPatches.Count}/{patches.Count} patches.");
    }

    public override void Unload(bool hotReload)
    {
        Logger.LogInformation("Bot AI Patches unloading...");
        RestoreAppliedPatches();
    }

    internal void ApplyPatches(IReadOnlyList<PatchDefinition> definitions, Func<string, nint> resolveSignature)
    {
        // Patches can overwrite bytes used by other signatures.
        var sites = new List<(PatchDefinition Definition, nint Address)>();
        foreach (var definition in definitions)
        {
            nint address = resolveSignature(definition.Signature);
            if (address == 0)
            {
                Logger.LogError("'{Patch}': signature not found.", definition.Name);
                continue;
            }
            sites.Add((definition, checked(address + definition.Offset)));
        }

        var byAddress = sites.OrderBy(site => site.Address).ToArray();
        for (int i = 1; i < byAddress.Length; i++)
        {
            var previous = byAddress[i - 1];
            var current = byAddress[i];
            if (current.Address < checked(previous.Address + previous.Definition.Patch.Length))
                throw new InvalidDataException($"Patches '{previous.Definition.Name}' and '{current.Definition.Name}' overlap at 0x{current.Address:X}; no patches written.");
        }

        try
        {
            foreach (var (definition, address) in sites)
            {
                try { WritePatch(definition, address); }
                catch (Exception ex)
                {
                    throw new InvalidOperationException($"Could not apply '{definition.Name}' at 0x{address:X}.", ex);
                }
            }
        }
        catch
        {
            RestoreAppliedPatches();
            throw;
        }
    }

    private void WritePatch(PatchDefinition definition, nint addr)
    {
        var (name, _, _, expectedOriginal, patchBytes) = definition;
        var site = MemoryPatch.Capture(addr, patchBytes.Length);
        var origBytes = new byte[patchBytes.Length];
        Marshal.Copy(addr, origBytes, 0, origBytes.Length);
        if (!MatchesOriginal(origBytes, expectedOriginal))
        {
            Logger.LogError("'{Patch}': byte mismatch. Expected [{Expected}] got [{Actual}].", name,
                string.Join(" ", expectedOriginal.Select(b => b?.ToString("X2") ?? "?")),
                Convert.ToHexString(origBytes));
            return;
        }

        // A failed write can leave modified bytes.
        _appliedPatches.Add(new PatchInfo(name, site, origBytes));
        site.Write(patchBytes);
        Logger.LogInformation("'{Patch}' patched at 0x{Address:X} ({Length} bytes).", name, addr, patchBytes.Length);
    }

    private void RestoreAppliedPatches()
    {
        int total = _appliedPatches.Count;
        for (int i = total - 1; i >= 0; i--)
        {
            var patch = _appliedPatches[i];
            try
            {
                patch.Site.Write(patch.OriginalBytes);
                _appliedPatches.RemoveAt(i);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Could not fully restore '{Patch}'.", patch.Name);
            }
        }
        if (_appliedPatches.Count == 0)
            Logger.LogInformation("Restored {Count} patches.", total);
        else
            Logger.LogCritical("Restored {Restored}/{Total} patches. Restart the server before reloading BotAI.",
                total - _appliedPatches.Count, total);
    }

    private static bool MatchesOriginal(ReadOnlySpan<byte> actual, ReadOnlySpan<byte?> expected)
    {
        for (int i = 0; i < expected.Length; i++)
            if (expected[i] is byte value && actual[i] != value) return false;
        return true;
    }

    private void UpdateBotBombState(CCSPlayerPawn pawn, string playerName, BotOffsets offsets)
    {
        try
        {
            if (pawn.Bot?.Handle is not { } handle || handle == nint.Zero) return;

            nint gsPtr = handle + offsets.GameState;
            if (Marshal.ReadByte(gsPtr + offsets.IsRoundOver) != 0) return;

            // The bomb state is writable bot data.
            nint bombAddr = gsPtr + offsets.BombState;
            if (Marshal.ReadInt32(bombAddr) != 0) Marshal.WriteInt32(bombAddr, 0);
        }
        catch (Exception ex)
        {
            Logger.LogError($"UpdateBotBombState({playerName}): {ex.Message}");
        }
    }
}
