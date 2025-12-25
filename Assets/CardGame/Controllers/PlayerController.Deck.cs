using System;
using System.Linq;
using Models.Cards;
using Models.Deck;
using Models.Player;
using Enums;

namespace Controllers
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
            CardDrawn?.Invoke(card);
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
            var beforeShuffle = string.Join(", ", deckController.Cards.Select(c => c != null ? c.CardName : "null"));

            deckController.Shuffle();

            var afterShuffle = string.Join(", ", deckController.Cards.Select(c => c != null ? c.CardName : "null"));
        }
    }
}
