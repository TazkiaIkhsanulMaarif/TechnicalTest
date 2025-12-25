using System;
using Controllers;
using Enums;
using Interfaces;
using Models.Cards;
using Models.Player;

namespace AI
{
    [Serializable]
    public sealed partial class SimplePhaseAI : IAIPlayer
    {
        public void HandlePhase(TurnPhase phase, PlayerController self, PlayerController opponent)
        {
            if (self == null)
                return;

            string label = self.DebugLabel;

            var aiPlayer = self.Player as AIPlayer;
            var difficulty = aiPlayer != null ? aiPlayer.Difficulty : AIDifficulty.Medium;

            switch (phase)
            {
                case TurnPhase.Main:
                    switch (difficulty)
                    {
                        case AIDifficulty.Easy:
                            PerformMainPhaseActionsEasy(self, opponent, label);
                            break;
                        case AIDifficulty.Medium:
                            PerformMainPhaseActions(self, opponent, label);
                            break;
                        case AIDifficulty.Hard:
                            PerformMainPhaseActionsHard(self, opponent, label);
                            break;
                        default:
                            PerformMainPhaseActions(self, opponent, label);
                            break;
                    }
                    break;
                case TurnPhase.Battle:
                    switch (difficulty)
                    {
                        case AIDifficulty.Easy:
                            PerformBattlePhaseActionsEasy(self, opponent, label);
                            break;
                        case AIDifficulty.Medium:
                            PerformBattlePhaseActions(self, opponent, label);
                            break;
                        case AIDifficulty.Hard:
                            PerformBattlePhaseActionsHard(self, opponent, label);
                            break;
                        default:
                            PerformBattlePhaseActions(self, opponent, label);
                            break;
                    }
                    break;
            }
        }

        partial void PerformMainPhaseActions(PlayerController controller, PlayerController opponent, string label);
        partial void PerformBattlePhaseActions(PlayerController selfController, PlayerController opponentController, string label);
        partial void PerformMainPhaseActionsEasy(PlayerController controller, PlayerController opponent, string label);
        partial void PerformBattlePhaseActionsEasy(PlayerController selfController, PlayerController opponentController, string label);
        partial void PerformMainPhaseActionsHard(PlayerController controller, PlayerController opponent, string label);
        partial void PerformBattlePhaseActionsHard(PlayerController selfController, PlayerController opponentController, string label);
    }
}
