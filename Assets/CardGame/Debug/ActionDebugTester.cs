using System.Linq;
using Controllers;
using Enums;
using Models.Cards;
using Models.Player;
using Testing;
using Views;
using Actions;
using UnityEngine;

namespace Debugging
{
    public sealed class ActionDebugTester : MonoBehaviour
    {
        [Header("Players")]
        [SerializeField] private GameManager playerManager;
        [SerializeField] private GameManager opponentManager;

        [Header("Optional Turn View (for phase checks)")]
        [SerializeField] private TurnView turnView;

        private PlayerController PlayerController => playerManager != null ? playerManager.Controller : null;
        private PlayerController OpponentController => opponentManager != null ? opponentManager.Controller : null;
        private string PlayerLabel => playerManager != null ? playerManager.PlayerLabel : "Player";
        public void DebugSummonFirstMonsterInHand()
        {
            var controller = PlayerController;
            if (controller == null)
            {
                Debug.LogWarning($"[ActionDebugTester][{PlayerLabel}] PlayerController is null.");
                return;
            }

            PlayerModel model = controller.Player.Model;
            var firstMonster = model.Hand.OfType<MonsterCard>().FirstOrDefault();

            if (firstMonster == null)
            {
                Debug.LogWarning($"[ActionDebugTester][{PlayerLabel}] No MonsterCard in hand to summon.");
                return;
            }

            bool success = controller.SummonMonster(firstMonster);
            Debug.Log(success
                ? $"[ActionDebugTester][{PlayerLabel}] Summon request for '{firstMonster.CardName}' sent (see [ACTION][{PlayerLabel}.SUMMON] logs for details)."
                : $"[ActionDebugTester][{PlayerLabel}] Summon for '{firstMonster.CardName}' was rejected (see [ACTION][{PlayerLabel}.SUMMON] logs for reason)."
            );
        }

        public void DebugAttackWithStrongestMonster()
        {
            var selfController = PlayerController;
            var opponentController = OpponentController;

            if (selfController == null || opponentController == null)
            {
                Debug.LogWarning($"[ActionDebugTester][{PlayerLabel}] PlayerController or OpponentController is null.");
                return;
            }

            if (turnView != null && turnView.CurrentPhase != TurnPhase.Battle)
            {
                Debug.LogWarning($"[ActionDebugTester][{PlayerLabel}] Current phase is {turnView.CurrentPhase}, expected Battle for attacks.");
            }

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
                Debug.LogWarning($"[ActionDebugTester][{PlayerLabel}] No monster on self field to attack with.");
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
                targetSlot = 0;
            }

            try
            {
                if (GameActionQueueRunner.Instance != null
                    && playerManager != null
                    && opponentManager != null
                    && playerManager.PlayerView != null
                    && opponentManager.PlayerView != null)
                {
                    var selfView = playerManager.PlayerView;
                    var opponentView = opponentManager.PlayerView;

                    Transform attackerTransform = selfView.GetMonsterCardTransformAt(attackerSlot);
                    Transform targetTransform = null;

                    if (weakestDefender != null)
                    {
                        targetTransform = opponentView.GetMonsterCardTransformAt(targetSlot);
                    }

                    var action = new MonsterAttackAction(
                        selfController,
                        opponentController,
                        attackerSlot,
                        targetSlot,
                        attackerTransform,
                        targetTransform);

                    GameActionQueueRunner.Instance.Queue.Enqueue(action);
                }
                else
                {
                    selfController.AttackWithMonster(attackerSlot, targetSlot, opponentController);
                }

                Debug.Log($"[ActionDebugTester][{PlayerLabel}] Attack request: attackerSlot={attackerSlot}, targetSlot={targetSlot} (see [ACTION][{PlayerLabel}.BATTLE] logs for details).");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[ActionDebugTester][{PlayerLabel}] Error when calling AttackWithMonster/MonsterAttackAction: {ex.Message}");
            }
        }
    }
}
