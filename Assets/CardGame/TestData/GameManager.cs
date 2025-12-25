using System.Collections;
using AI;
using Controllers;
using Enums;
using Models.Deck;
using Models.Player;
using Interfaces;
using Views;
using Actions;
using UnityEngine;

namespace Testing
{
    public sealed class GameManager : MonoBehaviour
    {
        [Header("Deck Data")]
        [SerializeField] private DeckTestData testDeckData;

        [Header("Player View (UI + Feel)")]
        [SerializeField] private PlayerView playerView;

        [Header("Hand Visual (deck/hand layout + anim)")]
        [SerializeField] private Transform handDeckPosition;
        [SerializeField] private Transform handParent;
        [SerializeField] private float handHorizontalSpacing = 1.2f;
        [SerializeField] private float handMaxTotalWidth = 8f;
        [SerializeField] private float handMaxTiltAngle = 12f;
        [SerializeField] private float handDrawDuration = 0.35f;
        [SerializeField] private AnimationCurve handDrawCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

        [Header("Card Interaction (hover/click)")]
        [SerializeField] private float cardHoverYOffset = 300f;
        [SerializeField] private float cardHoverScale = 1.3f;
        [SerializeField] private float cardSelectedScale = 1.3f;
        [SerializeField] private float cardHoverDuration = 0.15f;
        [SerializeField] private AnimationCurve cardHoverCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

        [Header("Debug")]
        [SerializeField] private string playerLabel = "Player 1";

        [Header("AI Settings")]
        [SerializeField] private bool isAI;
        [SerializeField] private AIDifficulty aiDifficulty = AIDifficulty.Easy;

        [Header("Rules")]
        [SerializeField] private int startingHandSize = 4;

        private PlayerController playerController;
        private IAIPlayer aiLogic;
        private HandLayoutAnimator handView;
        private CardInteractionAnimator cardInteractionAnimator;

        public bool IsAI => isAI;
        public PlayerController Controller => playerController;
        public string PlayerLabel => playerLabel;
        public PlayerView PlayerView => playerView;

        private void Start()
        {
            if (testDeckData == null || testDeckData.cards == null || testDeckData.cards.Count == 0)
            {
                Debug.LogError("DeckTestData is empty!");
                return;
            }
            DeckModel deckModel = new DeckModel(testDeckData.cards);
            var playerModel = new PlayerModel(deckModel);
            PlayerBase playerBase = isAI
                ? new AIPlayer(playerModel, aiDifficulty)
                : new HumanPlayer(playerModel);
            playerController = new PlayerController(playerBase, playerLabel);
            if (isAI)
            {
                aiLogic = new SimplePhaseAI();
            }
            if (playerView != null)
            {
                playerView.Initialize(playerController);
                playerView.SetRevealOnDraw(!isAI);
                cardInteractionAnimator = new CardInteractionAnimator(
                    cardHoverYOffset,
                    cardHoverScale,
                    cardSelectedScale,
                    cardHoverDuration,
                    cardHoverCurve);

                playerView.SetCardInteractionAnimator(cardInteractionAnimator);

                if (handDeckPosition != null && handParent != null)
                {
                    handView = new HandLayoutAnimator(
                        handDeckPosition,
                        handParent,
                        testDeckData != null ? new System.Func<Models.Cards.CardBase, UnityEngine.GameObject>(testDeckData.GetPrefab) : null,
                        handHorizontalSpacing,
                        handMaxTotalWidth,
                        handMaxTiltAngle,
                        handDrawDuration,
                        handDrawCurve);
                    handView.SetHoverConfig(cardHoverYOffset, cardHoverScale);

                    playerView.SetHandView(handView);
                }
            }

            playerController.ShuffleDeck();
            StartCoroutine(DrawStartingHandSequence());
        }

        private IEnumerator DrawStartingHandSequence()
        {
            if (playerController == null)
                yield break;

            for (int i = 0; i < startingHandSize; i++)
            {
                if (GameActionQueueRunner.Instance != null)
                {
                    var queue = GameActionQueueRunner.Instance.Queue;
                    while (queue.IsProcessing)
                    {
                        yield return null;
                    }
                }
                playerController.DrawCard();
                yield return null;
            }
        }

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
            playerController.DrawCard();
        }

        public void HandleAIPhase(Enums.TurnPhase phase, GameManager opponent)
        {
            if (!isAI || playerController == null || aiLogic == null)
                return;
            PlayerController opponentController = opponent != null ? opponent.playerController : null;
            Debug.Log($"[AI][{playerLabel}] Handling phase {phase}.");
            aiLogic.HandlePhase(phase, playerController, opponentController);
        }
    }
}
