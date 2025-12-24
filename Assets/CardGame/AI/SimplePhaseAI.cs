using System;
using CardGame.Controllers;
using CardGame.Enums;
using CardGame.Interfaces;
using CardGame.Models.Cards;
using CardGame.Models.Player;

namespace CardGame.AI
{
    [Serializable]
    public sealed class SimplePhaseAI : IAIPlayer
    {
        public void HandlePhase(TurnPhase phase, PlayerController self, PlayerController opponent)
        {
            if (self == null)
                return;

            string label = self.DebugLabel;

            switch (phase)
            {
                case TurnPhase.Main:
                    PerformMainPhaseActions(self, label);
                    break;
                case TurnPhase.Battle:
                    PerformBattlePhaseActions(self, opponent, label);
                    break;
                    // Draw and End: no active decision for this simple AI
            }
        }

        private static void PerformMainPhaseActions(PlayerController controller, string label)
        {
            PlayerModel model = controller.Player.Model;

            // 1) Summon monster with highest ATK to first empty monster slot
            MonsterCard bestMonster = null;

            foreach (CardBase card in model.Hand)
            {
                if (card is MonsterCard monster)
                {
                    if (bestMonster == null || monster.Attack > bestMonster.Attack)
                    {
                        bestMonster = monster;
                    }
                }
            }

            if (bestMonster != null)
            {
                UnityEngine.Debug.Log($"[AI][{label}] Main: Try summon monster '{bestMonster.CardName}' with ATK {bestMonster.Attack}.");
                // Use controller's SummonMonster, which enforces
                // 1 normal summon per turn, level restriction and field size.
                bool summoned = controller.SummonMonster(bestMonster);
                if (!summoned)
                {
                    UnityEngine.Debug.Log($"[AI][{label}] Main: Summon of '{bestMonster.CardName}' was rejected (see [ACTION][{label}.SUMMON] logs).");
                }
            }
            else
            {
                UnityEngine.Debug.Log($"[AI][{label}] Main: No monster in hand to summon.");
            }

            // 2) Use at most one spell per turn: pick first useful spell card
            CardBase spellCard = null;
            foreach (CardBase card in model.Hand)
            {
                if (card is SpellCard spell)
                {
                    spellCard = spell;
                    break;
                }
            }

            if (spellCard != null)
            {
                // Enforce "1 spell per turn" at AI level: just cast one if conditions allow
                int emptySpellSlot = -1;
                var spellField = model.SpellTrapField;
                for (int i = 0; i < spellField.Count; i++)
                {
                    if (spellField[i] == null)
                    {
                        emptySpellSlot = i;
                        break;
                    }
                }

                if (emptySpellSlot != -1)
                {
                    UnityEngine.Debug.Log($"[AI][{label}] Main: Play spell '{spellCard.CardName}' to slot {emptySpellSlot} (1 spell/turn rule).");
                    controller.PlayCard(spellCard, emptySpellSlot);
                }
                else
                {
                    UnityEngine.Debug.Log($"[AI][{label}] Main: Has spell '{spellCard.CardName}' but no empty spell/trap slot.");
                }
            }
            else
            {
                UnityEngine.Debug.Log($"[AI][{label}] Main: No spell card in hand.");
            }
        }

        private static void PerformBattlePhaseActions(PlayerController selfController, PlayerController opponentController, string label)
        {
            if (opponentController == null)
                return;

            PlayerModel selfModel = selfController.Player.Model;
            PlayerModel opponentModel = opponentController.Player.Model;

            // Select our attacker: monster with highest ATK
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
                return; // no monsters to attack with
            }

            // Find weakest enemy monster by ATK
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
                // No enemy monsters: direct attack to first slot index (0)
                targetSlot = 0;
                UnityEngine.Debug.Log($"[AI][{label}] Battle: Direct attack with '{attackerCard.CardName}' from slot {attackerSlot} (opponent has no monsters).");
            }
            else
            {
                UnityEngine.Debug.Log($"[AI][{label}] Battle: Attack weakest enemy '{weakestDefender.CardName}' (ATK {weakestDefender.Attack}) from slot {attackerSlot}.");
            }

            selfController.AttackWithMonster(attackerSlot, targetSlot, opponentController);
        }
    }
}
