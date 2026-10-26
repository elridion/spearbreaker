---
paths:
  - "src/**/*.cs"
---

# Spearbreaker code specifics

Adds to the workspace-wide `code-style.md` and `valheim-modding.md` in `../.claude/rules/`.

## How the effect works

- **Pierce bonus** (active, full adrenaline) is applied by `SE_Spearbreaker.ModifyAttack`, which
  vanilla calls on the attacker's side for every attack, including thrown spears (the projectile
  carries the hit). Only hits whose `HitData.m_skill` is `Spears` count; atgeirs use `Polearms` and
  are excluded on purpose.
- **Resistance shift** (passive, the charm's `m_equipStatusEffect`) runs where the creature is owned
  (`Character.RPC_Damage`), often another client, which can't see the attacker's status effects.
  Vanilla only syncs the `StatusEffect.StatusAttribute` flags through the player's ZDO
  (`ZDOVars.s_seAttrib`), so `SE_SpearbreakerPassive` sets the unused bit `1 << 20` and
  `DamagePatches` reads it with `SEMan.HaveStatusAttribute`, shifting the pierce modifier in
  `Character.GetDamageModifiers` during `RPC_Damage`.
- Resistant, VeryResistant and SlightlyResistant pierce become Normal; Normal and SlightlyWeak become
  Weak; Weak becomes VeryWeak. Immune, Ignore and VeryWeak are untouched. Player targets (PvP) keep
  vanilla modifiers.
- Damage patches run for every hit in the game: bail out on the cheapest check first, never allocate
  or log there.

## Item and visuals

- Config is admin-only and synced; values are read at use (`SE_Spearbreaker.Setup`, `SettingChanged`).
- The model is recoloured at startup (`SpearbreakerItem.Recolor`) on material copies and a greyscaled
  texture copy; skipped on a dedicated server. The item and passive icons are the Heart of the
  Forest's icon, the active icon is its full-adrenaline effect's icon, each copied out of its atlas
  cell, greyed and multiplied by the same tint (`SpearbreakerItem.Tinted`).
- A missing Heart of the Forest skips the charm; a missing icon keeps the Heart of the Forest's.
- Localization keys: `item_trinket_spearbreaker*`, `se_spearbreaker*`.
