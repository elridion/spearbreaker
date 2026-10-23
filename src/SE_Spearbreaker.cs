namespace Spearbreaker
{
    /// <summary>
    /// The charm's full-adrenaline effect. The pierce bonus is applied here, on the attacker's side. The
    /// resistance bypass happens on the side of whoever owns the creature that is hit, which can't see
    /// other players' status effects, only the attribute flags vanilla syncs through the player's ZDO;
    /// so this effect raises an attribute bit vanilla doesn't use, and <see cref="DamagePatches"/> checks it.
    /// </summary>
    internal class SE_Spearbreaker : StatusEffect
    {
        public const string EffectName = "SE_Spearbreaker";
        // High enough above vanilla's flags (1 to 8) to stay clear of any it adds.
        public const StatusAttribute BypassAttribute = (StatusAttribute)(1 << 20);

        public override void Setup(Character character)
        {
            // The server's synced config may differ from the value at registration.
            m_ttl = SpearbreakerPlugin.Duration.Value;
            m_attributes |= BypassAttribute;
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
