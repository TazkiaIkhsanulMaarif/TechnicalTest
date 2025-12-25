using System;
using System.Collections.Generic;
using Models.Cards;

namespace Models.Player
{
    public abstract class PlayerBase
    {
        protected readonly PlayerModel model;
        public PlayerModel Model => model;
        public IReadOnlyList<CardBase> Hand => model.Hand;

        public IReadOnlyList<CardBase> Graveyard => model.Graveyard;

        public IReadOnlyList<CardBase> MonsterField => model.MonsterField;

        public IReadOnlyList<CardBase> SpellTrapField => model.SpellTrapField;

        public int LifePoint => model.LifePoint;

        public bool IsDead => model.IsDead;

        protected PlayerBase(PlayerModel model)
        {
            this.model = model ?? throw new ArgumentNullException(nameof(model));
        }

        public abstract object DecideAction();

        public virtual void OnDamageTaken(int amount) { }

        public virtual void OnCardDrawn(CardBase card) { }

        public virtual void OnTurnStart() { }

        public virtual void OnTurnEnd() { }

        #region Template Method helpers

        public void StartTurn()
        {
            OnTurnStart();
        }

        public void EndTurn()
        {
            OnTurnEnd();
        }

        public void NotifyDamageTaken(int amount)
        {
            OnDamageTaken(amount);
        }

        public void NotifyCardDrawn(CardBase card)
        {
            OnCardDrawn(card);
        }

        #endregion
    }
}
