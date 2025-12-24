using System;
using System.Linq;
using CardGame.Models.Cards;
using CardGame.Models.Deck;
using CardGame.Models.Player;
using CardGame.Enums;

namespace CardGame.Controllers
{
    public sealed partial class PlayerController
    {
        public CardBase DrawCard()
        {
            if (deckController.Count <= 0)
            {
                PlayerDied?.Invoke();
                return null;
            }

            int deckBefore = deckController.Count;
            int handBefore = player.Model.Hand.Count;

            CardBase card = deckController.Draw();
            player.Model.AddToHand(card);

            int deckAfter = deckController.Count;
            int handAfter = player.Model.Hand.Count;

            LogAction("DRAW", $"Drew '{card?.CardName}' | Hand {handBefore}->{handAfter} | Deck {deckBefore}->{deckAfter}");

            player.NotifyCardDrawn(card);
            HandChanged?.Invoke();

            return card;
        }

        public void DrawStartingHand(int cardCount = 4)
        {
            for (int i = 0; i < cardCount; i++)
            {
                DrawCard();
            }
        }

        public void ShuffleDeck()
        {
            UnityEngine.Debug.Log($"[PlayerController][{debugLabel}] Shuffling deck...");
            var beforeShuffle = string.Join(", ", deckController.Cards.Select(c => c != null ? c.CardName : "null"));

            deckController.Shuffle();

            var afterShuffle = string.Join(", ", deckController.Cards.Select(c => c != null ? c.CardName : "null"));
            UnityEngine.Debug.Log($"[PlayerController][{debugLabel}] Before shuffle: {beforeShuffle}");
            UnityEngine.Debug.Log($"[PlayerController][{debugLabel}] After shuffle:  {afterShuffle}");
        }
    }
}
