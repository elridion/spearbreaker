using System.Collections;
using HarmonyLib;
using Jotunn.Managers;
using UnityEngine;

namespace Spearbreaker
{
    /// <summary>
    /// Recalls a thrown spear to a player wearing the upgraded Spearbreaker charm shortly after it lands.
    /// Vanilla removes the spear from the inventory on the throw and turns it back into an item drop in
    /// <c>Projectile.SpawnOnHit</c>, on the thrower's side; the drop made during that call is picked up
    /// after <see cref="RecallDelay"/> through vanilla <c>Humanoid.Pickup</c>, which files it back into the
    /// slot it was thrown from and re-equips it into an empty hand. With no room in the inventory the
    /// spear stays where it landed, as in vanilla. The spear vanishes in Odin's despawn smoke and arrives
    /// with a blue sparkle on the thrower, so it visibly travels instead of silently reappearing.
    /// </summary>
    [HarmonyPatch]
    internal static class ThrowPatches
    {
        // Long enough to see the spear land and stick before it is called back.
        private const float RecallDelay = 1f;
        // The smoke Odin vanishes in, where the spear leaves the ground; fits the charm's Odin theme.
        private const string VanishEffectPrefab = "vfx_odin_despawn";
        // The blue swirl of the minor eitr mead, on the thrower as the spear arrives.
        private const string ArriveEffectPrefab = "vfx_Potion_eitr_minor";

        private static Player s_thrower;
        private static EffectList s_vanishEffect;
        private static EffectList s_arriveEffect;
        private static bool s_effectsLoaded;

        [HarmonyPrefix]
        [HarmonyPatch(typeof(Projectile), "SpawnOnHit")]
        private static void SpawnOnHit_Prefix(Projectile __instance, Character ___m_owner)
        {
            ItemDrop.ItemData item = __instance.m_spawnItem;
            if (item == null || item.m_shared.m_skillType != Skills.SkillType.Spears) return;
            if (!___m_owner || ___m_owner != Player.m_localPlayer) return;
            if (!___m_owner.GetSEMan().HaveStatusAttribute(SE_SpearbreakerPassive.BypassAttribute)) return;
            if (!HasRecallCharm(Player.m_localPlayer)) return;
            s_thrower = Player.m_localPlayer;
        }

        // The recall belongs to the upgraded charm only. The passive's attribute says a charm is worn,
        // not its quality, so the equipped charm is looked up in the inventory.
        private static bool HasRecallCharm(Player player)
        {
            foreach (ItemDrop.ItemData item in player.GetInventory().GetAllItems())
            {
                if (!item.m_equipped || !item.m_dropPrefab || item.m_dropPrefab.name != SpearbreakerItem.PrefabName) continue;
                return item.m_quality >= SpearbreakerItem.RecallQuality;
            }
            return false;
        }

        // A finalizer, so a throwing SpawnOnHit never leaves a thrower set for an unrelated drop.
        [HarmonyFinalizer]
        [HarmonyPatch(typeof(Projectile), "SpawnOnHit")]
        private static void SpawnOnHit_Finalizer()
        {
            s_thrower = null;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(ItemDrop), nameof(ItemDrop.DropItem))]
        private static void DropItem_Postfix(ItemDrop __result)
        {
            if (!s_thrower || !__result) return;
            Player thrower = s_thrower;
            s_thrower = null;
            // Runs on the player, so it stops on its own if the player logs out or is destroyed.
            thrower.StartCoroutine(Recall(thrower, __result));
        }

        private static IEnumerator Recall(Player thrower, ItemDrop drop)
        {
            yield return new WaitForSeconds(RecallDelay);

            // Someone may have picked the spear up meanwhile. A thrower who died has already left a
            // tombstone; picking the spear up now would file it into an inventory about to be destroyed.
            if (!thrower || !drop || thrower.IsDead()) yield break;
            Vector3 landedAt = drop.transform.position;
            if (!thrower.Pickup(drop.gameObject, autoequip: true, autoPickupDelay: false)) yield break;

            LoadEffects();
            s_vanishEffect?.Create(landedAt, Quaternion.identity);
            s_arriveEffect?.Create(thrower.GetCenterPoint(), Quaternion.identity, thrower.transform);
        }

        // Looked up on the first recall rather than at startup: the effects are optional, and a missing
        // prefab only costs that visual, so it warns once and the recall keeps working without it.
        private static void LoadEffects()
        {
            if (s_effectsLoaded) return;
            s_effectsLoaded = true;
            s_vanishEffect = LoadEffect(VanishEffectPrefab);
            s_arriveEffect = LoadEffect(ArriveEffectPrefab);
        }

        private static EffectList LoadEffect(string name)
        {
            GameObject prefab = PrefabManager.Cache.GetPrefab<GameObject>(name);
            if (!prefab)
            {
                Jotunn.Logger.LogWarning($"Vanilla effect '{name}' not found; thrown spears are recalled without it.");
                return null;
            }
            return new EffectList
            {
                m_effectPrefabs = new[] { new EffectList.EffectData { m_prefab = prefab } }
            };
        }
    }
}
