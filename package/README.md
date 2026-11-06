# Spearbreaker

Makes spears a viable weapon. Craft the Spearbreaker charm to give your spears an edge of their own over bows and get your thrown spears back.

## Features

- **Gungnir's Aim (while worn):** spear hits, melee and thrown, treat creatures as one step weaker
  to pierce. Resistant creatures take normal damage, normal ones take damage as if weak, and weak
  ones become very weak. Immune creatures stay immune.
- **Odin's Claim:** when the trinket's adrenaline bar is full, your spear hits deal 100% more
  pierce damage for 30 seconds.
- **Upgraded charm:** a thrown spear returns to you after it lands, one second for every 30 m it
  flew, back into its slot and your hand. With a full inventory it stays where it landed.
- Bows, crossbows and atgeirs are unchanged, and in PvP players keep their own resistances.

## Recipe

| Quality | Station | Cost |
| --- | --- | --- |
| 1 | Forge level 1 | 5 Iron, 5 Guck, 1 Abomination trophy |
| 2 (thrown spear recall) | Forge level 2 | 5 Silver, 1 Thunderstone |

The Thunderstone is the one Haldor sells for the Obliterator.

## Installation

- Every client and the server need the mod, at the same minor version. Requires
  [Jotunn](https://thunderstore.io/c/valheim/p/ValheimModding/Jotunn/).
- The trinket and its effects follow the game's language, in every language Valheim offers except
  Abenaki. The translations have not been checked by native speakers yet.
- Uninstalling removes crafted charms from inventories and chests, since the game no longer knows
  the item; the materials are not refunded.

## Configuration

Settings are in `BepInEx/config/elridion.spearbreaker.cfg` (or a config manager). On a server, the
server's values apply to everyone.

| Key | Default | Description |
| --- | --- | --- |
| `Duration` | 30 | Seconds Odin's Claim lasts (5-300). |
| `AdrenalineToFill` | 50 | Adrenaline needed to trigger Odin's Claim (10-200). |
| `PierceBonus` | 1 | Extra pierce damage during Odin's Claim, 1 = +100% (0-3). |

## Credits

- The icon is "Shield impact" by Delapouite from [game-icons.net](https://game-icons.net),
  licensed under [CC BY 3.0](https://creativecommons.org/licenses/by/3.0/). Recoloured and outlined.
