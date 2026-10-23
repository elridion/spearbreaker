# Spearbreaker

A bronze trinket that lets spears punch through pierce resistance.

In vanilla, spears and bows share one damage type, pierce, so every creature that shrugs off arrows
shrugs off spears too. The Spearbreaker charm gives spear fighters their own answer.

## The Spearbreaker charm

- A trinket, worn in the trinket slot like the vanilla ones. Fighting fills its adrenaline bar; when
  it is full, the charm's effect fires.
- **Effect (30 s):** your spear hits ignore pierce resistance and deal 15% more pierce damage.
  - Creatures that resist pierce take normal pierce damage instead.
  - Creatures weak to pierce stay weak.
  - Creatures immune to pierce stay immune.
- Works for melee spear attacks and thrown spears. Bows, crossbows and atgeirs are unchanged.
- Only affects creatures: in PvP, players keep their own resistances.

## Recipe

Forge: 5 Bronze, 2 Troll trophies, 3 Rubies.

## Configuration

`BepInEx/config/elridion.spearbreaker.cfg`. On a server, the server's values apply to everyone.

| Setting | Default | Meaning |
| --- | --- | --- |
| `Duration` | 30 | Seconds the effect lasts |
| `AdrenalineToFill` | 50 | Adrenaline needed to fire the effect (vanilla Bronze Pendant: 50) |
| `PierceBonus` | 0.15 | Extra pierce damage on spear hits while active (0.15 = +15%) |

## Requirements

- BepInEx for Valheim
- [Jotunn](https://thunderstore.io/c/valheim/p/ValheimModding/Jotunn/)

Every client and the server need the mod installed, at the same minor version.

## Uninstalling

Remove the mod from your mod manager. Charms you have crafted disappear from inventories and chests,
since the game no longer knows the item; the materials are not refunded.

## Credits

- Icon: "Shield impact" by Delapouite from [game-icons.net](https://game-icons.net), licensed under
  [CC BY 3.0](https://creativecommons.org/licenses/by/3.0/). Recoloured and outlined.

## Source and issues

Source code (MIT license) and bug reports: [github.com/elridion/spearbreaker](https://github.com/elridion/spearbreaker).
