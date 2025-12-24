using System;
using CardGame.Models.Cards;
using CardGame.Models.Deck;
using CardGame.Models.Player;

namespace CardGame.Controllers
{
    public sealed class PlayerController
    {
        private readonly PlayerBase player;
        private readonly DeckController deckController;

        public PlayerBase Player => player;

        public event Action HandChanged;
        public event Action<int> LifePointChanged;
        public event Action<CardBase, int, bool> CardPlaced;
        public event Action<int, bool, CardBase> CardRemovedFromField;
        public event Action PlayerDied;

        public PlayerController(PlayerBase player)
        {
            this.player = player ?? throw new ArgumentNullException(nameof(player));

            DeckModel deckModel = player.Model.Deck;
            if (deckModel == null)
                throw new ArgumentException("Player model must have a deck.", nameof(player));

            deckController = new DeckController(deckModel);
        }

        public CardBase DrawCard()
        {
            if (deckController.Count <= 0)
            {
                PlayerDied?.Invoke();
                return null;
            }

            CardBase card = deckController.Draw();
            player.Model.AddToHand(card);

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
            deckController.Shuffle();
        }

        public void PlayCard(CardBase card, int slotIndex)
        {
            if (card == null) throw new ArgumentNullException(nameof(card));

            if (!player.Model.RemoveFromHand(card))
                throw new InvalidOperationException("Card is not in hand.");

            bool isMonster = card is MonsterCard;

            if (isMonster)
            {
                ValidateEmptySlot(player.Model.MonsterField, slotIndex);
                player.Model.PlaceMonster(card, slotIndex);
            }
            else
            {
                ValidateEmptySlot(player.Model.SpellTrapField, slotIndex);
                player.Model.PlaceSpellTrap(card, slotIndex);
            }

            HandChanged?.Invoke();
            CardPlaced?.Invoke(card, slotIndex, isMonster);
        }

        public void DiscardCard(CardBase card)
        {
            if (card == null) throw new ArgumentNullException(nameof(card));

            if (!player.Model.RemoveFromHand(card))
                throw new InvalidOperationException("Card is not in hand.");

            player.Model.AddToGraveyard(card);
            HandChanged?.Invoke();
        }

        public void TakeDamage(int damage)
        {
            if (damage < 0) throw new ArgumentOutOfRangeException(nameof(damage));

            int before = player.Model.LifePoint;
            player.Model.TakeDamage(damage);
            int after = player.Model.LifePoint;

            if (after != before)
            {
                player.NotifyDamageTaken(damage);
                LifePointChanged?.Invoke(after);
            }

            if (player.Model.IsDead)
            {
                PlayerDied?.Invoke();
            }
        }

        public void AttackWithMonster(int attackerSlot, int targetSlot, PlayerController opponent)
        {
            if (opponent == null) throw new ArgumentNullException(nameof(opponent));

            MonsterCard attacker = GetMonsterAtSlot(player.Model, attackerSlot);
            MonsterCard defender = GetMonsterAtSlot(opponent.player.Model, targetSlot, allowNull: true);

            if (attacker == null)
                throw new InvalidOperationException("No attacker monster in the specified slot.");

            if (defender == null)
            {
                opponent.TakeDamage(attacker.Attack);
                return;
            }

            int attackValue = attacker.Attack;
            int defenseValue = defender.Defense;

            if (attackValue > defenseValue)
            {
                int damage = attackValue - defenseValue;
                RemoveMonsterFromField(opponent.player.Model, targetSlot);
                opponent.TakeDamage(damage);
            }
            else if (attackValue < defenseValue)
            {
                int damage = defenseValue - attackValue;
                RemoveMonsterFromField(player.Model, attackerSlot);
                TakeDamage(damage);
            }
            else
            {
                RemoveMonsterFromField(player.Model, attackerSlot);
                RemoveMonsterFromField(opponent.player.Model, targetSlot);
            }
        }

        private static MonsterCard GetMonsterAtSlot(PlayerModel model, int slotIndex, bool allowNull = false)
        {
            if (slotIndex < 0 || slotIndex >= model.MonsterField.Count)
                throw new ArgumentOutOfRangeException(nameof(slotIndex));

            CardBase card = model.MonsterField[slotIndex];

            if (card == null && allowNull)
                return null;

            if (card is not MonsterCard monster)
                throw new InvalidOperationException("Card in slot is not a monster.");

            return monster;
        }

        private void RemoveMonsterFromField(PlayerModel model, int slotIndex)
        {
            CardBase removed = model.RemoveFromField(slotIndex, isMonster: true);
            if (removed != null)
            {
                model.AddToGraveyard(removed);
                CardRemovedFromField?.Invoke(slotIndex, true, removed);
            }
        }

        private static void ValidateEmptySlot(System.Collections.Generic.IReadOnlyList<CardBase> field, int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= field.Count)
                throw new ArgumentOutOfRangeException(nameof(slotIndex));

            if (field[slotIndex] != null)
                throw new InvalidOperationException("Field slot is already occupied.");
        }
    }
}
