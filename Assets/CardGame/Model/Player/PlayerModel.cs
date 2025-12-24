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
        private readonly bool[] monsterHasAttackedThisTurn = new bool[FieldSize];
        private readonly int[] attackModifierPercent = new int[FieldSize];
        private readonly int[] defenseModifierPercent = new int[FieldSize];
        private readonly int[] defenseDamage = new int[FieldSize];

        public DeckModel Deck { get; }
        public IReadOnlyList<CardBase> Hand => hand;
        public IReadOnlyList<CardBase> Graveyard => graveyard;
        public IReadOnlyList<CardBase> MonsterField => monsterField;
        public IReadOnlyList<CardBase> SpellTrapField => spellTrapField;

        public int LifePoint { get; private set; }
        public bool IsDead => LifePoint <= 0;
        public bool HasSummonedThisTurn { get; private set; }
        public bool PreventNextBattleDamage { get; private set; }

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
            attackModifierPercent[slotIndex] = 0;
            defenseModifierPercent[slotIndex] = 0;
            defenseDamage[slotIndex] = 0;
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
                attackModifierPercent[slotIndex] = 0;
                defenseModifierPercent[slotIndex] = 0;
                defenseDamage[slotIndex] = 0;
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

        public void MarkSummonedThisTurn()
        {
            HasSummonedThisTurn = true;
        }

        public void ResetTurnState()
        {
            HasSummonedThisTurn = false;

            for (int i = 0; i < FieldSize; i++)
            {
                monsterHasAttackedThisTurn[i] = false;
            }

            PreventNextBattleDamage = false;
        }

        public bool HasMonsterAttackedThisTurn(int slotIndex)
        {
            ValidateSlotIndex(slotIndex);
            return monsterHasAttackedThisTurn[slotIndex];
        }

        public void MarkMonsterAttackedThisTurn(int slotIndex)
        {
            ValidateSlotIndex(slotIndex);
            monsterHasAttackedThisTurn[slotIndex] = true;
        }

        public int GetEffectiveAttackAt(int slotIndex)
        {
            ValidateSlotIndex(slotIndex);

            if (monsterField[slotIndex] is not MonsterCard monster)
                throw new InvalidOperationException("No monster in the specified slot.");

            int percent = 100 + attackModifierPercent[slotIndex];
            if (percent < 0)
                percent = 0;

            return monster.Attack * percent / 100;
        }

        public int GetEffectiveDefenseAt(int slotIndex)
        {
            ValidateSlotIndex(slotIndex);

            if (monsterField[slotIndex] is not MonsterCard monster)
                throw new InvalidOperationException("No monster in the specified slot.");

            int percent = 100 + defenseModifierPercent[slotIndex];
            if (percent < 0)
                percent = 0;

            int modifiedBase = monster.Defense * percent / 100;
            int remaining = modifiedBase - defenseDamage[slotIndex];
            return remaining > 0 ? remaining : 0;
        }

        public void AddAttackModifierPercent(int slotIndex, int deltaPercent)
        {
            ValidateSlotIndex(slotIndex);
            attackModifierPercent[slotIndex] += deltaPercent;
        }

        public void AddDefenseModifierPercent(int slotIndex, int deltaPercent)
        {
            ValidateSlotIndex(slotIndex);
            defenseModifierPercent[slotIndex] += deltaPercent;
        }

        public void ApplyDefenseDamage(int slotIndex, int amount)
        {
            ValidateSlotIndex(slotIndex);
            if (amount <= 0)
                return;

            defenseDamage[slotIndex] += amount;
        }

        public void EnableBattleDamageImmunity()
        {
            PreventNextBattleDamage = true;
        }

        public bool ConsumeBattleDamageImmunity()
        {
            if (!PreventNextBattleDamage)
                return false;

            PreventNextBattleDamage = false;
            return true;
        }

        private static void ValidateSlotIndex(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= FieldSize)
                throw new ArgumentOutOfRangeException(nameof(slotIndex), $"Slot index must be between 0 and {FieldSize - 1}.");
        }
    }
}
