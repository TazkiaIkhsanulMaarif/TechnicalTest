using System.Linq;
using CardGame.Controllers;
using CardGame.Enums;
using CardGame.Models.Cards;
using CardGame.Models.Player;
using CardGame.Testing;
using CardGame.Views;
using UnityEngine;

namespace CardGame.Debugging
{
    /// <summary>
    /// Simple debug helper to manually trigger actions via UI buttons:
    /// - Summon first monster in hand (respecting Summon rules handled by PlayerController)
    /// - Attack automatically with strongest monster (like SimplePhaseAI)
    /// Attach this to a GameObject and wire public methods to UI Buttons.
    /// </summary>
    public sealed class ActionDebugTester : MonoBehaviour
    {
        [Header("Players")]
        [SerializeField] private GameManager playerManager;      // self
        [SerializeField] private GameManager opponentManager;    // opponent

        [Header("Optional Turn View (for phase checks)")]
        [SerializeField] private TurnView turnView;

        private PlayerController PlayerController => playerManager != null ? playerManager.Controller : null;
        private PlayerController OpponentController => opponentManager != null ? opponentManager.Controller : null;
        private string PlayerLabel => playerManager != null ? playerManager.PlayerLabel : "Player";

        // Call from a UI Button to try summoning the first monster in hand
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

        // Call from a UI Button to perform an automatic attack:
        // - Pick strongest own monster (highest ATK) as attacker
        // - If opponent has monsters: target weakest enemy monster (lowest ATK)
        // - If opponent has no monsters: direct attack (targetSlot = 0)
        public void DebugAttackWithStrongestMonster()
        {
            var selfController = PlayerController;
            var opponentController = OpponentController;

            if (selfController == null || opponentController == null)
            {
                Debug.LogWarning($"[ActionDebugTester][{PlayerLabel}] PlayerController or OpponentController is null.");
                return;
            }

            // Optional phase check
            if (turnView != null && turnView.CurrentPhase != TurnPhase.Battle)
            {
                Debug.LogWarning($"[ActionDebugTester][{PlayerLabel}] Current phase is {turnView.CurrentPhase}, expected Battle for attacks.");
            }

            PlayerModel selfModel = selfController.Player.Model;
            PlayerModel opponentModel = opponentController.Player.Model;

            // Pick attacker: own monster with highest ATK
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

            // Determine target
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
                // No enemy monsters: direct attack to index 0
                targetSlot = 0;
            }

            try
            {
                selfController.AttackWithMonster(attackerSlot, targetSlot, opponentController);
                Debug.Log($"[ActionDebugTester][{PlayerLabel}] Attack request: attackerSlot={attackerSlot}, targetSlot={targetSlot} (see [ACTION][{PlayerLabel}.BATTLE] logs for details).");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[ActionDebugTester][{PlayerLabel}] Error when calling AttackWithMonster: {ex.Message}");
            }
        }
    }
}
