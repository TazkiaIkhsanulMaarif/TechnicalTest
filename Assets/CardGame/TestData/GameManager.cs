using CardGame.AI;
using CardGame.Controllers;
using CardGame.Enums;
using CardGame.Models.Deck;
using CardGame.Models.Player;
using CardGame.Interfaces;
using CardGame.Views;
using UnityEngine;

namespace CardGame.Testing
{
    public sealed class GameManager : MonoBehaviour
    {
        [Header("Deck Data")]
        [SerializeField] private DeckTestData testDeckData;

        [Header("Player View (UI + Feel)")]
        [SerializeField] private PlayerView playerView;

        [Header("Debug")]
        [SerializeField] private string playerLabel = "Player 1";

        [Header("AI Settings")]
        [SerializeField] private bool isAI;
        [SerializeField] private AIDifficulty aiDifficulty = AIDifficulty.Easy;

        [Header("Rules")]
        [SerializeField] private int startingHandSize = 4;

        private PlayerController playerController;
        private IAIPlayer aiLogic;

        public bool IsAI => isAI;
        public PlayerController Controller => playerController;
        public string PlayerLabel => playerLabel;

        private void Start()
        {
            if (testDeckData == null || testDeckData.cards == null || testDeckData.cards.Count == 0)
            {
                Debug.LogError("DeckTestData is empty!");
                return;
            }

            // Build model -> player -> controller
            DeckModel deckModel = new DeckModel(testDeckData.cards);
            var playerModel = new PlayerModel(deckModel);

            // Choose Human or AI based on toggle
            PlayerBase playerBase = isAI
                ? new AIPlayer(playerModel, aiDifficulty)
                : new HumanPlayer(playerModel);

            // Pass a label ("Player 1", "Player 2", etc.) for debug logging
            playerController = new PlayerController(playerBase, playerLabel);

            // Initialize AI logic (controller-level, not UI)
            if (isAI)
            {
                aiLogic = new SimplePhaseAI();
            }

            // Hook up UI view
            if (playerView != null)
            {
                playerView.Initialize(playerController);
            }

            // Shuffle deck once at game start
            playerController.ShuffleDeck();

            // Draw starting hand according to rule (default 4 cards)
            playerController.DrawStartingHand(startingHandSize);
        }

        /// <summary>
        /// Called at the start of this player's turn to reset per-turn state
        /// (for example, the normal summon flag).
        /// </summary>
        public void ResetTurnState()
        {
            if (playerController == null)
            {
                Debug.LogWarning("PlayerController is not initialized when resetting turn state.");
                return;
            }

            playerController.ResetTurnState();
        }

        public void DrawOneCard()
        {
            if (playerController == null)
            {
                Debug.LogWarning("PlayerController is not initialized.");
                return;
            }
            // PlayerController handles deck empty -> PlayerDied event.
            playerController.DrawCard();
        }

        public void HandleAIPhase(CardGame.Enums.TurnPhase phase, GameManager opponent)
        {
            if (!isAI || playerController == null || aiLogic == null)
                return;
            PlayerController opponentController = opponent != null ? opponent.playerController : null;
            Debug.Log($"[AI][{playerLabel}] Handling phase {phase}.");
            aiLogic.HandlePhase(phase, playerController, opponentController);
        }
    }
}
