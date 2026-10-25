using System.Collections.Generic;
using Jotunn.Configs;
using Jotunn.Entities;
using Jotunn.Managers;
using UnityEngine;
using UnityEngine.Rendering;

namespace Spearbreaker
{
    /// <summary>
    /// Adds the Spearbreaker charm: a copy of the vanilla Heart of the Forest whose equip effect is
    /// <see cref="SE_SpearbreakerPassive"/> and whose full-adrenaline effect is <see cref="SE_Spearbreaker"/>,
    /// recoloured to blue-grey tin. Every copy of an item shares its prefab's SharedData, so changing
    /// the adrenaline cost there (when the server syncs its config) updates charms already crafted.
    /// </summary>
    internal static class SpearbreakerItem
    {
        public const string PrefabName = "TrinketSpearbreaker";
        // Vanilla Heart of the Forest: a Forge trinket of the same tier, so the copy looks and equips like one.
        private const string BasePrefab = "TrinketBronzeHealth";
        private const string IconResource = "Spearbreaker.icon.png";
        // Blue-grey tin. The base texture is turned grey first, so the tint replaces the bronze instead of
        // mixing with it; this covers the dropped model and the one drawn on the character alike.
        private static readonly Color Tint = new Color(0.78f, 0.86f, 1f);

        private static ItemDrop.ItemData.SharedData s_shared;

        public static void Register()
        {
            PrefabManager.OnVanillaPrefabsAvailable -= Register;

            // The Heart of the Forest's own icons, tinted like the model; the embedded icon only stands in
            // when one is missing.
            var vanilla = VanillaShared();
            var icon = Tinted(vanilla != null && vanilla.m_icons != null && vanilla.m_icons.Length > 0 ? vanilla.m_icons[0] : null);
            if (!icon) icon = LoadIcon();
            var surgeIcon = Tinted(vanilla != null && vanilla.m_fullAdrenalineSE ? vanilla.m_fullAdrenalineSE.m_icon : null);
            if (!surgeIcon) surgeIcon = icon;

            var effect = ScriptableObject.CreateInstance<SE_Spearbreaker>();
            effect.name = SE_Spearbreaker.EffectName;
            effect.m_name = "$se_spearbreaker";
            effect.m_startMessage = "$se_spearbreaker_start";
            effect.m_startMessageType = MessageHud.MessageType.TopLeft;
            effect.m_icon = surgeIcon;
            effect.m_ttl = SpearbreakerPlugin.Duration.Value;
            ItemManager.Instance.AddStatusEffect(new CustomStatusEffect(effect, false));

            var passive = ScriptableObject.CreateInstance<SE_SpearbreakerPassive>();
            passive.name = SE_SpearbreakerPassive.EffectName;
            passive.m_name = "$se_spearbreaker_passive";
            passive.m_icon = icon;
            ItemManager.Instance.AddStatusEffect(new CustomStatusEffect(passive, false));

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
            Recolor(item.ItemPrefab);
            s_shared = item.ItemDrop.m_itemData.m_shared;
            // Replaces any equip effect inherited from the Heart of the Forest.
            s_shared.m_equipStatusEffect = passive;
            s_shared.m_fullAdrenalineSE = effect;
            s_shared.m_maxAdrenaline = SpearbreakerPlugin.AdrenalineToFill.Value;
            ItemManager.Instance.AddItem(item);

            SpearbreakerPlugin.AdrenalineToFill.SettingChanged += (_, __) => s_shared.m_maxAdrenaline = SpearbreakerPlugin.AdrenalineToFill.Value;
        }

        private static void Recolor(GameObject prefab)
        {
            // A dedicated server runs without a graphics device, and nobody there sees the charm.
            if (SystemInfo.graphicsDeviceType == GraphicsDeviceType.Null) return;

            var greyed = new Dictionary<Texture, Texture2D>();
            // Includes inactive children: the "attach" model drawn on the character is one.
            foreach (var renderer in prefab.GetComponentsInChildren<Renderer>(true))
            {
                var materials = renderer.sharedMaterials;
                for (int i = 0; i < materials.Length; i++)
                {
                    if (!materials[i]) continue;
                    // A copy: the clone still shares its materials with the vanilla Heart of the Forest.
                    var material = new Material(materials[i]);
                    var texture = material.HasProperty("_MainTex") ? material.mainTexture : null;
                    if (texture)
                    {
                        if (!greyed.TryGetValue(texture, out var grey)) greyed[texture] = grey = Greyscale(texture);
                        material.mainTexture = grey;
                    }
                    if (material.HasProperty("_Color")) material.color = Tint;
                    materials[i] = material;
                }
                renderer.sharedMaterials = materials;
            }
        }

        private static ItemDrop.ItemData.SharedData VanillaShared()
        {
            var prefab = PrefabManager.Instance.GetPrefab(BasePrefab);
            var drop = prefab ? prefab.GetComponent<ItemDrop>() : null;
            return drop ? drop.m_itemData.m_shared : null;
        }

        private static Texture2D Greyscale(Texture source)
        {
            var copy = ReadableCopy(source, new Rect(0f, 0f, source.width, source.height), true);
            copy.name = source.name + "_spearbreaker";
            Recolor(copy, Color.white);
            return copy;
        }

        // A greyed copy of a vanilla sprite multiplied by the tint: the same recolour the model gets.
        private static Sprite Tinted(Sprite source)
        {
            if (!source) return null;
            // A dedicated server has no graphics device to copy with, and draws no icons anyway.
            if (SystemInfo.graphicsDeviceType == GraphicsDeviceType.Null) return source;

            // The sprite may be one cell of an atlas; copy only its own rectangle.
            var rect = source.textureRect;
            var copy = ReadableCopy(source.texture, rect, false);
            copy.name = source.name + "_spearbreaker";
            copy.wrapMode = TextureWrapMode.Clamp;
            Recolor(copy, Tint);
            return Sprite.Create(copy, new Rect(0f, 0f, copy.width, copy.height), new Vector2(0.5f, 0.5f), source.pixelsPerUnit);
        }

        private static Texture2D ReadableCopy(Texture source, Rect rect, bool mipmaps)
        {
            // Vanilla textures aren't readable from the CPU; copy through a render texture to get the pixels.
            var target = RenderTexture.GetTemporary(source.width, source.height, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB);
            var previous = RenderTexture.active;
            Graphics.Blit(source, target);
            RenderTexture.active = target;
            var copy = new Texture2D((int)rect.width, (int)rect.height, TextureFormat.RGBA32, mipmaps);
            copy.ReadPixels(rect, 0, 0);
            RenderTexture.active = previous;
            RenderTexture.ReleaseTemporary(target);
            return copy;
        }

        private static void Recolor(Texture2D texture, Color tint)
        {
            var pixels = texture.GetPixels32();
            for (int i = 0; i < pixels.Length; i++)
            {
                var p = pixels[i];
                var luma = (p.r * 299 + p.g * 587 + p.b * 114) / 1000f;
                pixels[i] = new Color32((byte)(luma * tint.r), (byte)(luma * tint.g), (byte)(luma * tint.b), p.a);
            }
            texture.SetPixels32(pixels);
            texture.Apply(true, true);
        }

        private static Sprite LoadIcon()
        {
            using (var stream = typeof(SpearbreakerItem).Assembly.GetManifestResourceStream(IconResource))
            {
                if (stream == null)
                {
                    Jotunn.Logger.LogWarning($"Embedded icon '{IconResource}' not found; the charm keeps the Heart of the Forest's icon.");
                    return null;
                }

                // Stream.Read may return fewer bytes than asked for; read until the buffer is full.
                var bytes = new byte[stream.Length];
                int read = 0, count;
                while (read < bytes.Length && (count = stream.Read(bytes, read, bytes.Length - read)) > 0) read += count;

                var texture = new Texture2D(2, 2, TextureFormat.RGBA32, false) { name = "spearbreaker_icon" };
                if (!Jotunn.Utils.AssetUtils.LoadImage(texture, bytes))
                {
                    Jotunn.Logger.LogWarning($"Embedded icon '{IconResource}' failed to decode; the charm keeps the Heart of the Forest's icon.");
                    Object.Destroy(texture);
                    return null;
                }
                return Sprite.Create(texture, new Rect(0f, 0f, texture.width, texture.height), new Vector2(0.5f, 0.5f));
            }
        }
    }
}
