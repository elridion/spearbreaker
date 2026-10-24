using HarmonyLib;

namespace Spearbreaker
{
    /// <summary>
    /// Shifts a creature's pierce modifier one step weaker for spear hits from a player wearing the
    /// Spearbreaker charm. Vanilla looks the modifiers up in <c>GetDamageModifiers</c> from inside
    /// <c>RPC_Damage</c>, which runs where the creature is owned; the hit being resolved is remembered
    /// for that call only. Immunity is left alone, and players hit in PvP keep vanilla modifiers.
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
            if (s_hit == null || __instance != s_target || !Applies(__instance, s_hit)) return;
            __result.m_pierce = Weaken(__result.m_pierce);
        }

        private static bool Applies(Character target, HitData hit)
        {
            if (target.IsPlayer() || hit.m_skill != Skills.SkillType.Spears || hit.m_damage.m_pierce <= 0f) return false;
            return hit.GetAttacker() is Player attacker && attacker.GetSEMan().HaveStatusAttribute(SE_SpearbreakerPassive.BypassAttribute);
        }

        // Every resistance drops to normal, normal becomes weak, and weaknesses go one step further.
        // VeryWeak is already the weakest modifier vanilla has; Immune and Ignore stay as they are.
        private static HitData.DamageModifier Weaken(HitData.DamageModifier modifier)
        {
            switch (modifier)
            {
                case HitData.DamageModifier.Resistant:
                case HitData.DamageModifier.VeryResistant:
                case HitData.DamageModifier.SlightlyResistant:
                    return HitData.DamageModifier.Normal;
                case HitData.DamageModifier.Normal:
                case HitData.DamageModifier.SlightlyWeak:
                    return HitData.DamageModifier.Weak;
                case HitData.DamageModifier.Weak:
                    return HitData.DamageModifier.VeryWeak;
                default:
                    return modifier;
            }
        }
    }
}
