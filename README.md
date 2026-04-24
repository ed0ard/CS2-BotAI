Couldn't file an issue so forking to alert you to this issue.

Running v1.6.4 on a CS2 Linux dedicated server. All 32 patches fail to apply — 31 with "signature not found" and 1 with a byte mismatch.

## Environment

- OS: Ubuntu 22.04 (VPS)
- CS2 build: 22627914 (Apr 2, 2026 — Animgraph 2 update)
- CS2 ClientVersion: 2000777
- CS2 PatchVersion: 1.41.4.1
- CounterStrikeSharp: v364 (1.0.364) — matches your README
- Metamod: 2.0.0-dev+1366
- Plugin loaded successfully, patches execute during load

## Boot log (BotAI section)
07:05:34 [INFO] (cssharp:PluginContext) Loading plugin BotAI
07:05:34 [INFO] (plugin:Patches - Bot AI) Bot AI Patches loading...
07:05:34 [EROR] (plugin:Patches - Bot AI) 'HasVisitedEnemySpawn': signature not found.
07:05:34 [EROR] (plugin:Patches - Bot AI) HasVisitedEnemySpawn: FAILED.
07:05:34 [EROR] (plugin:Patches - Bot AI) 'GameState_Reset': signature not found.
07:05:34 [EROR] (plugin:Patches - Bot AI) GameState_Reset: FAILED.
07:05:34 [EROR] (plugin:Patches - Bot AI) 'Idle_IsSafeAlwaysFalse': signature not found.
07:05:34 [EROR] (plugin:Patches - Bot AI) Idle_IsSafeAlwaysFalse: FAILED.
07:05:34 [EROR] (plugin:Patches - Bot AI) 'EscapeFromBomb_OnEnter_NoEquipKnife': signature not found.
07:05:34 [EROR] (plugin:Patches - Bot AI) EscapeFromBomb_OnEnter_NoEquipKnife: FAILED.
07:05:34 [EROR] (plugin:Patches - Bot AI) 'EscapeFromBomb_OnUpdate_NoEquipKnife': signature not found.
07:05:34 [EROR] (plugin:Patches - Bot AI) EscapeFromBomb_OnUpdate_NoEquipKnife: FAILED.
07:05:34 [EROR] (plugin:Patches - Bot AI) 'EscapeFromFlames_OnEnter_NoEquipKnife': signature not found.
07:05:34 [EROR] (plugin:Patches - Bot AI) EscapeFromFlames_OnEnter_NoEquipKnife: FAILED.
07:05:34 [EROR] (plugin:Patches - Bot AI) 'InvestigateNoise_SkipSelfDefenseCheck': signature not found.
07:05:34 [EROR] (plugin:Patches - Bot AI) InvestigateNoise_SkipSelfDefenseCheck: FAILED.
07:05:34 [EROR] (plugin:Patches - Bot AI) 'InvestigateNoise_SkipRecentEnemyCheck': signature not found.
07:05:34 [EROR] (plugin:Patches - Bot AI) InvestigateNoise_SkipRecentEnemyCheck: FAILED.
07:05:34 [EROR] (plugin:Patches - Bot AI) 'PlantBombLookAtPriorityLow': signature not found.
07:05:34 [EROR] (plugin:Patches - Bot AI) PlantBombLookAtPriorityLow: FAILED.
07:05:34 [EROR] (plugin:Patches - Bot AI) 'DefuseBombLookAtPriorityLow': signature not found.
07:05:34 [EROR] (plugin:Patches - Bot AI) DefuseBombLookAtPriorityLow: FAILED.
07:05:34 [EROR] (plugin:Patches - Bot AI) 'SprayAllDistances_FireDecision1': signature not found.
07:05:34 [EROR] (plugin:Patches - Bot AI) SprayAllDistances_FireDecision1: FAILED.
07:05:34 [EROR] (plugin:Patches - Bot AI) 'SprayAllDistances_FireDecision2': signature not found.
07:05:34 [EROR] (plugin:Patches - Bot AI) SprayAllDistances_FireDecision2: FAILED.
07:05:34 [EROR] (plugin:Patches - Bot AI) 'AttackState_SkipFireRateCheck': byte mismatch. Expected [0F 82 87 00 00 00] got [0F 82 64 03 00 00].
07:05:34 [EROR] (plugin:Patches - Bot AI) AttackState_SkipFireRateCheck: FAILED.
07:05:35 [EROR] (plugin:Patches - Bot AI) 'AttackState_SkipSteadyFireShortcut': signature not found.
07:05:35 [EROR] (plugin:Patches - Bot AI) AttackState_SkipSteadyFireShortcut: FAILED.
07:05:35 [EROR] (plugin:Patches - Bot AI) 'AttackState_SkipZoomFireShortcut': signature not found.
07:05:35 [EROR] (plugin:Patches - Bot AI) AttackState_SkipZoomFireShortcut: FAILED.
07:05:35 [EROR] (plugin:Patches - Bot AI) 'SprayAllDistances_ja1': signature not found.
07:05:35 [EROR] (plugin:Patches - Bot AI) SprayAllDistances_ja1: FAILED.
07:05:35 [EROR] (plugin:Patches - Bot AI) 'SprayAllDistances_ja2': signature not found.
07:05:35 [EROR] (plugin:Patches - Bot AI) SprayAllDistances_ja2: FAILED.
07:05:35 [EROR] (plugin:Patches - Bot AI) 'SprayAllDistances_ja3': signature not found.
07:05:35 [EROR] (plugin:Patches - Bot AI) SprayAllDistances_ja3: FAILED.
07:05:35 [EROR] (plugin:Patches - Bot AI) 'SprayAllDistances_ja4': signature not found.
07:05:35 [EROR] (plugin:Patches - Bot AI) SprayAllDistances_ja4: FAILED.
07:05:35 [EROR] (plugin:Patches - Bot AI) 'AttackState_DodgeDuringReload': signature not found.
07:05:35 [EROR] (plugin:Patches - Bot AI) AttackState_DodgeDuringReload: FAILED.
07:05:35 [EROR] (plugin:Patches - Bot AI) 'SniperCrouchDodge_jb': signature not found.
07:05:35 [EROR] (plugin:Patches - Bot AI) SniperCrouchDodge_jb: FAILED.
07:05:35 [EROR] (plugin:Patches - Bot AI) 'LowSKill_JumpChance0': signature not found.
07:05:35 [EROR] (plugin:Patches - Bot AI) LowSKill_JumpChance0: FAILED.
07:05:35 [EROR] (plugin:Patches - Bot AI) 'AllSkill_DodgeChance100_OnOutnumberedOrSniper': signature not found.
07:05:35 [EROR] (plugin:Patches - Bot AI) AllSkill_DodgeChance100_OnOutnumberedOrSniper: FAILED.
07:05:35 [EROR] (plugin:Patches - Bot AI) 'DodgeChance_Flat80': signature not found.
07:05:35 [EROR] (plugin:Patches - Bot AI) DodgeChance_Flat80: FAILED.
07:05:35 [EROR] (plugin:Patches - Bot AI) 'AllSkill_KeepMoving_WhenSeeSniper': signature not found.
07:05:35 [EROR] (plugin:Patches - Bot AI) AllSkill_KeepMoving_WhenSeeSniper: FAILED.
07:05:35 [EROR] (plugin:Patches - Bot AI) 'AttackState_CanStrafe_jne': signature not found.
07:05:35 [EROR] (plugin:Patches - Bot AI) AttackState_CanStrafe_jne: FAILED.
07:05:35 [EROR] (plugin:Patches - Bot AI) 'SniperDodge_SkipIsSniper_DodgeA': signature not found.
07:05:35 [EROR] (plugin:Patches - Bot AI) SniperDodge_SkipIsSniper_DodgeA: FAILED.
07:05:35 [EROR] (plugin:Patches - Bot AI) 'Vision_AlwaysWatchApproachPoints': signature not found.
07:05:35 [EROR] (plugin:Patches - Bot AI) Vision_AlwaysWatchApproachPoints: FAILED.
07:05:35 [EROR] (plugin:Patches - Bot AI) 'Vision_ApproachBody_SkipSkillCheck': signature not found.
07:05:35 [EROR] (plugin:Patches - Bot AI) Vision_ApproachBody_SkipSkillCheck: FAILED.
07:05:35 [EROR] (plugin:Patches - Bot AI) 'Vision_ApproachBody_SkipHidingSpotCheck': signature not found.
07:05:35 [EROR] (plugin:Patches - Bot AI) Vision_ApproachBody_SkipHidingSpotCheck: FAILED.
07:05:35 [EROR] (plugin:Patches - Bot AI) 'Vision_SkipIsMovingGate': signature not found.
07:05:35 [EROR] (plugin:Patches - Bot AI) Vision_SkipIsMovingGate: FAILED.
07:05:35 [EROR] (plugin:Patches - Bot AI) 'Vision_AlwaysEnterApproachBody': signature not found.
07:05:35 [EROR] (plugin:Patches - Bot AI) Vision_AlwaysEnterApproachBody: FAILED.
07:05:35 [INFO] (plugin:Patches - Bot AI) Applied 0/32 patches.
07:05:35 [INFO] (cssharp:PluginContext) Finished loading plugin Patches - Bot AI

## What I tested

- Plugin is loaded (confirmed via `css_plugins list`)
- `BotAI.dll` checksum matches your release zip
- CS2 binary is current (steamcmd reports "already up to date", matches buildid 22627914 in appmanifest_730.acf)
- Austin's upstream v1.3 applies 2/3 patches on the same binary — confirming two of the Austin-era signatures still match but most of your newer signatures don't

The byte mismatch on `AttackState_SkipFireRateCheck` is interesting — the signature was found but surrounding bytes differ (expected `[0F 82 87 00 00 00]`, got `[0F 82 64 03 00 00]`). Looks like a jump offset change, suggesting adjacent code shifted.

I'm guessing the signatures were derived from a Windows binary or a pre-Animgraph-2 Linux build. Happy to provide additional diagnostic info, test candidate signatures on my binary, or share the `libserver.so` hash (`bf03bbf4342a26d64fa308e5c3207931`) for reference.

Thanks for maintaining the plugin.

---

# CS2-BotAI
Improves the built in bots AI.<br>
<br>
Keeps them from running with a knife or nade.<br>
Keeps them from switching to their knives when in flames.<br>
Keeps them from switching to their knives when escaping from bomb.<br>
Keeps them from rushing to enemy spawn as their first goal.<br>
Keeps them from stopping after the bomb is planted.<br>
Allows them to flick with sniper rifles and spray at all ranges.<br>
Improves their movement comprehensively, especially in dodging and peeking.<br>
Enhances bots' awareness of their surroundings.<br>
Allows defusing to be interrupted.<br>
<br>
V 1.6.7<br>
Built and testing with<br>
cs# 1.0.367<br>
# Installation
1. Download the latest BotAI.zip from [Releases](https://github.com/ed0ard/CS2-BotAI/releases)

2. Extract the folder and upload it to `game/csgo/addons/counterstrikesharp/plugins` on your server

3. Restart your server
