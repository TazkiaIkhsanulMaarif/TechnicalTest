using System.Linq;
using Models.Deck;
using Testing;
using UnityEngine;

namespace Debugging
{
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

            var deckModel = new DeckModel(testDeckData.cards);

            Debug.Log($"[DeckDebugLogger] Created deck with {deckModel.Cards.Count} cards.");
            Debug.Log($"[DeckDebugLogger] Composition -> Monster={deckModel.Cards.Count(c => c.CardType == Enums.CardType.Monster)}, " +
                      $"Spell={deckModel.Cards.Count(c => c.CardType == Enums.CardType.Spell)}, " +
                      $"Trap={deckModel.Cards.Count(c => c.CardType == Enums.CardType.Trap)}");

            var initialOrder = string.Join(", ", deckModel.Cards.Select(c => c != null ? c.CardName : "null"));
            Debug.Log($"[DeckDebugLogger] Initial order: {initialOrder}");

            var deckController = new Controllers.DeckController(deckModel);

            var beforeShuffle = string.Join(", ", deckController.Cards.Select(c => c != null ? c.CardName : "null"));
            deckController.Shuffle();
            var afterShuffle = string.Join(", ", deckController.Cards.Select(c => c != null ? c.CardName : "null"));

            Debug.Log($"[DeckDebugLogger] Before shuffle: {beforeShuffle}");
            Debug.Log($"[DeckDebugLogger] After shuffle:  {afterShuffle}");

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
