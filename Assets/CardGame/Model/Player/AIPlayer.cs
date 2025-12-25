using System;
using Models.Cards;

namespace Models.Player
{
    public sealed class AIPlayer : PlayerBase
    {
        public Enums.AIDifficulty Difficulty { get; }

        public AIPlayer(PlayerModel model, Enums.AIDifficulty difficulty) : base(model)
        {
            Difficulty = difficulty;
        }

        public override object DecideAction()
        {
            switch (Difficulty)
            {
                case Enums.AIDifficulty.Easy:
                    return DecideEasy();
                case Enums.AIDifficulty.Medium:
                    return DecideMedium();
                case Enums.AIDifficulty.Hard:
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
