# Spearbreaker

Valheim mod (BepInEx + Jotunn) adding one trinket, the **Spearbreaker charm**. While worn, its
passive `SE_SpearbreakerPassive` makes spear hits (melee and thrown) treat a creature as one step
weaker to pierce; at full adrenaline it fires `SE_Spearbreaker`, which adds extra pierce damage for a while. Vanilla spears and bows share the pierce type; this gives
spears their own answer without adding a damage type. Player-facing description lives in
`package/README.md`; the root `README.md` is the GitHub page for developers; release history in
`CHANGELOG.md`.

Sibling projects `../heraldry` and `../warden` use the same stack, build and release setup; their
`.claude/rules/` hold more Valheim lessons.

## Stack

- C# targeting `net462`, `LangVersion` latest, nullable reference types disabled.
- `JotunnLib` 2.30.2 from NuGet. Jotunn's prebuild generates publicized Valheim assemblies in
  `Valheim_Data/Managed/publicized_assemblies`; Valheim is located through the Steam registry entry
  (override with `-p:VALHEIM_INSTALL="<path>"`).
- Harmony (via BepInEx) for patches. No Unity project and no asset bundles: the item is a Jotunn
  clone of the vanilla Heart of the Forest, with its icon loaded from an embedded PNG. Its model is
  recoloured at startup (`SpearbreakerItem.Recolor`) on copies of the materials and a greyscaled copy
  of the texture; the vanilla materials stay untouched. Skipped on a dedicated server (no GPU).
  The item and passive icons are the Heart of the Forest's icon, and the full-adrenaline icon is its
  full-adrenaline effect's icon, each copied out of its atlas cell, greyed and multiplied by the same
  tint (`SpearbreakerItem.Tinted`). Display names: item "Spearbreaker", passive "Gungnir's Aim",
  active "Odin's Claim".

## Layout

| Path | Purpose |
| --- | --- |
| `src/SpearbreakerPlugin.cs` | BepInEx entry point: GUID/version, config, English localization, Harmony `PatchAll` |
| `src/SpearbreakerItem.cs` | Registers the trinket (clone of `TrinketBronzeHealth`), recipe, icon and status effect |
| `src/SE_SpearbreakerPassive.cs` | The equip effect: raises the bypass attribute bit while the charm is worn |
| `src/SE_Spearbreaker.cs` | The full-adrenaline effect: pierce bonus (attacker side) |
| `src/DamagePatches.cs` | Shifts the pierce modifier one step weaker in `Character.GetDamageModifiers` during `RPC_Damage` |
| `assets/icon.png` | Fallback icon, embedded as `Spearbreaker.icon.png`; used only when a vanilla icon is missing |
| `tools/make_icons.ps1` | Generates `assets/icon.png` and `package/icon.png` from the game-icons.net archive |
| `package/README.md`, `package/icon.png` | Mod page and icon shipped in the package |
| `.github/workflows/build.yml` | CI: builds the package on push/PR, GitHub release and Hexium upload on `v*` tags |

## Commands

Run from the repo root (Windows, PowerShell).

```
dotnet build -c Release                           # build bin/Release/Spearbreaker.dll, touches nothing outside the repo
dotnet build -c Release -t:Package                # also writes dist/Spearbreaker-<version>.zip
```

- The build never copies anything into the game. The user tests by importing the packaged zip into
  their mod manager (Gale), which installs its own copy; never copy the DLL into a game or Gale
  profile by hand.
- A clean build prints one known warning, `MSB3245` for `UnityEngine.ProfilerModule`. Any other is new.

## Verification

There is no test project. "Done" means: `dotnet build -c Release` succeeds with no new warnings, and
behaviour that needs the game (crafting, adrenaline, damage numbers, multiplayer) is called out as
untested-in-game unless the user has confirmed it in Valheim. Never claim in-game behaviour works on
the strength of a successful build.

## How the effect works

- **Pierce bonus** (active, full adrenaline) is applied by `SE_Spearbreaker.ModifyAttack`, which vanilla calls on the
  attacker's side for every attack, including thrown spears (the projectile carries the hit).
  Only hits whose `HitData.m_skill` is `Spears` count; atgeirs use `Polearms` and are excluded on
  purpose.
- **Resistance shift** (passive, the charm's equip effect `m_equipStatusEffect`) runs where the
  creature is owned (`Character.RPC_Damage`), often another client. That client can't see the
  attacker's status effects; vanilla only syncs the `StatusEffect.StatusAttribute` flags through the
  player's ZDO (`ZDOVars.s_seAttrib`). `SE_SpearbreakerPassive` sets an unused bit (`1 << 20`) and
  `DamagePatches` reads it with `SEMan.HaveStatusAttribute`.
- Resistant, VeryResistant and SlightlyResistant pierce become Normal; Normal and SlightlyWeak become
  Weak; Weak becomes VeryWeak. Immune, Ignore and VeryWeak are untouched. Player targets (PvP) keep
  vanilla modifiers.

## Invariants that must not break

- **Saved-world compatibility.** Persisted or networked identifiers: the plugin GUID
  `elridion.spearbreaker`, the item prefab `TrinketSpearbreaker` (saved in inventories and chests),
  and the status effect names `SE_Spearbreaker` and `SE_SpearbreakerPassive` (synced by name hash). Renaming one breaks saves or
  multiplayer.
- **The attribute bit** `1 << 20` is part of the network protocol between peers; changing it needs a
  minor version bump. If another mod is found using it, move it and bump.
- **Vanilla assets are shared.** Never modify the Heart of the Forest prefab or its SharedData; the clone
  has its own. Config changes write only the clone's SharedData, which every crafted charm shares.
- **Every client and the server need the mod** (`NetworkCompatibility(EveryoneMustHaveMod, VersionStrictness.Minor)`):
  the bypass runs on whichever peer owns the creature.
- **Config is admin-only and synced** (Jotunn `IsAdminOnly`); read values at use (`SE_Spearbreaker.Setup`,
  `SettingChanged`), never cache them at startup only.
- **Docs follow code.** Recipe, defaults and effect description in `package/README.md` stay in sync
  with `SpearbreakerItem` and `SpearbreakerPlugin` in the same change.

## Art and licensing

The icon is game-icons.net "shield-impact" by Delapouite under CC BY 3.0, generated by
`tools/make_icons.ps1` (the archive is downloaded to `assets/source/`, which is gitignored). Keep the
credit in `package/README.md` and `README.md`.

## Commits

Plain imperative, sentence-case subjects, no type prefix, no trailing period; a wrapped body that
explains why. User-visible changes update `CHANGELOG.md` (and `package/README.md` where relevant) in
the same commit. Do not commit `bin/`, `obj/` or `dist/`.

## Detailed rules

Path-scoped rules in `.claude/rules/` load when matching files are read or edited:

- `code-style.md` - C# conventions for `src/`
- `release.md` - versioning, packaging, README and CHANGELOG
