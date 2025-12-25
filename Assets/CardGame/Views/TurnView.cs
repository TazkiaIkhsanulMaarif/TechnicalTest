using Controllers;
using Enums;
using Models.Turn;
using Testing;
using UnityEngine;
using UnityEngine.Events;

namespace Views
{
    public sealed class TurnView : MonoBehaviour
    {
        [Header("Config")]
        [SerializeField] private int playerCount = 2;

        [Header("Phase Events (hook UI / Feel)")]
        [SerializeField] private UnityEvent onGameStarted;
        [SerializeField] private UnityEvent onDrawPhase;
        [SerializeField] private UnityEvent onMainPhase;
        [SerializeField] private UnityEvent onBattlePhase;
        [SerializeField] private UnityEvent onEndPhase;

        [Header("Turn Events")]
        [SerializeField] private UnityEvent<int> onTurnSwitched;

        [Header("Player Decks (index = player index)")]
        [SerializeField] private Testing.GameManager[] playerDecks;

        private TurnController controller;

        public TurnPhase CurrentPhase => controller != null ? controller.CurrentPhase : TurnPhase.Draw;

        private void Awake()
        {
            var model = new TurnModel(playerCount);
            controller = new TurnController(model);

            controller.GameStarted += HandleGameStarted;
            controller.TurnSwitched += HandleTurnSwitched;
            controller.PhaseChanged += HandlePhaseChanged;
        }

        private void Start()
        {
            controller.StartGame();
        }

        private void OnDestroy()
        {
            if (controller == null) return;

            controller.GameStarted -= HandleGameStarted;
            controller.TurnSwitched -= HandleTurnSwitched;
            controller.PhaseChanged -= HandlePhaseChanged;
        }

        public void NextPhase()
        {
            controller.NextPhase();
        }

        private void HandleGameStarted()
        {
            Debug.Log("[TURN] Game Started");
            onGameStarted?.Invoke();
        }

        private void HandleTurnSwitched(int currentPlayerIndex)
        {
            int playerNumber = currentPlayerIndex + 1;
            Debug.Log($"[TURN] Switched to Player {playerNumber}");
            onTurnSwitched?.Invoke(currentPlayerIndex);
        }

        private void HandlePhaseChanged(TurnPhase phase)
        {
            int playerNumber = controller != null ? controller.CurrentPlayerIndex + 1 : -1;

            // Main phase flow log: Player X + phase name
            Debug.Log($"[TURN] Player {playerNumber} - {phase}");

            switch (phase)
            {
                case TurnPhase.Draw:
                    ResetTurnStateForCurrentPlayer();
                    onDrawPhase?.Invoke();
                    DrawForCurrentPlayer();
                    break;
                case TurnPhase.Main:
                    onMainPhase?.Invoke();
                    break;
                case TurnPhase.Battle:
                    onBattlePhase?.Invoke();
                    break;
                case TurnPhase.End:
                    onEndPhase?.Invoke();
                    break;
            }

            HandleAIForCurrentPlayer(phase);
        }

        private void ResetTurnStateForCurrentPlayer()
        {
            if (controller == null)
                return;

            int index = controller.CurrentPlayerIndex;

            if (playerDecks == null || index < 0 || index >= playerDecks.Length)
            {
                Debug.LogWarning($"No deck assigned for player index {index} in TurnView when resetting turn state.");
                return;
            }

            GameManager deck = playerDecks[index];

            if (deck == null)
            {
                Debug.LogWarning($"GameManager is null for player index {index} when resetting turn state.");
                return;
            }

            deck.ResetTurnState();
        }

        private void DrawForCurrentPlayer()
        {
            if (controller == null)
                return;

            int index = controller.CurrentPlayerIndex;

            if (playerDecks == null || index < 0 || index >= playerDecks.Length)
            {
                Debug.LogWarning($"No deck assigned for player index {index} in TurnView.");
                return;
            }

            GameManager deck = playerDecks[index];

            if (deck == null)
            {
                Debug.LogWarning($"GameManager is null for player index {index}.");
                return;
            }

            Debug.Log($"[TURN] Player {index + 1} draws a card");
            deck.DrawOneCard();
        }

        private void HandleAIForCurrentPlayer(TurnPhase phase)
        {
            if (controller == null)
                return;

            int index = controller.CurrentPlayerIndex;

            if (playerDecks == null || index < 0 || index >= playerDecks.Length)
                return;

            GameManager current = playerDecks[index];
            if (current == null || !current.IsAI)
                return;

            GameManager opponent = null;
            if (playerDecks.Length > 1)
            {
                int opponentIndex = (index + 1) % playerDecks.Length;
                if (opponentIndex >= 0 && opponentIndex < playerDecks.Length)
                {
                    opponent = playerDecks[opponentIndex];
                }
            }

            current.HandleAIPhase(phase, opponent);
        }
    }
}
