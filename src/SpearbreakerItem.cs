using Jotunn.Configs;
using Jotunn.Entities;
using Jotunn.Managers;
using UnityEngine;

namespace Spearbreaker
{
    /// <summary>
    /// Adds the Spearbreaker charm: a copy of the vanilla Bronze Pendant whose full-adrenaline effect is
    /// <see cref="SE_Spearbreaker"/>. Every copy of an item shares its prefab's SharedData, so changing
    /// the adrenaline cost there (when the server syncs its config) updates charms already crafted.
    /// </summary>
    internal static class SpearbreakerItem
    {
        public const string PrefabName = "TrinketSpearbreaker";
        // Vanilla Bronze Pendant: a Forge trinket of the same tier, so the copy looks and equips like one.
        private const string BasePrefab = "TrinketBronzeStamina";
        private const string IconResource = "Spearbreaker.icon.png";

        private static ItemDrop.ItemData.SharedData s_shared;

        public static void Register()
        {
            PrefabManager.OnVanillaPrefabsAvailable -= Register;

            var icon = LoadIcon();
            var effect = ScriptableObject.CreateInstance<SE_Spearbreaker>();
            effect.name = SE_Spearbreaker.EffectName;
            effect.m_name = "$se_spearbreaker";
            effect.m_startMessage = "$se_spearbreaker_start";
            effect.m_startMessageType = MessageHud.MessageType.TopLeft;
            effect.m_icon = icon;
            effect.m_ttl = SpearbreakerPlugin.Duration.Value;
            ItemManager.Instance.AddStatusEffect(new CustomStatusEffect(effect, false));

            var config = new ItemConfig
            {
                Name = "$item_trinket_spearbreaker",
                Description = "$item_trinket_spearbreaker_description",
                CraftingStation = CraftingStations.Forge,
                MinStationLevel = 1,
                Requirements = new[]
                {
                    new RequirementConfig("Bronze", 5),
                    new RequirementConfig("TrophyFrostTroll", 2),
                    new RequirementConfig("Ruby", 3),
                },
            };
            if (icon) config.Icons = new[] { icon };

            var item = new CustomItem(PrefabName, BasePrefab, config);
            if (!item.ItemDrop)
            {
                Jotunn.Logger.LogWarning($"Vanilla trinket '{BasePrefab}' not found; the Spearbreaker charm is not added.");
                return;
            }
            s_shared = item.ItemDrop.m_itemData.m_shared;
            s_shared.m_fullAdrenalineSE = effect;
            s_shared.m_maxAdrenaline = SpearbreakerPlugin.AdrenalineToFill.Value;
            ItemManager.Instance.AddItem(item);

            SpearbreakerPlugin.AdrenalineToFill.SettingChanged += (_, __) => s_shared.m_maxAdrenaline = SpearbreakerPlugin.AdrenalineToFill.Value;
        }

        private static Sprite LoadIcon()
        {
            using (var stream = typeof(SpearbreakerItem).Assembly.GetManifestResourceStream(IconResource))
            {
                if (stream == null)
                {
                    Jotunn.Logger.LogWarning($"Embedded icon '{IconResource}' not found; the charm keeps the Bronze Pendant's icon.");
                    return null;
                }

                // Stream.Read may return fewer bytes than asked for; read until the buffer is full.
                var bytes = new byte[stream.Length];
                int read = 0, count;
                while (read < bytes.Length && (count = stream.Read(bytes, read, bytes.Length - read)) > 0) read += count;

                var texture = new Texture2D(2, 2, TextureFormat.RGBA32, false) { name = "spearbreaker_icon" };
                if (!Jotunn.Utils.AssetUtils.LoadImage(texture, bytes))
                {
                    Jotunn.Logger.LogWarning($"Embedded icon '{IconResource}' failed to decode; the charm keeps the Bronze Pendant's icon.");
                    Object.Destroy(texture);
                    return null;
                }
                return Sprite.Create(texture, new Rect(0f, 0f, texture.width, texture.height), new Vector2(0.5f, 0.5f));
            }
        }
    }
}
