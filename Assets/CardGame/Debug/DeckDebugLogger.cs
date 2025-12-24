using System.Linq;
using CardGame.Models.Deck;
using CardGame.Testing;
using UnityEngine;

namespace CardGame.Debugging
{
    /// <summary>
    /// Simple helper MonoBehaviour to log deck data and draw behavior.
    /// Attach this to a GameObject and assign a DeckTestData.
    /// This does NOT affect game logic (purely debug / UI side).
    /// </summary>
    public sealed class DeckDebugLogger : MonoBehaviour
    {
        [Header("Source Deck Data")]
        [SerializeField] private DeckTestData testDeckData;

        [Header("Draw Settings")]
        [SerializeField] private int cardsToDraw = 5;

        private void Start()
        {
            if (testDeckData == null || testDeckData.cards == null || testDeckData.cards.Count == 0)
            {
                Debug.LogError("[DeckDebugLogger] DeckTestData is empty or not assigned.");
                return;
            }

            Debug.Log("[DeckDebugLogger] --- Deck Debug Start ---");

            // Build a temporary DeckModel just for inspection
            var deckModel = new DeckModel(testDeckData.cards);

            Debug.Log($"[DeckDebugLogger] Created deck with {deckModel.Cards.Count} cards.");
            Debug.Log($"[DeckDebugLogger] Composition -> Monster={deckModel.Cards.Count(c => c.CardType == CardGame.Enums.CardType.Monster)}, " +
                      $"Spell={deckModel.Cards.Count(c => c.CardType == CardGame.Enums.CardType.Spell)}, " +
                      $"Trap={deckModel.Cards.Count(c => c.CardType == CardGame.Enums.CardType.Trap)}");

            var initialOrder = string.Join(", ", deckModel.Cards.Select(c => c != null ? c.CardName : "null"));
            Debug.Log($"[DeckDebugLogger] Initial order: {initialOrder}");

            // Use a runtime DeckController to test shuffle + draw
            var deckController = new CardGame.Controllers.DeckController(deckModel);

            var beforeShuffle = string.Join(", ", deckController.Cards.Select(c => c != null ? c.CardName : "null"));
            deckController.Shuffle();
            var afterShuffle = string.Join(", ", deckController.Cards.Select(c => c != null ? c.CardName : "null"));

            Debug.Log($"[DeckDebugLogger] Before shuffle: {beforeShuffle}");
            Debug.Log($"[DeckDebugLogger] After shuffle:  {afterShuffle}");

            // Draw some cards and log deck count decreasing
            int drawCount = Mathf.Clamp(cardsToDraw, 0, deckController.Count);
            for (int i = 0; i < drawCount; i++)
            {
                var drawn = deckController.Draw();
                Debug.Log($"[DeckDebugLogger] Draw {i + 1}: '{drawn?.CardName}' -> Deck count now {deckController.Count}.");
            }

            Debug.Log("[DeckDebugLogger] --- Deck Debug End ---");
        }
    }
}
