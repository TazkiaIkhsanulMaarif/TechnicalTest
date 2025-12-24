using CardGame.Controllers;
using CardGame.Enums;
using CardGame.Models.Turn;
using CardGame.Testing;
using UnityEngine;
using UnityEngine.Events;

namespace CardGame.Views
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
        [SerializeField] private CardGame.Testing.GameManager[] playerDecks;

        private TurnController controller;

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
            onGameStarted?.Invoke();
        }

        private void HandleTurnSwitched(int currentPlayerIndex)
        {
            onTurnSwitched?.Invoke(currentPlayerIndex);
        }

        private void HandlePhaseChanged(TurnPhase phase)
        {
            switch (phase)
            {
                case TurnPhase.Draw:
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

            // Simple 2-player assumption: opponent is the next index in the array
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
