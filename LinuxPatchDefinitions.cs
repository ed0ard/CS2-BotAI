namespace BotAI;

internal static class LinuxPatchDefinitions
{
    internal static IReadOnlyDictionary<string, (string signature, string patch, string expectedOriginal, int patchOffset)> All { get; } =
        new Dictionary<string, (string signature, string patch, string expectedOriginal, int patchOffset)>()
        {
        // Confirmed against CS2 1.41.8.5 libserver.so (ServerVersion 2000918, built 2026-09-21): every signature
        // matches exactly once, inside the function its comment names, with expectedOriginal at the site.
        // NOP the BombState reset in CSGameState::Reset() (linux-specific bytes).
        ["GameState_Reset"] = (
            signature:        "0F 2E 43 18 C7 43 0C 00 00 00 00",
            patch:            "0F 1F 80 00 00 00 00",
            expectedOriginal: "C7 43 0C 00 00 00 00",
            patchOffset:      4
        ),

        // IdleState::OnUpdate: treat IsSafe() as false so bots don't idle near safe areas.
        ["Idle_IsSafeAlwaysFalse"] = (
            signature:        "E8 ? ? ? ? 84 C0 0F 85 ? ? ? ? 4C 8D B3 ? ? 00 00 4C 89 F7 E8",
            patch:            "90 90 90 90 90 90",
            expectedOriginal: "0F 85 ? ? ? ?",
            patchOffset:      7
        ),

        // EscapeFromBombState::OnEnter tail-call to EquipKnife() -> ret.
        ["EscapeFromBomb_OnEnter_NoEquipKnife"] = (
            signature:        "C6 83 ? ? 00 00 00 48 8B 5D F8 C9 E9 ? ? ? ?",
            patch:            "C3 90 90 90 90",
            expectedOriginal: "E9 ? ? ? ?",
            patchOffset:      12
        ),

        // EscapeFromBombState::OnUpdate call to EquipKnife() -> NOP.
        ["EscapeFromBomb_OnUpdate_NoEquipKnife"] = (
            signature:        "48 85 C0 0F 84 ? ? ? ? 48 89 DF 49 89 C4 E8 ? ? ? ? 31 F6 48 89 DF E8 ? ? ? ?",
            patch:            "90 90 90 90 90",
            expectedOriginal: "E8 ? ? ? ?",
            patchOffset:      15
        ),

        // EscapeFromFlamesState::OnEnter call to EquipKnife() -> NOP.
        ["EscapeFromFlames_OnEnter_NoEquipKnife"] = (
            signature:        "C6 83 ? ? 00 00 00 48 89 DF C6 83 ? ? 00 00 00 E8 ? ? ? ? F3 0F 10 1D",
            patch:            "90 90 90 90 90",
            expectedOriginal: "E8 ? ? ? ?",
            patchOffset:      17
        ),

        ["PlantBombLookAtPriorityLow"] = (
            signature:        "48 8D 55 C8 4C 89 E7 45 31 C9 F3 0F 10 40 08 45 31 C0 B9 02 00 00 00 48 89 5D C8 F3 0F 10 0D ? ? ? ? 48 8D 35 ? ? ? ? F3 0F 11 45 D0",
            patch:            "B9 00 00 00 00",
            expectedOriginal: "B9 02 00 00 00",
            patchOffset:      18
        ),

        ["DefuseBombLookAtPriorityLow"] = (
            signature:        "4C 89 E2 45 31 C9 45 31 C0 F3 0F 10 05 ? ? ? ? B9 02 00 00 00 48 89 DF 48 8D 35 ? ? ? ? E8 ? ? ? ?",
            patch:            "B9 00 00 00 00",
            expectedOriginal: "B9 02 00 00 00",
            patchOffset:      17
        ),

        // MoveToState::OnUpdate - DefuseBomb IsVisible gate removal.
        ["DefuseBomb_SkipIsVisibleCheck"] = (
            signature:        "0F 2F C8 0F 86 ? ? ? ? 31 C9 31 D2 4C 89 E6 48 89 DF E8 ? ? ? ? 84 C0 0F 84 ? ? ? ? 48 83 C4 68 48 89 DF",
            patch:            "90 90 90 90 90 90",
            expectedOriginal: "0F 84 ? ? ? ?",
            patchOffset:      26
        ),

        // AttackState::OnUpdate: keep the fire shortcut from returning early.
        ["AttackState_SkipSteadyFireShortcut"] = (
            signature:        "BA 01 00 00 00 48 89 DF 48 89 C6 E8 ? ? ? ? 84 C0 0F 84 ? ? ? ? 48 89 DF E8 ? ? ? ?",
            patch:            "90 90 90 90 90 90",
            expectedOriginal: "0F 84 ? ? ? ?",
            patchOffset:      18
        ),

        // AttackState::OnUpdate: don't leave the zoom/lineup shortcut path early.
        ["AttackState_SkipZoomFireShortcut"] = (
            signature:        "F3 0F 10 05 ? ? ? ? 48 89 DF E8 ? ? ? ? 84 C0 0F 84 ? ? ? ? 83 BB C8 05 00 00 14",
            patch:            "90 90 90 90 90 90",
            expectedOriginal: "0F 84 ? ? ? ?",
            patchOffset:      18
        ),

        // AttackState::OnUpdate: on Linux this clears holdPosition on the final retreat-and-hide CCSBot::Hide()
        // tail call; the Windows entry targets the continuous-fire flag, which has no Linux counterpart here.
        ["SprayAllDistances_ForceHoldTrigger"] = (
            signature:        "48 89 DF BA 01 00 00 00 F3 0F 10 85 ? ? ? ? 48 83 C4 ? 5B 41 5C 41 5D 41 5E 41 5F 5D E9",
            patch:            "31 D2 90 90 90",
            expectedOriginal: "BA 01 00 00 00",
            patchOffset:      3
        ),

        // AttackState::OnEnter: always take the high-skill dodge chance path.
        ["AttackState_DodgeChance100_Always"] = (
            signature:        "48 89 DF F3 0F 11 85 48 FE FF FF E8 ? ? ? ? 84 C0 0F 84 ? ? ? ? 44 8B 2D ? ? ? ? 45 89 EE",
            patch:            "90 90 90 90 90 90",
            expectedOriginal: "0F 84 ? ? ? ?",
            patchOffset:      18
        ),

        // AttackState::OnUpdate: skip the CanSeeSniper retreat block.
        // Since CS2 1.41.8.5 the compiler emits a jne rel32 to an out-of-line block instead of je rel8, so the jne
        // is NOPed; the effect matches the old je -> jmp rewrite.
        ["AttackState_RetreatOnSniper_Disable"] = (
            signature:        "48 8B 07 48 8D 15 ? ? ? ? 48 8B 80 ? ? 00 00 48 39 D0 0F 85 ? ? ? ? 80 BF ? ? 00 00 00 0F 85 ? ? ? ? 4C 8D ? ? ? ? ? 49 8B",
            patch:            "90 90 90 90 90 90",
            expectedOriginal: "0F 85 ? ? ? ?",
            patchOffset:      33
        ),

        // AttackState::OnUpdate: don't leave the nearby-fire threat path because spread is zero.
        ["AttackState_SkipSniperSpreadCheck"] = (
            signature:        "83 BB C8 05 00 00 14 48 89 DF 0F 84 ? ? ? ? E8 ? ? ? ? F3 0F 10 8B ? ? 00 00 66 0F EF C0 0F 2F C8 0F 86 ? ? ? ? F3 0F 10 0D",
            patch:            "90 90 90 90 90 90",
            expectedOriginal: "0F 86 ? ? ? ?",
            patchOffset:      36
        ),

        // Keep bot movement behavior when seeing enemies.
        ["AllSkill_KeepMoving_WhenSeeSniper"] = (
            signature:        "0F 2F 05 ? ? ? ? 76 0D 80 BB C4 05 00 00 00 0F 85",
            patch:            "90 90",
            expectedOriginal: "76 0D",
            patchOffset:      7
        ),

        ["AttackState_CanStrafe_jne"] = (
            signature:        "BE 01 00 00 00 48 89 DF E8 ? ? ? ? 84 C0 74 ? 80 BB ? ? 00 00 00 0F 84",
            patch:            "90 90",
            expectedOriginal: "74 ?",
            patchOffset:      15
        ),

        // AttackState::OnEnter: force the reload-dodge chance flag true.
        ["AttackState_DodgeDuringReload"] = (
            signature:        "F3 0F 59 40 08 0F 2F C8 41 0F 97 44 24 44 48 81 C4 A8 01 00 00",
            patch:            "41 C6 44 24 44 01",
            expectedOriginal: "41 0F 97 44 24 44",
            patchOffset:      8
        ),

        // AttackState::OnEnter: force the crouch-dodge chance flag true.
        ["SniperCrouchDodge_jb"] = (
            signature:        "0F 2F F8 66 0F EF C0 41 0F 93 44 24 42 E8 ? ? ? ? 48 8B 43 08",
            patch:            "41 C6 44 24 42 01",
            expectedOriginal: "41 0F 93 44 24 42",
            patchOffset:      7
        ),

        // AttackState::OnEnter: don't require the current weapon to be a sniper for dodge A.
        ["SniperDodge_SkipIsSniper_DodgeA"] = (
            signature:        "48 89 DF E8 ? ? ? ? 84 C0 0F 84 ? ? ? ? 44 8B 35 ? ? ? ? F3 0F 10 0D",
            patch:            "90 90 90 90 90 90",
            expectedOriginal: "0F 84 ? ? ? ?",
            patchOffset:      10
        ),

        // CCSBot::UpdateLookAround: ignore the movement timer gate.
        ["Vision_SkipIsMovingGate"] = (
            signature:        "F3 0F 10 83 00 06 00 00 66 0F EF C9 0F 2F C1 7A 08 74 06 0F 87 ? ? ? ?",
            patch:            "90 90 90 90 90 90",
            expectedOriginal: "0F 87 ? ? ? ?",
            patchOffset:      19
        ),

        // 1.8.9: all `00 00 00 00` rel32 fields in the cave pairs below are COMPUTED at
        // load time from the resolved addresses of both pair members (see
        // LinuxDisplacementFixups). The cave<->site distance spans function boundaries
        // and changes on every game update — hardcoding it is what used to make a
        // sig-matched pair jump into the wrong bytes after build drift.
        ["Vision_AlwaysEnterApproachBody_Cave"] = (
            signature:        "48 89 DF FF D0 E9 ? ? ? ? CC CC CC CC CC CC CC CC CC CC CC CC CC CC CC CC CC CC",
            patch:            "48 83 7B 18 00 0F 84 00 00 00 00 E9 00 00 00 00",
            expectedOriginal: "CC CC CC CC CC CC CC CC CC CC CC CC CC CC CC CC",
            patchOffset:      10
        ),

        // Always take approach-body path in Vision logic.
        ["Vision_AlwaysEnterApproachBody"] = (
            signature:        "80 BB 39 04 00 00 00 0F 85 ? ? ? ? E9 ? ? ? ? 66 0F 1F 44 00 00 48 89 DF",
            patch:            "E9 00 00 00 00 90",
            expectedOriginal: "0F 85 ? ? ? ?",
            patchOffset:      7
        ),

        ["Vision_AlwaysWatchApproachPoints_Cave"] = (
            signature:        "81 E2 FF FF FF 3F 89 55 AC E9 ? ? ? ? CC CC CC CC CC CC CC CC CC CC CC CC CC CC CC CC",
            patch:            "48 83 7B 18 00 0F 84 00 00 00 00 E9 00 00 00 00",
            expectedOriginal: "CC CC CC CC CC CC CC CC CC CC CC CC CC CC CC CC",
            patchOffset:      14
        ),

        // CCSBot::UpdateLookAround: skip the skill threshold before approach-body checks.
        ["Vision_AlwaysWatchApproachPoints"] = (
            signature:        "F3 0F 58 85 ? ? ? ? 80 BB ? ? 00 00 00 F3 0F 11 83 ? ? 00 00 0F 84 ? ? ? ? F3 0F 10 1D",
            patch:            "E9 00 00 00 00 90",
            expectedOriginal: "0F 84 ? ? ? ?",
            patchOffset:      23
        ),

        // Signature pins all 32 padding bytes the patch writes (was 30 — the last two
        // patch bytes were only covered by expectedOriginal, not the signature).
        ["Vision_AlwaysWatchApproachPoints_LoopEntry_Cave"] = (
            signature:        "48 8B 07 FF 50 20 E9 ? ? ? ? CC CC CC CC CC CC CC CC CC CC CC CC CC CC CC CC CC CC CC CC CC CC CC CC CC CC CC CC CC CC CC CC",
            patch:            "49 8B 7F 10 48 85 FF 0F 84 00 00 00 00 80 BA 24 06 00 00 02 75 05 BE 03 00 00 00 E9 00 00 00 00",
            expectedOriginal: "CC CC CC CC CC CC CC CC CC CC CC CC CC CC CC CC CC CC CC CC CC CC CC CC CC CC CC CC CC CC CC CC",
            patchOffset:      11
        ),

        ["Vision_AlwaysWatchApproachPoints_LoopEntry"] = (
            signature:        "49 8B 56 18 BE 02 00 00 00 49 8B 7F 10 80 BA 24 06 00 00 02",
            patch:            "E9 00 00 00 00 90 90 90 90 90 90",
            expectedOriginal: "49 8B 7F 10 80 BA 24 06 00 00 02",
            patchOffset:      9
        ),

        // CCSBot::UpdateLookAround: don't leave the approach-body path when the hiding spot cone check fails.
        ["Vision_ApproachBody_SkipHidingSpotCheck"] = (
            signature:        "48 89 DF E8 ? ? ? ? 85 C0 0F 84 ? ? ? ? 48 8B 03 48 8D 15 ? ? ? ? 48 8B 80 ? ? 00 00",
            patch:            "90 90 90 90 90 90",
            expectedOriginal: "0F 84 ? ? ? ?",
            patchOffset:      10
        ),

        // CCSBot::InViewCone: do not reject targets outside the 60-degree outer FOV.
        ["InViewCone_RemoveOuterFOV"] = (
            signature:        "48 8B 47 18 48 8B 98 ? ? 00 00 48 8B 03 48 89 DF FF 90 E8 00 00 00 31 C0 0F 2F 05 ? ? ? ? 77 20",
            patch:            "90 90",
            expectedOriginal: "77 20",
            patchOffset:      32
        ),

        // CCSBot::InViewCone: treat all accepted targets as inside the inner cone.
        ["InViewCone_RemoveInnerFOV"] = (
            signature:        "FF 90 E8 00 00 00 B8 01 00 00 00 BA 02 00 00 00 0F 2F 05 ? ? ? ? 0F 46 C2 48 8B 5D F8 C9 C3",
            patch:            "89 D0 90",
            expectedOriginal: "0F 46 C2",
            patchOffset:      23
        ),

        // CCSBot::OnAudibleEvent: accept sounds regardless of distance.
        ["OnAudibleEvent_GlobalHearRange"] = (
            signature:        "F3 0F 51 ? 0F 2F ? 0F 86 ? ? ? ? 4C 89 EF",
            patch:            "90 90 90 90 90 90",
            expectedOriginal: "0F 86 ? ? ? ?",
            patchOffset:      7
        ),

        // Idle/bomb-search fallback: GetNextBombsiteToSearch() -> GetPlantedBombsite().
        // Deliberately left hardcoded on BOTH sides (values from CS2-BotAI's 14172
        // revalidation): the call retargets one game function to a sibling function,
        // so there is nothing at the patch site to compute the new displacement from.
        // Hardcoding expectedOriginal keeps it fail-safe — on build drift validation
        // stops matching and the patch is skipped cleanly rather than writing a stale
        // call target. A future drift-proof version would resolve GetPlantedBombsite
        // via its own signature and compute the displacement like the cave pairs.
        ["TBot_BombsiteSearch_UseKnownPlantedSite"] = (
            signature:        "48 8B BB ? ? 00 00 E8 ? ? ? ? 4C 89 F7 E8 ? ? ? ? 49 8B 3C 24 31 F6",
            patch:            "E8 BC B1 F6 FF",
            expectedOriginal: "E8 AC B4 F6 FF",
            patchOffset:      15
        ),

        // OnBombPickedUp: force the pathfind/hear gate to enter the tracking path.
        // 1.8.9: opcode-only rewrite (jne -> jmp); rel8 displacement left untouched.
        ["BombPickup_CT_GlobalHearRange"] = (
            signature:        "E8 ? ? ? ? 31 C9 BA 02 00 00 00 48 89 DF F3 0F 10 05 ? ? ? ? 48 89 C6 E8 ? ? ? ? 84 C0 75 84",
            patch:            "EB",
            expectedOriginal: "75",
            patchOffset:      33
        ),

        // OnBombBeep: ignore the 1500-unit hear range and update the bombsite from any distance.
        ["BombBeep_CT_GlobalHearRange"] = (
            signature:        "F3 0F 58 C2 F3 0F 58 C1 F3 0F 10 0D ? ? ? ? 0F 2F C8 0F 86 ? ? ? ? 48 8B 43 18",
            patch:            "90 90 90 90 90 90",
            expectedOriginal: "0F 86 ? ? ? ?",
            patchOffset:      19
        ),

        // CSGameState::OnBombPlanted: all bot-owned game states learn the planted site.
        // 1.8.9: rel32 computed at load as origRel32 + 1 (jz rel32 is 6 bytes, jmp rel32
        // is 5 — same target, instruction shrinks by one). expectedOriginal wildcards the
        // displacement so validation survives game updates.
        ["OnBombPlanted_AllBotsLearnSite"] = (
            signature:        "48 8B 83 ? ? 00 00 48 8B 40 18 80 B8 ? ? 00 00 02 0F 84 ? ? ? ? 48 8B 7B 18",
            patch:            "E9 00 00 00 00 90",
            expectedOriginal: "0F 84 ? ? ? ?",
            patchOffset:      18
        ),

        // CT defuse task path: SetDisposition(SELF_DEFENSE) -> ENGAGE_AND_INVESTIGATE.
        ["CT_Defuse_EngageAndInvestigate"] = (
            signature:        "48 8B 05 ? ? ? ? BE 02 00 00 00 48 89 DF 48 89 83 C8 05 00 00 E8 ? ? ? ? BA 02 00 00 00 4C 89 EE E9 ? ? ? ?",
            patch:            "BE 00 00 00 00",
            expectedOriginal: "BE 02 00 00 00",
            patchOffset:      7
        ),

        // DefuseBombState::OnUpdate: SetDisposition(SELF_DEFENSE) -> ENGAGE_AND_INVESTIGATE.
        ["DefuseBombState_OnUpdate_EngageAndInvestigate"] = (
            signature:        "55 48 8D BE ? ? 00 00 48 89 E5 41 54 53 48 89 F3 E8 ? ? ? ? BE 02 00 00 00 48 89 DF 49 89 C4 E8 ? ? ? ?",
            patch:            "BE 00 00 00 00",
            expectedOriginal: "BE 02 00 00 00",
            patchOffset:      22
        ),

        // DefuseBombState::OnEnter: SetDisposition(SELF_DEFENSE) -> ENGAGE_AND_INVESTIGATE.
        ["DefuseBombState_OnEnter_EngageAndInvestigate"] = (
            signature:        "55 48 89 E5 41 54 53 48 89 F3 BE 02 00 00 00 48 89 DF E8 ? ? ? ? 4C 8B A3 ? ? 00 00",
            patch:            "BE 00 00 00 00",
            expectedOriginal: "BE 02 00 00 00",
            patchOffset:      10
        ),

        // Disable flashbang avoidance SetLookAt/StopAiming block.
        ["FlashbangAvoidance_Disable"] = (
            signature:        "0F 5C C2 48 8D 35 ? ? ? ? F3 0F 11 4D ? F3 0F 10 0D ? ? ? ? 0F 13 45 ? F3 0F 10 05 ? ? ? ? E8 ? ? ? ? C6 83 ? ? 00 00 00",
            patch:            "90 90 90 90 90 90 90 90 90 90 90 90",
            expectedOriginal: "E8 ? ? ? ? C6 83 ? ? 00 00 00",
            patchOffset:      35
        )
    };
}
