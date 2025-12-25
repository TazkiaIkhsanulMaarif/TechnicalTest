using System.Collections.Generic;
using Models.Cards;
using Models.Player;

namespace AI
{
    public sealed partial class SimplePhaseAI
    {
        #region Hard helper methods

        private static int CountSetCards(PlayerModel player)
        {
            int count = 0;
            var spellField = player.SpellTrapField;
            for (int i = 0; i < spellField.Count; i++)
            {
                if (spellField[i] != null)
                    count++;
            }
            return count;
        }

        private static bool HasAnyMonsterOnField(PlayerModel player)
        {
            for (int i = 0; i < player.MonsterField.Count; i++)
            {
                if (player.MonsterField[i] is MonsterCard)
                    return true;
            }
            return false;
        }

        private static MonsterCard GetStrongestMonster(PlayerModel player)
        {
            MonsterCard strongest = null;
            for (int i = 0; i < player.MonsterField.Count; i++)
            {
                if (player.MonsterField[i] is MonsterCard m)
                {
                    if (strongest == null || m.Attack > strongest.Attack)
                        strongest = m;
                }
            }
            return strongest;
        }

        private static MonsterCard GetHighestAttack(List<MonsterCard> list)
        {
            MonsterCard best = null;
            foreach (var m in list)
            {
                if (best == null || m.Attack > best.Attack)
                    best = m;
            }
            return best;
        }

        private static MonsterCard GetHighestDefense(List<MonsterCard> list)
        {
            MonsterCard best = null;
            foreach (var m in list)
            {
                if (best == null || m.Defense > best.Defense)
                    best = m;
            }
            return best;
        }

        private static int CalculatePotentialDirectDamage(PlayerModel self)
        {
            int sum = 0;
            for (int i = 0; i < self.MonsterField.Count; i++)
            {
                if (self.MonsterField[i] is MonsterCard)
                {
                    sum += self.GetEffectiveAttackAt(i);
                }
            }
            return sum;
        }

        private static int CalculatePotentialDirectDamageAfterSummon(PlayerModel self, List<MonsterCard> handMonsters)
        {
            // Approximate: assume we can add the highest ATK monster from hand to our current direct damage potential
            int current = CalculatePotentialDirectDamage(self);
            MonsterCard bestHand = GetHighestAttack(handMonsters);
            if (bestHand != null)
                current += bestHand.Attack;
            return current;
        }

        private static int CalculateThreatLevel(PlayerModel self, PlayerModel opp)
        {
            int threat = 0;
            MonsterCard myStrongest = GetStrongestMonster(self);

            for (int i = 0; i < opp.MonsterField.Count; i++)
            {
                if (opp.MonsterField[i] is MonsterCard enemy)
                {
                    if (myStrongest == null || enemy.Attack > myStrongest.Attack)
                        threat += 3;
                }
            }

            threat += CountSetCards(opp) * 2;

            int myBestDef = 0;
            for (int i = 0; i < self.MonsterField.Count; i++)
            {
                if (self.MonsterField[i] is MonsterCard mine && mine.Defense > myBestDef)
                    myBestDef = mine.Defense;
            }

            foreach (CardBase card in opp.Hand)
            {
                if (card is MonsterCard m && m.Attack > myBestDef)
                    threat += 1;
            }

            return threat;
        }

        private static bool AllOpponentMonstersStronger(PlayerModel self, PlayerModel opp)
        {
            MonsterCard myStrongest = GetStrongestMonster(self);
            if (myStrongest == null)
                return true;

            for (int i = 0; i < opp.MonsterField.Count; i++)
            {
                if (opp.MonsterField[i] is MonsterCard enemy && enemy.Attack <= myStrongest.Attack)
                    return false;
            }

            return true;
        }

        private static int GetWeakestAttackerSlot(PlayerModel self)
        {
            int slot = -1;
            int weakestAtk = int.MaxValue;
            for (int i = 0; i < self.MonsterField.Count; i++)
            {
                if (self.MonsterField[i] is MonsterCard m)
                {
                    if (m.Attack < weakestAtk)
                    {
                        weakestAtk = m.Attack;
                        slot = i;
                    }
                }
            }
            return slot;
        }

        private static int GetStrongestAttackerSlot(PlayerModel self)
        {
            int slot = -1;
            int strongestAtk = -1;
            for (int i = 0; i < self.MonsterField.Count; i++)
            {
                if (self.MonsterField[i] is MonsterCard m)
                {
                    if (m.Attack > strongestAtk)
                    {
                        strongestAtk = m.Attack;
                        slot = i;
                    }
                }
            }
            return slot;
        }

        private static int GetWeakestDefenderSlot(PlayerModel opp)
        {
            int slot = -1;
            int weakestAtk = int.MaxValue;
            for (int i = 0; i < opp.MonsterField.Count; i++)
            {
                if (opp.MonsterField[i] is MonsterCard m)
                {
                    if (m.Attack < weakestAtk)
                    {
                        weakestAtk = m.Attack;
                        slot = i;
                    }
                }
            }
            return slot;
        }

        private static bool OpponentHasHandThreat(PlayerModel opp, PlayerModel self)
        {
            int myBestDef = 0;
            for (int i = 0; i < self.MonsterField.Count; i++)
            {
                if (self.MonsterField[i] is MonsterCard mine && mine.Defense > myBestDef)
                    myBestDef = mine.Defense;
            }

            foreach (CardBase card in opp.Hand)
            {
                if (card is MonsterCard m && m.Attack > myBestDef)
                    return true;
            }
            return false;
        }

        #endregion
    }
}
