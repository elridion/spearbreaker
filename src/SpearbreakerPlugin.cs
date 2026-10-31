using System.IO;
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
        public const string PluginVersion = "0.2.1";

        private const string TranslationPrefix = "Spearbreaker.translations.";

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

        // Each embedded assets/translations/<Language>.json is named after a Valheim language. Jotunn
        // falls back to English for languages and keys without a translation.
        private static void AddLocalizations()
        {
            var localization = LocalizationManager.Instance.GetLocalization();
            var assembly = typeof(SpearbreakerPlugin).Assembly;
            foreach (string resource in assembly.GetManifestResourceNames())
            {
                if (!resource.StartsWith(TranslationPrefix) || !resource.EndsWith(".json")) continue;
                string language = resource.Substring(TranslationPrefix.Length, resource.Length - TranslationPrefix.Length - ".json".Length);
                using (var reader = new StreamReader(assembly.GetManifestResourceStream(resource)))
                    localization.AddJsonFile(language, reader.ReadToEnd());
            }
        }
    }
}
