using Controllers;
using Models.Cards;
using Models.Player;

namespace AI
{
    public sealed partial class SimplePhaseAI
    {
        partial void PerformMainPhaseActionsEasy(PlayerController controller, PlayerController opponent, string label)
        {
            PlayerModel model = controller.Player.Model;

            var monstersInHand = new System.Collections.Generic.List<MonsterCard>();
            foreach (CardBase card in model.Hand)
            {
                if (card is MonsterCard m)
                {
                    monstersInHand.Add(m);
                }
            }

            if (monstersInHand.Count > 0)
            {
                int index = UnityEngine.Random.Range(0, monstersInHand.Count);
                MonsterCard chosen = monstersInHand[index];

                bool wrongPosition = UnityEngine.Random.value < 0.5f;
                if (wrongPosition)
                {
                    UnityEngine.Debug.Log($"[AI][{label}] Main(Easy): Random summon '{chosen.CardName}' in a suboptimal position (pretend DEF mode).");
                }
                else
                {
                    UnityEngine.Debug.Log($"[AI][{label}] Main(Easy): Random summon '{chosen.CardName}' (position OK).");
                }

                bool summoned = controller.SummonMonster(chosen);
                if (!summoned)
                {
                    UnityEngine.Debug.Log($"[AI][{label}] Main(Easy): Summon of '{chosen.CardName}' failed (possible high-level without tribute or no slot).");
                }
            }
            else
            {
                UnityEngine.Debug.Log($"[AI][{label}] Main(Easy): No monster in hand to summon.");
            }

            if (UnityEngine.Random.value < 0.3f)
            {
                UnityEngine.Debug.Log($"[AI][{label}] Main(Easy): Skips casting spell this turn (random).");
            }
            else
            {
                var spellsInHand = new System.Collections.Generic.List<SpellCard>();
                var trapsInHand = new System.Collections.Generic.List<TrapCard>();

                foreach (CardBase card in model.Hand)
                {
                    if (card is SpellCard sc) spellsInHand.Add(sc);
                    else if (card is TrapCard tc) trapsInHand.Add(tc);
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

                if (emptySpellSlot != -1 && spellsInHand.Count > 0)
                {
                    int idx = UnityEngine.Random.Range(0, spellsInHand.Count);
                    SpellCard spell = spellsInHand[idx];
                    UnityEngine.Debug.Log($"[AI][{label}] Main(Easy): Randomly play spell '{spell.CardName}' to slot {emptySpellSlot} (may be wasteful).");
                    controller.PlayCard(spell, emptySpellSlot);
                }
                else
                {
                    UnityEngine.Debug.Log($"[AI][{label}] Main(Easy): No slot for spell or no spell in hand.");
                }

                if (trapsInHand.Count > 0)
                {
                    int trapSlot = -1;
                    for (int i = 0; i < spellField.Count; i++)
                    {
                        if (spellField[i] == null)
                        {
                            trapSlot = i;
                            break;
                        }
                    }

                    if (trapSlot != -1)
                    {
                        int tIdx = UnityEngine.Random.Range(0, trapsInHand.Count);
                        TrapCard trap = trapsInHand[tIdx];
                        UnityEngine.Debug.Log($"[AI][{label}] Main(Easy): Randomly set trap '{trap.CardName}' to slot {trapSlot} (non-strategic).");
                        controller.PlayCard(trap, trapSlot);
                    }
                }
            }
        }

        partial void PerformBattlePhaseActionsEasy(PlayerController selfController, PlayerController opponentController, string label)
        {
            if (opponentController == null)
                return;

            PlayerModel selfModel = selfController.Player.Model;
            PlayerModel opponentModel = opponentController.Player.Model;

            var attackerSlots = new System.Collections.Generic.List<int>();
            for (int i = 0; i < selfModel.MonsterField.Count; i++)
            {
                if (selfModel.MonsterField[i] is MonsterCard)
                {
                    attackerSlots.Add(i);
                }
            }

            if (attackerSlots.Count == 0)
            {
                UnityEngine.Debug.Log($"[AI][{label}] Battle(Easy): No monsters to attack with.");
                return;
            }

            int attackerSlot = attackerSlots[UnityEngine.Random.Range(0, attackerSlots.Count)];
            MonsterCard attackerCard = selfModel.MonsterField[attackerSlot] as MonsterCard;
            var defenderSlots = new System.Collections.Generic.List<int>();
            for (int i = 0; i < opponentModel.MonsterField.Count; i++)
            {
                if (opponentModel.MonsterField[i] is MonsterCard)
                {
                    defenderSlots.Add(i);
                }
            }

            int targetSlot;

            if (defenderSlots.Count == 0)
            {
                targetSlot = 0;
                UnityEngine.Debug.Log($"[AI][{label}] Battle(Easy): Direct attack with '{attackerCard.CardName}' from slot {attackerSlot} (ignores traps and risk).");
            }
            else
            {
                bool suicideAttack = UnityEngine.Random.value < 0.4f;
                if (suicideAttack)
                {
                    int strongestSlot = defenderSlots[0];
                    MonsterCard strongest = opponentModel.MonsterField[strongestSlot] as MonsterCard;
                    foreach (int slot in defenderSlots)
                    {
                        if (opponentModel.MonsterField[slot] is MonsterCard candidate && candidate.Attack > strongest.Attack)
                        {
                            strongest = candidate;
                            strongestSlot = slot;
                        }
                    }

                    targetSlot = strongestSlot;
                    UnityEngine.Debug.Log($"[AI][{label}] Battle(Easy): Risky attack - '{attackerCard.CardName}' attacks stronger enemy '{strongest.CardName}' (suicide chance).");
                }
                else
                {
                    targetSlot = defenderSlots[UnityEngine.Random.Range(0, defenderSlots.Count)];
                    MonsterCard randomTarget = opponentModel.MonsterField[targetSlot] as MonsterCard;
                    UnityEngine.Debug.Log($"[AI][{label}] Battle(Easy): Random attack - '{attackerCard.CardName}' attacks '{randomTarget.CardName}'.");
                }
            }

            selfController.AttackWithMonster(attackerSlot, targetSlot, opponentController);
        }
    }
}
