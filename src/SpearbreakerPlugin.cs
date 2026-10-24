using System.Collections.Generic;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using Jotunn.Managers;
using Jotunn.Utils;

namespace Spearbreaker
{
    [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
    [BepInDependency(Jotunn.Main.ModGuid)]
    [NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.Minor)]
    internal class SpearbreakerPlugin : BaseUnityPlugin
    {
        public const string PluginGuid = "elridion.spearbreaker";
        public const string PluginName = "Spearbreaker";
        public const string PluginVersion = "0.1.0";

        public static ConfigEntry<float> Duration;
        public static ConfigEntry<float> AdrenalineToFill;
        public static ConfigEntry<float> PierceBonus;

        private Harmony _harmony;

        private void Awake()
        {
            var adminOnly = new ConfigurationManagerAttributes { IsAdminOnly = true };

            Duration = Config.Bind("Trinket", "Duration", 30f,
                new ConfigDescription("Seconds the effect lasts once the trinket's adrenaline is full.",
                    new AcceptableValueRange<float>(5f, 300f), adminOnly));
            AdrenalineToFill = Config.Bind("Trinket", "AdrenalineToFill", 50f,
                new ConfigDescription("Adrenaline the trinket needs before its effect fires (vanilla Heart of the Forest: 50).",
                    new AcceptableValueRange<float>(10f, 200f), adminOnly));
            PierceBonus = Config.Bind("Trinket", "PierceBonus", 1f,
                new ConfigDescription("Extra pierce damage on spear hits while the full-adrenaline effect lasts, as a fraction (1 = +100%).",
                    new AcceptableValueRange<float>(0f, 3f), adminOnly));

            AddLocalizations();
            PrefabManager.OnVanillaPrefabsAvailable += SpearbreakerItem.Register;

            _harmony = new Harmony(PluginGuid);
            _harmony.PatchAll(typeof(SpearbreakerPlugin).Assembly);
        }

        private void OnDestroy()
        {
            _harmony?.UnpatchSelf();
        }

        private static void AddLocalizations()
        {
            LocalizationManager.Instance.GetLocalization().AddTranslation("English", new Dictionary<string, string>
            {
                { "item_trinket_spearbreaker", "Spearbreaker charm" },
                { "item_trinket_spearbreaker_description", "A bronze shard of a split shield. Spears remember the hide they could not pierce." },
                { "se_spearbreaker", "Spearbreaker surge" },
                { "se_spearbreaker_start", "Your spear finds the gaps" },
                { "se_spearbreaker_tooltip", "Spear hits deal $1% more pierce damage for $2 s." },
                { "se_spearbreaker_passive", "Spearbreaker" },
                { "se_spearbreaker_passive_tooltip", "Spear hits find the gaps: creatures that resist pierce take normal damage, others take pierce damage as if one step weaker to it." },
            });
        }
    }
}
