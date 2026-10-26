# Spearbreaker

A blue-grey tin trinket that lets spears punch through pierce resistance, with a burst of extra damage at
full adrenaline.

## Why

Spear and bow make same hurt: pierce. Monster laugh at arrow, monster laugh at spear too. Spear sad.

Charm fix. Wear charm, spear find gap in monster skin. Fight hard, charm get angry, spear hit twice
as hard. Bow get nothing. Spear happy.

## The Spearbreaker trinket

- A trinket, worn in the trinket slot like the vanilla ones.
- **Gungnir's Aim (while worn):** your spear hits treat creatures as one step weaker to pierce.
  - Creatures that resist pierce (slightly, normally or very) take normal pierce damage.
  - Creatures with no pierce modifier take pierce damage as if weak to it.
  - Creatures slightly weak to pierce become weak; weak becomes very weak.
  - Creatures immune to pierce stay immune.
- **Odin's Claim (30 s):** fighting fills the trinket's adrenaline bar; when it is full, your spear hits deal
  100% more pierce damage, on top of the passive.
- Works for melee spear attacks and thrown spears. Bows, crossbows and atgeirs are unchanged.
- Only affects creatures: in PvP, players keep their own resistances.

## Recipe

Forge: 5 Bronze, 2 Troll trophies, 3 Rubies.

## Configuration

`BepInEx/config/elridion.spearbreaker.cfg`. On a server, the server's values apply to everyone.

| Setting | Default | Meaning |
| --- | --- | --- |
| `Duration` | 30 | Seconds the active effect lasts |
| `AdrenalineToFill` | 50 | Adrenaline needed to fire the active effect (vanilla Heart of the Forest: 50) |
| `PierceBonus` | 1 | Extra pierce damage on spear hits while the active effect lasts (1 = +100%) |

## Requirements

- BepInEx for Valheim
- [Jotunn](https://thunderstore.io/c/valheim/p/ValheimModding/Jotunn/)

Every client and the server need the mod installed, at the same minor version.

## Uninstalling

Remove the mod from your mod manager. Charms you have crafted disappear from inventories and chests,
since the game no longer knows the item; the materials are not refunded.

## Credits

- Mod icon: "Shield impact" by Delapouite from [game-icons.net](https://game-icons.net), licensed under
  [CC BY 3.0](https://creativecommons.org/licenses/by/3.0/). Recoloured and outlined.

## Source and issues

Bug reports and feedback: the [#spearbreaker channel on Discord](https://discord.gg/fBtGdJ6YSs).

Source code (MIT license): [github.com/elridion/spearbreaker](https://github.com/elridion/spearbreaker).
