using Models.Cards;
using Models.Player;
using Enums;

namespace AI
{
    public sealed partial class SimplePhaseAI
    {
        private static bool IsSpellLikelyUseful(SpellCard spell, PlayerModel self, PlayerModel opponent)
        {
            if (spell == null || self == null)
                return false;

            switch (spell.EffectType)
            {
                case SpellEffectType.BoostAllyAttack:
                case SpellEffectType.BoostAllyDefense:
                    for (int i = 0; i < self.MonsterField.Count; i++)
                    {
                        if (self.MonsterField[i] is MonsterCard)
                            return true;
                    }
                    return false;

                case SpellEffectType.DestroyWeakMonster:
                    if (opponent == null)
                        return false;

                    for (int i = 0; i < opponent.MonsterField.Count; i++)
                    {
                        if (opponent.MonsterField[i] is MonsterCard enemy && enemy.Attack < spell.EffectValue)
                            return true;
                    }
                    return false;

                case SpellEffectType.ReduceTargetAttack:
                case SpellEffectType.ReduceTargetDefense:
                    if (opponent == null)
                        return false;

                    for (int i = 0; i < opponent.MonsterField.Count; i++)
                    {
                        if (opponent.MonsterField[i] is MonsterCard)
                            return true;
                    }
                    return false;

                default:
                    return false;
            }
        }
    }
}
