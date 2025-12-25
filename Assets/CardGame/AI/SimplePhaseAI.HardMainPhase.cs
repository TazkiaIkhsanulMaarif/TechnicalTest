using System.Collections.Generic;
using Controllers;
using Enums;
using Models.Cards;
using Models.Player;

namespace AI
{
    public sealed partial class SimplePhaseAI
    {
        partial void PerformMainPhaseActionsHard(PlayerController controller, PlayerController opponent, string label)
        {
            PlayerModel self = controller.Player.Model;
            PlayerModel opp = opponent != null ? opponent.Player.Model : null;

            List<MonsterCard> monstersInHand = new();
            foreach (CardBase card in self.Hand)
            {
                if (card is MonsterCard m)
                    monstersInHand.Add(m);
            }

            MonsterCard chosenMonster = null;

            if (monstersInHand.Count > 0)
            {
                bool canOTK = opp != null && !HasAnyMonsterOnField(opp) &&
                               CalculatePotentialDirectDamageAfterSummon(self, monstersInHand) >= opp.LifePoint;

                if (canOTK)
                {
                    chosenMonster = GetHighestAttack(monstersInHand);
                    UnityEngine.Debug.Log($"[AI][{label}] Main(Hard): CanOTK detected -> summon strongest ATK '{chosenMonster.CardName}' for lethal.");
                }
                else if (self.LifePoint < 2000)
                {
                    chosenMonster = GetHighestDefense(monstersInHand);
                    UnityEngine.Debug.Log($"[AI][{label}] Main(Hard): LP < 2000 -> summon highest DEF '{chosenMonster.CardName}' (defensive).");
                }
                else if (opp != null && CountSetCards(opp) >= 2)
                {
                    chosenMonster = GetHighestDefense(monstersInHand);
                    UnityEngine.Debug.Log($"[AI][{label}] Main(Hard): Opponent has many set cards -> summon in defensive style '{chosenMonster.CardName}'.");
                }
                else if (opp != null && self.LifePoint < opp.LifePoint && self.LifePoint < 4000)
                {
                    chosenMonster = GetHighestDefense(monstersInHand);
                    UnityEngine.Debug.Log($"[AI][{label}] Main(Hard): Behind on LP and < 4000 -> prioritize DEF '{chosenMonster.CardName}'.");
                }
                else
                {
                    chosenMonster = GetHighestAttack(monstersInHand);
                    UnityEngine.Debug.Log($"[AI][{label}] Main(Hard): Default case -> summon highest ATK '{chosenMonster.CardName}'.");
                }

                if (chosenMonster != null)
                {
                    bool summoned = controller.SummonMonster(chosenMonster);
                    if (!summoned)
                    {
                        UnityEngine.Debug.Log($"[AI][{label}] Main(Hard): Summon of '{chosenMonster.CardName}' failed (summon rules / no slot).");
                    }
                }
            }
            else
            {
                UnityEngine.Debug.Log($"[AI][{label}] Main(Hard): No monster in hand to summon.");
            }

            int handCount = self.Hand.Count;

            bool conserveResources = handCount <= 2;

            int emptySpellSlot = -1;
            var spellField = self.SpellTrapField;
            for (int i = 0; i < spellField.Count; i++)
            {
                if (spellField[i] == null)
                {
                    emptySpellSlot = i;
                    break;
                }
            }

            SpellCard removalSpell = null;
            SpellCard buffSpell = null;
            foreach (CardBase card in self.Hand)
            {
                if (card is SpellCard spell)
                {
                    if (spell.SpellType == SpellType.Destroy && removalSpell == null)
                        removalSpell = spell;
                    else if ((spell.SpellType == SpellType.Buff || spell.SpellType == SpellType.Debuff) && buffSpell == null)
                        buffSpell = spell;
                }
            }

            bool opponentHasStrongMonster = opp != null && GetStrongestMonster(opp) != null &&
                                            GetStrongestMonster(opp).Attack > GetStrongestMonster(self)?.Attack;

            SpellCard spellToUse = null;
            if (!conserveResources && removalSpell != null && opponentHasStrongMonster)
            {
                spellToUse = removalSpell;
                UnityEngine.Debug.Log($"[AI][{label}] Main(Hard): Use removal spell '{spellToUse.CardName}' against strong opponent monster.");
            }
            else if (!conserveResources && buffSpell != null && HasAnyMonsterOnField(self))
            {
                spellToUse = buffSpell;
                UnityEngine.Debug.Log($"[AI][{label}] Main(Hard): Use buff/debuff spell '{spellToUse.CardName}' to strengthen board.");
            }

            if (spellToUse != null && emptySpellSlot != -1)
            {
                controller.PlayCard(spellToUse, emptySpellSlot);

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
            else if (spellToUse == null && (removalSpell != null || buffSpell != null))
            {
                UnityEngine.Debug.Log($"[AI][{label}] Main(Hard): Holds spell(s) for better timing (resource management / no good target).");
            }

            if (emptySpellSlot != -1)
            {
                TrapCard trapToSet = null;
                foreach (CardBase card in self.Hand)
                {
                    if (card is TrapCard trap)
                    {
                        trapToSet = trap;
                        break;
                    }
                }

                if (trapToSet != null)
                {
                    UnityEngine.Debug.Log($"[AI][{label}] Main(Hard): Set trap '{trapToSet.CardName}' to slot {emptySpellSlot} (pressure / bluff).");
                    controller.PlayCard(trapToSet, emptySpellSlot);
                }
            }
        }
    }
}
