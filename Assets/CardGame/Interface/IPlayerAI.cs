using CardGame.Controllers;
using CardGame.Enums;

namespace CardGame.Interfaces
{
    public interface IAIPlayer
    {
        void HandlePhase(TurnPhase phase, PlayerController self, PlayerController opponent);
    }
}
