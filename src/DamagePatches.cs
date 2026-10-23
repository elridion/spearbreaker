using HarmonyLib;

namespace Spearbreaker
{
    /// <summary>
    /// Caps a creature's pierce resistance at normal for spear hits from a player with Spearbreaker
    /// active. Vanilla looks the resistances up in <c>GetDamageModifiers</c> from inside
    /// <c>RPC_Damage</c>, which runs where the creature is owned; the hit being resolved is remembered
    /// for that call only. Immunity and weaknesses are left alone, and players hit in PvP keep vanilla
    /// resistances.
    /// </summary>
    [HarmonyPatch(typeof(Character))]
    internal static class DamagePatches
    {
        private static Character s_target;
        private static HitData s_hit;

        [HarmonyPrefix]
        [HarmonyPatch("RPC_Damage")]
        private static void RPC_Damage_Prefix(Character __instance, HitData hit)
        {
            s_target = __instance;
            s_hit = hit;
        }

        // A finalizer, so a throwing RPC_Damage never leaves a stale hit behind.
        [HarmonyFinalizer]
        [HarmonyPatch("RPC_Damage")]
        private static void RPC_Damage_Finalizer()
        {
            s_target = null;
            s_hit = null;
        }

        [HarmonyPostfix]
        [HarmonyPatch(nameof(Character.GetDamageModifiers))]
        private static void GetDamageModifiers_Postfix(Character __instance, ref HitData.DamageModifiers __result)
        {
            if (s_hit == null || __instance != s_target || !Bypasses(__instance, s_hit)) return;
            __result.m_pierce = CapResistance(__result.m_pierce);
        }

        private static bool Bypasses(Character target, HitData hit)
        {
            if (target.IsPlayer() || hit.m_skill != Skills.SkillType.Spears || hit.m_damage.m_pierce <= 0f) return false;
            return hit.GetAttacker() is Player attacker && attacker.GetSEMan().HaveStatusAttribute(SE_Spearbreaker.BypassAttribute);
        }

        private static HitData.DamageModifier CapResistance(HitData.DamageModifier modifier)
        {
            switch (modifier)
            {
                case HitData.DamageModifier.Resistant:
                case HitData.DamageModifier.VeryResistant:
                case HitData.DamageModifier.SlightlyResistant:
                    return HitData.DamageModifier.Normal;
                default:
                    return modifier;
            }
        }
    }
}
