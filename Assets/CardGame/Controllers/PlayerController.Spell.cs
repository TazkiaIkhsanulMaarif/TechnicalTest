using System;
using CardGame.Models.Cards;
using CardGame.Models.Player;
using CardGame.Enums;

namespace CardGame.Controllers
{
    public sealed partial class PlayerController
    {
        // SPELL EFFECT HELPERS
        public void ApplySpellToMonster(SpellCard spell, PlayerController targetPlayer, int targetSlotIndex)
        {
            if (spell == null) throw new ArgumentNullException(nameof(spell));
            if (targetPlayer == null) throw new ArgumentNullException(nameof(targetPlayer));

            PlayerModel targetModel = targetPlayer.player.Model;
            string targetLabel = targetPlayer.debugLabel;

            if (targetSlotIndex < 0 || targetSlotIndex >= targetModel.MonsterField.Count)
                throw new ArgumentOutOfRangeException(nameof(targetSlotIndex));

            if (targetModel.MonsterField[targetSlotIndex] is not MonsterCard targetMonster)
                throw new InvalidOperationException("Target slot does not contain a monster.");

            int value = spell.EffectValue;

            switch (spell.EffectType)
            {
                case SpellEffectType.DestroyWeakMonster:
                    if (targetMonster.Attack < value)
                    {
                        RemoveMonsterFromField(targetModel, targetSlotIndex);
                        LogAction("SPELL", $"[To {targetLabel}] Spell '{spell.CardName}' destroys weak monster '{targetMonster.CardName}' (ATK {targetMonster.Attack} < {value}).");
                    }
                    else
                    {
                        LogAction("SPELL", $"[To {targetLabel}] Spell '{spell.CardName}' had no effect on '{targetMonster.CardName}' (ATK {targetMonster.Attack} >= {value}).");
                    }
                    break;

                case SpellEffectType.ReduceTargetAttack:
                    targetModel.AddAttackModifierPercent(targetSlotIndex, -Math.Abs(value));
                    LogAction("SPELL", $"[To {targetLabel}] Spell '{spell.CardName}' reduces ATK of '{targetMonster.CardName}' by {value}%.");
                    break;

                case SpellEffectType.BoostAllyAttack:
                    targetModel.AddAttackModifierPercent(targetSlotIndex, Math.Abs(value));
                    LogAction("SPELL", $"[To {targetLabel}] Spell '{spell.CardName}' boosts ATK of '{targetMonster.CardName}' by {value}%.");
                    break;

                case SpellEffectType.ReduceTargetDefense:
                    targetModel.AddDefenseModifierPercent(targetSlotIndex, -Math.Abs(value));
                    LogAction("SPELL", $"[To {targetLabel}] Spell '{spell.CardName}' reduces DEF of '{targetMonster.CardName}' by {value}%.");
                    break;

                case SpellEffectType.BoostAllyDefense:
                    targetModel.AddDefenseModifierPercent(targetSlotIndex, Math.Abs(value));
                    LogAction("SPELL", $"[To {targetLabel}] Spell '{spell.CardName}' boosts DEF of '{targetMonster.CardName}' by {value}%.");
                    break;
            }
        }
    }
}
