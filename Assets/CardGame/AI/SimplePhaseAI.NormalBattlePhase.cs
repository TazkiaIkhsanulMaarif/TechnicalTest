using Controllers;
using Models.Cards;
using Models.Player;

namespace AI
{
    public sealed partial class SimplePhaseAI
    {
        partial void PerformBattlePhaseActions(PlayerController selfController, PlayerController opponentController, string label)
        {
            if (opponentController == null)
                return;

            PlayerModel selfModel = selfController.Player.Model;
            PlayerModel opponentModel = opponentController.Player.Model;

            int attackerSlot = -1;
            MonsterCard attackerCard = null;

            for (int i = 0; i < selfModel.MonsterField.Count; i++)
            {
                if (selfModel.MonsterField[i] is MonsterCard monster)
                {
                    if (attackerCard == null || monster.Attack > attackerCard.Attack)
                    {
                        attackerCard = monster;
                        attackerSlot = i;
                    }
                }
            }

            if (attackerSlot == -1)
            {
                UnityEngine.Debug.Log($"[AI][{label}] Battle: No monsters to attack with.");
                return;
            }

            int targetSlot = -1;
            MonsterCard weakestDefender = null;

            for (int i = 0; i < opponentModel.MonsterField.Count; i++)
            {
                if (opponentModel.MonsterField[i] is MonsterCard defender)
                {
                    if (weakestDefender == null || defender.Attack < weakestDefender.Attack)
                    {
                        weakestDefender = defender;
                        targetSlot = i;
                    }
                }
            }

            if (weakestDefender == null)
            {
                int attackerEffectiveDefense = selfModel.GetEffectiveDefenseAt(attackerSlot);
                int attackerAttackValue = selfModel.GetEffectiveAttackAt(attackerSlot);
                bool opponentCanKillNextTurn = false;

                foreach (CardBase card in opponentModel.Hand)
                {
                    if (card is MonsterCard candidate && candidate.Attack > attackerEffectiveDefense)
                    {
                        opponentCanKillNextTurn = true;
                        break;
                    }
                }

                bool opponentHasSetCards = false;
                var enemySpellField = opponentModel.SpellTrapField;
                for (int i = 0; i < enemySpellField.Count; i++)
                {
                    if (enemySpellField[i] != null)
                    {
                        opponentHasSetCards = true;
                        break;
                    }
                }

                bool isLethal = attackerAttackValue >= opponentModel.LifePoint;

                if (isLethal)
                {
                    targetSlot = 0;
                    UnityEngine.Debug.Log($"[AI][{label}] Battle: Direct attack for lethal with '{attackerCard.CardName}' ATK {attackerAttackValue} (opponent LP {opponentModel.LifePoint}).");
                }
                else if (opponentCanKillNextTurn || opponentHasSetCards)
                {
                    UnityEngine.Debug.Log($"[AI][{label}] Battle: Skip direct attack with '{attackerCard.CardName}' due to potential threats (stronger monster in hand or set cards).");
                    return;
                }
                else
                {
                    targetSlot = 0;
                    UnityEngine.Debug.Log($"[AI][{label}] Battle: Direct attack with '{attackerCard.CardName}' from slot {attackerSlot} (opponent has no monsters and no obvious threat).");
                }
            }
            else
            {
                UnityEngine.Debug.Log($"[AI][{label}] Battle: Attack weakest enemy '{weakestDefender.CardName}' (ATK {weakestDefender.Attack}) from slot {attackerSlot}.");
            }

            selfController.AttackWithMonster(attackerSlot, targetSlot, opponentController);
        }
    }
}
