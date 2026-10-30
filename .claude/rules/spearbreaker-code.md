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
- **Thrown spear recall** (passive, quality 2 charm only; `ThrowPatches` checks the equipped charm's
  `m_quality` against `SpearbreakerItem.RecallQuality`, since the passive's attribute bit carries no
  quality): a throw unequips the spear and removes it from the inventory
  (`Attack.ConsumeItem`); vanilla respawns it with `ItemDrop.DropItem` inside `Projectile.SpawnOnHit`
  (impact or TTL), on the thrower's side. `ThrowPatches` flags that call when the projectile's
  `m_spawnItem` is a spear and its owner is the local player wearing the charm, and picks the
  returned drop up at once with vanilla `Humanoid.Pickup` unless the thrower died meanwhile. Vanilla
  pickup already files the spear back into the slot it was thrown from, re-equips it into an empty
  hand and refuses when the inventory is full; don't redo any of that in the patch. The pickup runs
  in a coroutine on the thrower after `RecallDelay` (1 s), skipped if the drop is gone or the
  thrower died. A successful recall plays vanilla `vfx_odin_despawn` at the landing point and
  `vfx_Potion_eitr_minor` on the thrower, looked up on first use; a missing prefab warns once and
  only drops that visual.
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
