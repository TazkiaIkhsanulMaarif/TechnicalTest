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

        public bool SummonMonster(MonsterCard monster)
        {
            if (monster == null)
                throw new ArgumentNullException(nameof(monster));

            var model = player.Model;

            int handBefore = model.Hand.Count;
            int monstersOnFieldBefore = model.MonsterField.Count(c => c != null);

            if (model.HasSummonedThisTurn)
            {
                LogAction("SUMMON", $"Failed to summon '{monster.CardName}': already summoned this turn.");
                return false;
            }

            if (monster.Level > 4)
            {
                LogAction("SUMMON", $"Failed to summon '{monster.CardName}': level {monster.Level} > 4.");
                return false;
            }

            if (!IsMonsterInHand(model, monster))
            {
                LogAction("SUMMON", $"Failed to summon '{monster.CardName}': monster not in hand.");
                return false;
            }

            var monsterField = model.MonsterField;
            if (!TryFindEmptyMonsterSlot(monsterField, out int emptyMonsterSlot))
            {
                LogAction("SUMMON", $"Failed to summon '{monster.CardName}': no empty monster slot (max 3).");
                return false;
            }

            PlayCard(monster, emptyMonsterSlot);

            model.MarkSummonedThisTurn();

            int handAfter = model.Hand.Count;
            int monstersOnFieldAfter = model.MonsterField.Count(c => c != null);

            LogAction("SUMMON", $"Summoned '{monster.CardName}' (Level {monster.Level}) to slot {emptyMonsterSlot} | Hand {handBefore}->{handAfter} | MonstersOnField {monstersOnFieldBefore}->{monstersOnFieldAfter}");

            return true;
        }

        public void DiscardCard(CardBase card)
        {
            if (card == null) throw new ArgumentNullException(nameof(card));

            if (!player.Model.RemoveFromHand(card))
                throw new InvalidOperationException("Card is not in hand.");

            player.Model.AddToGraveyard(card);
            HandChanged?.Invoke();
        }
    }
}
