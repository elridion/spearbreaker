namespace Spearbreaker
{
    /// <summary>
    /// The charm's full-adrenaline effect: extra pierce damage on spear hits. Applied on the attacker's
    /// side, where vanilla calls <c>ModifyAttack</c> for every attack, thrown spears included, so unlike
    /// the passive's resistance shift (<see cref="SE_SpearbreakerPassive"/>) it needs nothing synced.
    /// </summary>
    internal class SE_Spearbreaker : StatusEffect
    {
        public const string EffectName = "SE_Spearbreaker";

        public override void Setup(Character character)
        {
            // The server's synced config may differ from the value at registration.
            m_ttl = SpearbreakerPlugin.Duration.Value;
            base.Setup(character);
        }

        public override void ModifyAttack(Skills.SkillType skill, ref HitData hitData)
        {
            if (hitData.m_skill != Skills.SkillType.Spears) return;
            hitData.m_damage.m_pierce *= 1f + SpearbreakerPlugin.PierceBonus.Value;
        }

        public override string GetTooltipString()
        {
            int bonus = UnityEngine.Mathf.RoundToInt(SpearbreakerPlugin.PierceBonus.Value * 100f);
            int seconds = UnityEngine.Mathf.RoundToInt(SpearbreakerPlugin.Duration.Value);
            return Localization.instance.Localize("$se_spearbreaker_tooltip", bonus.ToString(), seconds.ToString());
        }
    }
}
