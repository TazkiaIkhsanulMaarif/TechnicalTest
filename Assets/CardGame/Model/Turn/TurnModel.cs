using System;
using Enums;

namespace Models.Turn
{
    [Serializable]
    public sealed class TurnModel
    {
        public int PlayerCount { get; }

        public int CurrentPlayerIndex { get; private set; }
        public TurnPhase CurrentPhase { get; private set; }

        public TurnModel(int playerCount)
        {
            if (playerCount <= 0)
                throw new ArgumentException("Player count must be positive.", nameof(playerCount));

            PlayerCount = playerCount;
            CurrentPlayerIndex = 0;
            CurrentPhase = TurnPhase.Draw;
        }

        public void SetPhase(TurnPhase phase)
        {
            CurrentPhase = phase;
        }

        public void NextPlayer()
        {
            CurrentPlayerIndex = (CurrentPlayerIndex + 1) % PlayerCount;
            CurrentPhase = TurnPhase.Draw;
        }
    }
}
