using System;
using Models.Cards;
using Models.Player;
using Enums;

namespace Controllers
{
    public sealed partial class PlayerController
    {
        public void AttackWithMonster(int attackerSlot, int targetSlot, PlayerController opponent)
        {
            if (opponent == null) throw new ArgumentNullException(nameof(opponent));
            PlayerModel selfModel = player.Model;
            PlayerModel opponentModel = opponent.player.Model;

            if (attackerSlot < 0 || attackerSlot >= selfModel.MonsterField.Count)
                throw new ArgumentOutOfRangeException(nameof(attackerSlot));

            if (selfModel.HasMonsterAttackedThisTurn(attackerSlot))
            {
                LogAction("BATTLE", $"Monster at slot {attackerSlot} already attacked this turn.");
                return;
            }

            bool opponentHasMonster = HasAnyMonsterOnField(opponentModel);

            MonsterCard attacker = GetMonsterAtSlot(selfModel, attackerSlot);
            MonsterCard defender = GetMonsterAtSlot(opponentModel, targetSlot, allowNull: true);

            if (attacker == null)
                throw new InvalidOperationException("No attacker monster in the specified slot.");

            if (opponentHasMonster && defender == null)
                throw new InvalidOperationException("Opponent has monsters; you must select a monster as attack target.");

            int selfLPBefore = selfModel.LifePoint;
            int opponentLPBefore = opponentModel.LifePoint;

            TriggerTrapsOnAttack(opponent);

            if (!opponentHasMonster)
            {
                ResolveDirectAttack(attackerSlot, opponent, selfModel, opponentModel, opponentLPBefore);
                return;
            }

            ResolveMonsterBattle(attackerSlot, targetSlot, opponent, selfModel, opponentModel, selfLPBefore, opponentLPBefore);
        }

        private void TriggerTrapsOnAttack(PlayerController defender)
        {
            if (defender == null)
                return;

            PlayerModel defenderModel = defender.player.Model;
            var spellField = defenderModel.SpellTrapField;

            for (int i = 0; i < spellField.Count; i++)
            {
                if (spellField[i] is TrapCard trap && trap.TriggerType == TrapTriggerType.OnAttack)
                {
                    trap.OnActivate();

                    // Apply simple reactive trap effects; defensive/"reactive" AI philosophy
                    ApplyTrapEffectOnAttack(trap, this, defender);

                    defender.RemoveSpellTrapFromField(defenderModel, i);
                }
            }
        }

        private void ApplyTrapEffectOnAttack(TrapCard trap, PlayerController attacker, PlayerController defender)
        {
            int value = trap.EffectValue;

            switch (trap.EffectType)
            {
                case TrapEffectType.DamageAttacker:
                    {
                        int damage = value > 0 ? value : 100;
                        int before = attacker.player.Model.LifePoint;
                        attacker.TakeDamage(damage);
                        int after = attacker.player.Model.LifePoint;
                        LogAction("TRAP", $"Trap '{trap.CardName}' deals {damage} damage to attacker {attacker.debugLabel} | LP {before}->{after}.");
                    }
                    break;

                case TrapEffectType.ReduceIncomingDamage:
                    defender.player.Model.EnableBattleDamageImmunity();
                    LogAction("TRAP", $"Trap '{trap.CardName}' prevents the next battle damage to defender {defender.debugLabel}.");
                    break;

                case TrapEffectType.DebuffSummonedMonster:
                    ApplyAttackDebuffToCurrentAttacker(attacker, value);
                    LogAction("TRAP", $"Trap '{trap.CardName}' debuffs attacking monster ATK by {value}% on {attacker.debugLabel}.");
                    break;

                case TrapEffectType.NegateSpell:
                    LogAction("TRAP", $"Trap '{trap.CardName}' is configured to negate spells but requires a spell-activation hook.");
                    break;

                case TrapEffectType.DamagePlayer:
                    {
                        int damage = value > 0 ? value : 80;
                        int before = defender.player.Model.LifePoint;
                        defender.TakeDamage(damage);
                        int after = defender.player.Model.LifePoint;
                        LogAction("TRAP", $"Trap '{trap.CardName}' deals {damage} damage to defending player {defender.debugLabel} | LP {before}->{after}.");
                    }
                    break;
            }
        }

        private void ApplyAttackDebuffToCurrentAttacker(PlayerController attackerController, int percent)
        {
            PlayerModel model = attackerController.player.Model;

            for (int i = 0; i < model.MonsterField.Count; i++)
            {
                if (model.MonsterField[i] is MonsterCard)
                {
                    model.AddAttackModifierPercent(i, -Math.Abs(percent));
                    break;
                }
            }
        }

        private void ResolveDirectAttack(int attackerSlot, PlayerController opponent, PlayerModel selfModel, PlayerModel opponentModel, int opponentLPBefore)
        {
            int attackValue = selfModel.GetEffectiveAttackAt(attackerSlot);
            int damage = attackValue;
            ApplyBattleDamage(opponent, damage);
            int opponentLPAfter = opponentModel.LifePoint;

            MonsterCard attacker = GetMonsterAtSlot(selfModel, attackerSlot);
            LogAction("BATTLE", $"Direct attack with '{attacker.CardName}' ATK {attackValue} | Opponent LP {opponentLPBefore}->{opponentLPAfter} (damage {damage})");

            selfModel.MarkMonsterAttackedThisTurn(attackerSlot);
        }

        private void ResolveMonsterBattle(int attackerSlot, int targetSlot,
            PlayerController opponent, PlayerModel selfModel, PlayerModel opponentModel, int selfLPBefore, int opponentLPBefore)
        {
            int attackValue = selfModel.GetEffectiveAttackAt(attackerSlot);
            int defenseValue = opponentModel.GetEffectiveDefenseAt(targetSlot);

            MonsterCard attacker = GetMonsterAtSlot(selfModel, attackerSlot);
            MonsterCard defender = GetMonsterAtSlot(opponentModel, targetSlot);

            if (attackValue > defenseValue)
            {
                RemoveMonsterFromField(opponentModel, targetSlot);
                LogAction("BATTLE", $"'{attacker.CardName}' ATK {attackValue} vs '{defender.CardName}' DEF {defenseValue} | Defender destroyed | No LP damage");
            }
            else if (attackValue < defenseValue)
            {
                RemoveMonsterFromField(selfModel, attackerSlot);
                LogAction("BATTLE", $"'{attacker.CardName}' ATK {attackValue} vs '{defender.CardName}' DEF {defenseValue} | Attacker destroyed | No LP damage");
                opponentModel.ApplyDefenseDamage(targetSlot, attackValue);
                int newDefense = opponentModel.GetEffectiveDefenseAt(targetSlot);
                LogAction("BATTLE", $"Defender '{defender.CardName}' DEF reduced by {attackValue} from this attack | New effective DEF = {newDefense}.");
            }
            else
            {
                RemoveMonsterFromField(selfModel, attackerSlot);
                RemoveMonsterFromField(opponentModel, targetSlot);

                LogAction("BATTLE", $"'{attacker.CardName}' ATK {attackValue} vs '{defender.CardName}' DEF {defenseValue} | Both destroyed | No LP damage");
            }

            selfModel.MarkMonsterAttackedThisTurn(attackerSlot);
        }

        private void ApplyBattleDamage(PlayerController target, int damage)
        {
            if (damage <= 0)
                return;

            PlayerModel model = target.player.Model;

            if (model.ConsumeBattleDamageImmunity())
            {
                LogAction("TRAP", $"Battle damage {damage} to '{target.debugLabel}' prevented by trap.");
                return;
            }

            int before = model.LifePoint;
            target.TakeDamage(damage);
            int after = model.LifePoint;
            LogAction("BATTLE", $"Battle damage {damage} applied to '{target.debugLabel}' | LP {before}->{after}.");
        }
    }
}
