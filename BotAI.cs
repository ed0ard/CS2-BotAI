using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes;
using CounterStrikeSharp.API.Modules.Memory;
using CounterStrikeSharp.API.Modules.Utils;
using Common;
using Microsoft.Extensions.Logging;
using System.Runtime.InteropServices;

namespace BotAI;

internal sealed record PatchInfo(string Name, nint Address, byte[] OriginalBytes);

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

            var gameRules = Utilities
                .FindAllEntitiesByDesignerName<CCSGameRulesProxy>("cs_gamerules")
                .FirstOrDefault()?.GameRules;

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

        foreach (var (definition, address) in sites)
            WritePatch(definition, address);
    }

    private void WritePatch(PatchDefinition definition, nint addr)
    {
        var (name, _, _, expectedOriginal, patchBytes) = definition;
        var origBytes = new byte[patchBytes.Length];
        Marshal.Copy(addr, origBytes, 0, origBytes.Length);
        if (!MatchesOriginal(origBytes, expectedOriginal))
        {
            Logger.LogError("'{Patch}': byte mismatch. Expected [{Expected}] got [{Actual}].", name,
                string.Join(" ", expectedOriginal.Select(b => b?.ToString("X2") ?? "?")),
                Convert.ToHexString(origBytes));
            return;
        }

        if (!MemoryPatch.SetMemAccess(addr, patchBytes.Length))
        {
            Logger.LogError("Could not make '{Patch}' writable.", name);
            return;
        }
        Marshal.Copy(patchBytes, 0, addr, patchBytes.Length);
        _appliedPatches.Add(new PatchInfo(name, addr, origBytes));
        Logger.LogInformation("'{Patch}' patched at 0x{Address:X} ({Length} bytes).", name, addr, patchBytes.Length);
    }

    private void RestoreAppliedPatches()
    {
        foreach (var patch in _appliedPatches)
        {
            try
            {
                if (MemoryPatch.SetMemAccess(patch.Address, patch.OriginalBytes.Length))
                    Marshal.Copy(patch.OriginalBytes, 0, patch.Address, patch.OriginalBytes.Length);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Could not restore '{Patch}'.", patch.Name);
            }
        }
        _appliedPatches.Clear();
    }

    private static bool MatchesOriginal(ReadOnlySpan<byte> actual, ReadOnlySpan<byte?> expected)
    {
        for (int i = 0; i < expected.Length; i++)
            if (expected[i] is byte value && actual[i] != value) return false;
        return true;
    }

    private static bool IsValid(nint addr)
    {
        if (addr == nint.Zero) return false;
        try { Marshal.ReadByte(addr); return true; }
        catch { return false; }
    }

    private bool UpdateBotBombState(CCSPlayerPawn pawn, string playerName, BotOffsets offsets)
    {
        try
        {
            if (pawn?.Bot?.Handle is not { } handle || handle == nint.Zero) return false;
            if (!IsValid(handle)) return false;

            nint gsPtr = handle + offsets.GameState;
            if (!IsValid(gsPtr)) return false;
            if (Marshal.ReadByte(gsPtr + offsets.IsRoundOver) != 0) return true;

            nint bombAddr = gsPtr + offsets.BombState;
            if (!IsValid(bombAddr)) return false;
            if (!MemoryPatch.SetMemAccess(bombAddr, sizeof(int))) return false;
            if (Marshal.ReadInt32(bombAddr) != 0) Marshal.WriteInt32(bombAddr, 0);
            return true;
        }
        catch (Exception ex)
        {
            Logger.LogError($"UpdateBotBombState({playerName}): {ex.Message}");
            return false;
        }
    }
}
