namespace CardGame.Enums
{
    public enum TrapEffectType
    {
        DamageAttacker,          // Deal fixed damage to attacking player
        ReduceIncomingDamage,    // Reduce damage by percentage
        DebuffSummonedMonster,   // Summoned monster -X% ATK
        NegateSpell,             // Negate a spell activation
        DamagePlayer             // Deal fixed damage to player directly
    }
}
