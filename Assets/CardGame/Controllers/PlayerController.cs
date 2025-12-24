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
        private readonly PlayerBase player;
        private readonly DeckController deckController;
        private readonly string debugLabel;

        public PlayerBase Player => player;
        public string DebugLabel => debugLabel;

        public event Action HandChanged;
        public event Action<int> LifePointChanged;
        public event Action<CardBase, int, bool> CardPlaced;
        public event Action<int, bool, CardBase> CardRemovedFromField;
        public event Action PlayerDied;

        public PlayerController(PlayerBase player, string debugLabel)
        {
            this.player = player ?? throw new ArgumentNullException(nameof(player));
            this.debugLabel = string.IsNullOrEmpty(debugLabel) ? "Player" : debugLabel;

            DeckModel deckModel = player.Model.Deck;
            if (deckModel == null)
                throw new ArgumentException("Player model must have a deck.", nameof(player));

            deckController = new DeckController(deckModel);
            UnityEngine.Debug.Log($"[PlayerController][{this.debugLabel}] Initialized with deck size={deckController.Count}.");
        }
        public void ResetTurnState()
        {
            player.Model.ResetTurnState();
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
                LogAction("BATTLE", $"Monster '{removed.CardName}' destroyed and sent to graveyard (slot {slotIndex}).");
            }
        }

        private void RemoveSpellTrapFromField(PlayerModel model, int slotIndex)
        {
            CardBase removed = model.RemoveFromField(slotIndex, isMonster: false);
            if (removed != null)
            {
                model.AddToGraveyard(removed);
                CardRemovedFromField?.Invoke(slotIndex, false, removed);
            }
        }


        private void LogAction(string action, string message)
        {
            // Example: [ACTION][Player 1.SUMMON] ... or [ACTION][Player 2.ATTACK] ...
            UnityEngine.Debug.Log($"[ACTION][{debugLabel}.{action}] {message}");
        }

        private static void ValidateEmptySlot(System.Collections.Generic.IReadOnlyList<CardBase> field, int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= field.Count)
                throw new ArgumentOutOfRangeException(nameof(slotIndex));

            if (field[slotIndex] != null)
                throw new InvalidOperationException("Field slot is already occupied.");
        }

        private static bool IsMonsterInHand(PlayerModel model, MonsterCard monster)
        {
            return model.Hand.Contains(monster);
        }

        private static bool TryFindEmptyMonsterSlot(System.Collections.Generic.IReadOnlyList<CardBase> field, out int slotIndex)
        {
            for (int i = 0; i < field.Count; i++)
            {
                if (field[i] == null)
                {
                    slotIndex = i;
                    return true;
                }
            }

            slotIndex = -1;
            return false;
        }

        private static bool HasAnyMonsterOnField(PlayerModel model)
        {
            for (int i = 0; i < model.MonsterField.Count; i++)
            {
                if (model.MonsterField[i] is MonsterCard)
                {
                    return true;
                }
            }

            return false;
        }

    }
}
