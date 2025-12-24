using System;
using CardGame.Models.Cards;

namespace CardGame.Models.Player
{
    public sealed class AIPlayer : PlayerBase
    {
        public CardGame.Enums.AIDifficulty Difficulty { get; }

        public AIPlayer(PlayerModel model, CardGame.Enums.AIDifficulty difficulty) : base(model)
        {
            Difficulty = difficulty;
        }

        public override object DecideAction()
        {
            switch (Difficulty)
            {
                case CardGame.Enums.AIDifficulty.Easy:
                    return DecideEasy();
                case CardGame.Enums.AIDifficulty.Medium:
                    return DecideMedium();
                case CardGame.Enums.AIDifficulty.Hard:
                    return DecideHard();
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private object DecideEasy()
        {
            return null;
        }

        private object DecideMedium()
        {
            return null;
        }

        private object DecideHard()
        {
            return null;
        }
    }
}
