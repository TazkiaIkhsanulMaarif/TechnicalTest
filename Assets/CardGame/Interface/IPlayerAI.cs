using Controllers;
using Enums;

namespace Interfaces
{
    public interface IAIPlayer
    {
        void HandlePhase(TurnPhase phase, PlayerController self, PlayerController opponent);
    }
}
