using System.Collections.Generic;
using Models.Cards;

namespace Interfaces
{
    public interface IDeck
    {
        IReadOnlyList<CardBase> Cards { get; }
        int Count { get; }

        CardBase Draw();
        void Shuffle();
    }
}
