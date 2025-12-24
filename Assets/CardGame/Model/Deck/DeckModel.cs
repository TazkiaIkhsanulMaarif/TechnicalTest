using System;
using System.Collections.Generic;
using System.Linq;
using CardGame.Enums;
using CardGame.Models.Cards;

namespace CardGame.Models.Deck
{
    [Serializable]
    public sealed class DeckModel
    {
        private const int TOTAL_CARDS = 20;
        private const int MONSTER_COUNT = 10;
        private const int SPELL_COUNT = 5;
        private const int TRAP_COUNT = 5;

        private readonly List<CardBase> cards = new();

        public IReadOnlyList<CardBase> Cards => cards;
        public int Count => cards.Count;

        public DeckModel(IEnumerable<CardBase> initialCards)
        {
            ValidateDeck(initialCards);
            cards.AddRange(initialCards);
        }

        private void ValidateDeck(IEnumerable<CardBase> deckCards)
        {
            var list = deckCards.ToList();

            if (list.Count != TOTAL_CARDS)
                throw new Exception($"Deck must contain exactly {TOTAL_CARDS} cards.");

            int monster = list.Count(c => c.CardType == CardType.Monster);
            int spell = list.Count(c => c.CardType == CardType.Spell);
            int trap = list.Count(c => c.CardType == CardType.Trap);

            if (monster != MONSTER_COUNT ||
                spell != SPELL_COUNT ||
                trap != TRAP_COUNT)
            {
                throw new Exception(
                    $"Invalid deck composition: " +
                    $"Monster={monster}, Spell={spell}, Trap={trap}");
            }
        }
    }
}
