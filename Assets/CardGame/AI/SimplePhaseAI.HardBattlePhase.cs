using Controllers;
using Models.Cards;
using Models.Player;

namespace AI
{
    public sealed partial class SimplePhaseAI
    {
        partial void PerformBattlePhaseActionsHard(PlayerController selfController, PlayerController opponentController, string label)
        {
            if (opponentController == null)
                return;

            PlayerModel self = selfController.Player.Model;
            PlayerModel opp = opponentController.Player.Model;

            if (!HasAnyMonsterOnField(self))
            {
                UnityEngine.Debug.Log($"[AI][{label}] Battle(Hard): No monsters to attack with.");
                return;
            }

            int threatLevel = CalculateThreatLevel(self, opp);
            bool opponentHasMonsters = HasAnyMonsterOnField(opp);
            int opponentSetCards = CountSetCards(opp);

            bool canDirectLethal = !opponentHasMonsters &&
                                   CalculatePotentialDirectDamage(self) >= opp.LifePoint;

            if (canDirectLethal)
            {
                UnityEngine.Debug.Log($"[AI][{label}] Battle(Hard): Direct lethal available -> go for lethal with all attackers.");

                for (int i = 0; i < self.MonsterField.Count; i++)
                {
                    if (self.MonsterField[i] is MonsterCard)
                    {
                        selfController.AttackWithMonster(i, 0, opponentController);
                    }
                }
                return;
            }

            if (threatLevel >= 5 || self.LifePoint < 2000)
            {
                UnityEngine.Debug.Log($"[AI][{label}] Battle(Hard): High threat ({threatLevel}) or low LP -> skip attacks and play defensively.");
                return;
            }

            if (opponentHasMonsters && AllOpponentMonstersStronger(self, opp))
            {
                UnityEngine.Debug.Log($"[AI][{label}] Battle(Hard): All opponent monsters stronger -> skip attacks to avoid losing board.");
                return;
            }

            if (opponentSetCards >= 2)
            {
                int weakestAttackerSlot = GetWeakestAttackerSlot(self);
                if (weakestAttackerSlot != -1)
                {
                    int targetSlot = GetWeakestDefenderSlot(opp);
                    if (targetSlot == -1)
                        targetSlot = 0;

                    UnityEngine.Debug.Log($"[AI][{label}] Battle(Hard): Bait traps with weakest attacker from slot {weakestAttackerSlot}.");
                    selfController.AttackWithMonster(weakestAttackerSlot, targetSlot, opponentController);
                }
                return;
            }

            if (threatLevel < 3 && self.LifePoint > 3000)
            {
                int attackerSlot = GetStrongestAttackerSlot(self);
                if (attackerSlot == -1)
                {
                    UnityEngine.Debug.Log($"[AI][{label}] Battle(Hard): No valid attacker slot found.");
                    return;
                }

                int targetSlot = GetWeakestDefenderSlot(opp);
                if (targetSlot == -1)
                {
                    bool opponentCanKillNextTurn = OpponentHasHandThreat(opp, self);
                    if (opponentCanKillNextTurn)
                    {
                        UnityEngine.Debug.Log($"[AI][{label}] Battle(Hard): Skip direct attack due to hand threats despite low threat level.");
                        return;
                    }

                    targetSlot = 0;
                    UnityEngine.Debug.Log($"[AI][{label}] Battle(Hard): Safe aggro -> direct attack from slot {attackerSlot}.");
                }
                else
                {
                    UnityEngine.Debug.Log($"[AI][{label}] Battle(Hard): Safe aggro -> attack weakest enemy from slot {attackerSlot}.");
                }

                selfController.AttackWithMonster(attackerSlot, targetSlot, opponentController);
                return;
            }
            UnityEngine.Debug.Log($"[AI][{label}] Battle(Hard): Fallback to normal battle behavior.");
            PerformBattlePhaseActions(selfController, opponentController, label);
        }
    }
}
