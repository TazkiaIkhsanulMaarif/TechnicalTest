using System.Collections.Generic;
using CardGame.Models.Cards;

namespace CardGame.Interfaces
{
    public interface IDeck
    {
        IReadOnlyList<CardBase> Cards { get; }
        int Count { get; }

        CardBase Draw();
        void Shuffle();
    }
}
