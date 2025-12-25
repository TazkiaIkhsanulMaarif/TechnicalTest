using Controllers;
using Models.Cards;
using Models.Player;

namespace AI
{
    public sealed partial class SimplePhaseAI
    {
        partial void PerformMainPhaseActions(PlayerController controller, PlayerController opponent, string label)
        {
            PlayerModel model = controller.Player.Model;
            PlayerModel opponentModel = opponent != null ? opponent.Player.Model : null;
            MonsterCard bestMonster = null;

            bool prioritizeDefense = opponentModel != null && model.LifePoint < opponentModel.LifePoint;

            foreach (CardBase card in model.Hand)
            {
                if (card is not MonsterCard monster)
                    continue;

                if (bestMonster == null)
                {
                    bestMonster = monster;
                    continue;
                }

                if (prioritizeDefense)
                {
                    if (monster.Defense > bestMonster.Defense ||
                        (monster.Defense == bestMonster.Defense && monster.Attack > bestMonster.Attack))
                    {
                        bestMonster = monster;
                    }
                }
                else
                {
                    if (monster.Attack > bestMonster.Attack)
                    {
                        bestMonster = monster;
                    }
                }
            }

            if (bestMonster != null)
            {
                if (prioritizeDefense)
                {
                    UnityEngine.Debug.Log($"[AI][{label}] Main: LP is lower than opponent; prioritize defense. Try summon '{bestMonster.CardName}' DEF {bestMonster.Defense} (ATK {bestMonster.Attack}).");
                }
                else
                {
                    UnityEngine.Debug.Log($"[AI][{label}] Main: Try summon monster '{bestMonster.CardName}' with ATK {bestMonster.Attack}.");
                }
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

            SpellCard chosenSpell = null;
            foreach (CardBase card in model.Hand)
            {
                if (card is SpellCard spell && IsSpellLikelyUseful(spell, model, opponentModel))
                {
                    chosenSpell = spell;
                    break;
                }
            }

            if (chosenSpell != null && emptySpellSlot != -1)
            {
                UnityEngine.Debug.Log($"[AI][{label}] Main: Play useful spell '{chosenSpell.CardName}' to slot {emptySpellSlot} (1 spell/turn rule).");
                controller.PlayCard(chosenSpell, emptySpellSlot);

                emptySpellSlot = -1;
                for (int i = 0; i < spellField.Count; i++)
                {
                    if (spellField[i] == null)
                    {
                        emptySpellSlot = i;
                        break;
                    }
                }
            }
            else if (chosenSpell == null)
            {
                bool hasAnySpell = false;
                foreach (CardBase card in model.Hand)
                {
                    if (card is SpellCard) { hasAnySpell = true; break; }
                }

                if (hasAnySpell)
                {
                    UnityEngine.Debug.Log($"[AI][{label}] Main: Has spell(s) in hand but none are useful in current board state; skip casting.");
                }
                else
                {
                    UnityEngine.Debug.Log($"[AI][{label}] Main: No spell card in hand.");
                }
            }
            else if (emptySpellSlot == -1 && chosenSpell != null)
            {
                UnityEngine.Debug.Log($"[AI][{label}] Main: Has useful spell '{chosenSpell.CardName}' but no empty spell/trap slot.");
            }

            if (emptySpellSlot != -1)
            {
                TrapCard trapToSet = null;
                foreach (CardBase card in model.Hand)
                {
                    if (card is TrapCard trap)
                    {
                        trapToSet = trap;
                        break;
                    }
                }

                if (trapToSet != null)
                {
                    UnityEngine.Debug.Log($"[AI][{label}] Main: Set trap '{trapToSet.CardName}' to slot {emptySpellSlot}.");
                    controller.PlayCard(trapToSet, emptySpellSlot);
                }
            }
        }
    }
}
