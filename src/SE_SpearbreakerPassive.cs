namespace Spearbreaker
{
    /// <summary>
    /// The charm's passive, active while it is worn: spear hits treat a creature's pierce modifier one
    /// step weaker. The shift happens on the side of whoever owns the creature that is hit, which can't
    /// see other players' status effects, only the attribute flags vanilla syncs through the player's ZDO;
    /// so this effect raises an attribute bit vanilla doesn't use, and <see cref="DamagePatches"/> checks it.
    /// </summary>
    internal class SE_SpearbreakerPassive : StatusEffect
    {
        public const string EffectName = "SE_SpearbreakerPassive";
        // High enough above vanilla's flags (1 to 8) to stay clear of any it adds.
        public const StatusAttribute BypassAttribute = (StatusAttribute)(1 << 20);

        public override void Setup(Character character)
        {
            m_attributes |= BypassAttribute;
            base.Setup(character);
        }

        public override string GetTooltipString()
        {
            return Localization.instance.Localize("$se_spearbreaker_passive_tooltip");
        }
    }
}
