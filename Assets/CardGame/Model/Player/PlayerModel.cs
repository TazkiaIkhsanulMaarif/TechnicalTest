using System;
using System.Collections.Generic;
using CardGame.Models.Cards;
using CardGame.Models.Deck;

namespace CardGame.Models.Player
{
    [Serializable]
    public sealed class PlayerModel
    {
        private const int DefaultLifePoints = 8000;
        private const int FieldSize = 3;

        private readonly List<CardBase> hand = new();
        private readonly List<CardBase> graveyard = new();
        private readonly CardBase[] monsterField = new CardBase[FieldSize];
        private readonly CardBase[] spellTrapField = new CardBase[FieldSize];

        public DeckModel Deck { get; }
        public IReadOnlyList<CardBase> Hand => hand;
        public IReadOnlyList<CardBase> Graveyard => graveyard;
        public IReadOnlyList<CardBase> MonsterField => monsterField;
        public IReadOnlyList<CardBase> SpellTrapField => spellTrapField;

        public int LifePoint { get; private set; }
        public bool IsDead => LifePoint <= 0;

        public PlayerModel(DeckModel deck, int initialLifePoints = DefaultLifePoints)
        {
            Deck = deck ?? throw new ArgumentNullException(nameof(deck));

            if (initialLifePoints <= 0)
                throw new ArgumentOutOfRangeException(nameof(initialLifePoints), "Life points must be positive.");

            LifePoint = initialLifePoints;
        }

        public void AddToHand(CardBase card)
        {
            if (card == null) throw new ArgumentNullException(nameof(card));
            hand.Add(card);
        }

        public bool RemoveFromHand(CardBase card)
        {
            if (card == null) throw new ArgumentNullException(nameof(card));
            return hand.Remove(card);
        }

        public void AddToGraveyard(CardBase card)
        {
            if (card == null) throw new ArgumentNullException(nameof(card));
            graveyard.Add(card);
        }

        public void PlaceMonster(CardBase card, int slotIndex)
        {
            if (card == null) throw new ArgumentNullException(nameof(card));
            ValidateSlotIndex(slotIndex);

            monsterField[slotIndex] = card;
        }

        public void PlaceSpellTrap(CardBase card, int slotIndex)
        {
            if (card == null) throw new ArgumentNullException(nameof(card));
            ValidateSlotIndex(slotIndex);

            spellTrapField[slotIndex] = card;
        }

        public CardBase RemoveFromField(int slotIndex, bool isMonster)
        {
            ValidateSlotIndex(slotIndex);

            if (isMonster)
            {
                var card = monsterField[slotIndex];
                monsterField[slotIndex] = null;
                return card;
            }
            else
            {
                var card = spellTrapField[slotIndex];
                spellTrapField[slotIndex] = null;
                return card;
            }
        }

        public void TakeDamage(int amount)
        {
            if (amount < 0) throw new ArgumentOutOfRangeException(nameof(amount), "Damage cannot be negative.");
            LifePoint = Math.Max(0, LifePoint - amount);
        }

        public void Heal(int amount)
        {
            if (amount < 0) throw new ArgumentOutOfRangeException(nameof(amount), "Heal amount cannot be negative.");
            LifePoint += amount;
        }

        private static void ValidateSlotIndex(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= FieldSize)
                throw new ArgumentOutOfRangeException(nameof(slotIndex), $"Slot index must be between 0 and {FieldSize - 1}.");
        }
    }
}
