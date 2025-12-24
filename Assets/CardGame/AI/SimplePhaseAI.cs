using System;
using CardGame.Controllers;
using CardGame.Enums;
using CardGame.Interfaces;
using CardGame.Models.Cards;
using CardGame.Models.Player;

namespace CardGame.AI
{
    [Serializable]
    public sealed class SimplePhaseAI : IAIPlayer
    {
        public void HandlePhase(TurnPhase phase, PlayerController self, PlayerController opponent)
        {
            if (self == null)
                return;

            switch (phase)
            {
                case TurnPhase.Main:
                    PerformMainPhaseActions(self);
                    break;
                case TurnPhase.Battle:
                    PerformBattlePhaseActions(self, opponent);
                    break;
                    // Draw and End: no active decision for this simple AI
            }
        }

        private static void PerformMainPhaseActions(PlayerController controller)
        {
            PlayerModel model = controller.Player.Model;

            // 1) Summon monster with highest ATK to first empty monster slot
            MonsterCard bestMonster = null;

            foreach (CardBase card in model.Hand)
            {
                if (card is MonsterCard monster)
                {
                    if (bestMonster == null || monster.Attack > bestMonster.Attack)
                    {
                        bestMonster = monster;
                    }
                }
            }

            if (bestMonster != null)
            {
                int emptyMonsterSlot = -1;
                var monsterField = model.MonsterField;
                for (int i = 0; i < monsterField.Count; i++)
                {
                    if (monsterField[i] == null)
                    {
                        emptyMonsterSlot = i;
                        break;
                    }
                }

                if (emptyMonsterSlot != -1)
                {
                    controller.PlayCard(bestMonster, emptyMonsterSlot);
                }
            }

            // 2) Use first spell card (buff) to first empty spell/trap slot
            CardBase spellCard = null;
            foreach (CardBase card in model.Hand)
            {
                if (card.CardType == CardType.Spell)
                {
                    spellCard = card;
                    break;
                }
            }

            if (spellCard != null)
            {
                int emptySpellSlot = -1;
                var spellField = model.SpellTrapField;
                for (int i = 0; i < spellField.Count; i++)
                {
                    if (spellField[i] == null)
                    {
                        emptySpellSlot = i;
                        break;
                    }
                }

                if (emptySpellSlot != -1)
                {
                    controller.PlayCard(spellCard, emptySpellSlot);
                }
            }
        }

        private static void PerformBattlePhaseActions(PlayerController selfController, PlayerController opponentController)
        {
            if (opponentController == null)
                return;

            PlayerModel selfModel = selfController.Player.Model;
            PlayerModel opponentModel = opponentController.Player.Model;

            // Select our attacker: monster with highest ATK
            int attackerSlot = -1;
            MonsterCard attackerCard = null;

            for (int i = 0; i < selfModel.MonsterField.Count; i++)
            {
                if (selfModel.MonsterField[i] is MonsterCard monster)
                {
                    if (attackerCard == null || monster.Attack > attackerCard.Attack)
                    {
                        attackerCard = monster;
                        attackerSlot = i;
                    }
                }
            }

            if (attackerSlot == -1)
                return; // no monsters to attack with

            // Find weakest enemy monster by ATK
            int targetSlot = -1;
            MonsterCard weakestDefender = null;

            for (int i = 0; i < opponentModel.MonsterField.Count; i++)
            {
                if (opponentModel.MonsterField[i] is MonsterCard defender)
                {
                    if (weakestDefender == null || defender.Attack < weakestDefender.Attack)
                    {
                        weakestDefender = defender;
                        targetSlot = i;
                    }
                }
            }

            if (weakestDefender == null)
            {
                // No enemy monsters: direct attack to first slot index (0)
                targetSlot = 0;
            }

            selfController.AttackWithMonster(attackerSlot, targetSlot, opponentController);
        }
    }
}
