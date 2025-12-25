using System;
using System.Collections.Generic;
using Interfaces;
using Models.Cards;
using Models.Deck;

namespace Controllers
{
    public sealed class DeckController : IDeck
    {
        private readonly List<CardBase> runtimeDeck;

        public event Action Shuffled;
        public event Action<CardBase> CardDrawn;

        public IReadOnlyList<CardBase> Cards => runtimeDeck.AsReadOnly();
        public int Count => runtimeDeck.Count;

        public DeckController(DeckModel model)
        {
            runtimeDeck = new List<CardBase>(model.Cards);
        }

        public void Shuffle()
        {
            var rng = new Random();

            for (int i = runtimeDeck.Count - 1; i > 0; i--)
            {
                int j = rng.Next(i + 1);
                (runtimeDeck[i], runtimeDeck[j]) =
                    (runtimeDeck[j], runtimeDeck[i]);
            }

            Shuffled?.Invoke();
        }

        public CardBase Draw()
        {
            if (runtimeDeck.Count == 0)
                throw new Exception("Deck is empty.");

            CardBase card = runtimeDeck[0];
            runtimeDeck.RemoveAt(0);

            CardDrawn?.Invoke(card);
            return card;
        }
    }
}
