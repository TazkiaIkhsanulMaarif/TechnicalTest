using Models.Cards;

namespace Models.Player
{
    public sealed class HumanPlayer : PlayerBase
    {
        public HumanPlayer(PlayerModel model) : base(model)
        {
        }

        public override object DecideAction()
        {
            return null;
        }
    }
}
