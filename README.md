# Spearbreaker

Valheim mod (BepInEx + Jotunn) adding the Spearbreaker charm: a trinket whose full-adrenaline effect
lets spear hits ignore pierce resistance and deal extra pierce damage, so spears stop sharing every
weakness of bows.

Players: install it from a mod manager. Features, recipe and settings are on the mod page, whose
source is [`package/README.md`](package/README.md).

## Building

Requires the .NET SDK (8+) and a Valheim install. The build locates Valheim through the Steam
registry entry; override with `-p:VALHEIM_INSTALL="<path>"`.

```
dotnet build -c Release                           # build bin/Release/Spearbreaker.dll
dotnet build -c Release -t:Package                # also write dist/Spearbreaker-<version>.zip
```

The first build generates publicized Valheim assemblies in `Valheim_Data/Managed/publicized_assemblies`.

The build touches nothing outside the repo. To test in game, install the zip with a mod manager
(e.g. import it into a [Gale](https://github.com/Kesomannen/gale) profile).

## Project layout

| Path | Purpose |
| --- | --- |
| `src/` | Plugin source: the trinket, its status effect, the damage patches |
| `assets/icon.png` | Item and status effect icon, embedded in the DLL |
| `tools/make_icons.ps1` | Generates `assets/icon.png` and `package/icon.png` from game-icons.net |
| `package/` | Mod page README and icon shipped in the package |

No Unity project or asset bundles: the trinket is a copy of the vanilla Heart of the Forest.

## Releases

CI (`.github/workflows/build.yml`) builds the package on every push and pull request. Pushing a
`vX.Y.Z` tag checks that the tag matches the version in `Spearbreaker.csproj` and
`src/SpearbreakerPlugin.cs` and has a `CHANGELOG.md` section, then creates a GitHub release and
publishes to Hexium.

## License

Code: [MIT](LICENSE). The icon is "Shield impact" by Delapouite from
[game-icons.net](https://game-icons.net), licensed under
[CC BY 3.0](https://creativecommons.org/licenses/by/3.0/).
