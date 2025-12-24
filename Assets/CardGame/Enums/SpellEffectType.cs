namespace CardGame.Enums
{
    public enum SpellEffectType
    {
        DestroyWeakMonster,      // destroy monster with ATK < threshold
        ReduceTargetAttack,      // -X% ATK target
        BoostAllyAttack,         // +X% ATK ally
        ReduceTargetDefense,     // -X% DEF target
        BoostAllyDefense         // +X% DEF ally
    }
}
