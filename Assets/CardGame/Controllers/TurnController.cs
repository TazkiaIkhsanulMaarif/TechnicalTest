using System;
using Enums;
using Models.Turn;

namespace Controllers
{
    public sealed class TurnController
    {
        private readonly TurnModel model;
        private bool isTransitioning;

        public event Action GameStarted;
        public event Action<int> TurnSwitched;
        public event Action<TurnPhase> PhaseChanged;

        public int CurrentPlayerIndex => model.CurrentPlayerIndex;
        public TurnPhase CurrentPhase => model.CurrentPhase;
        public bool IsTransitioning => isTransitioning;

        public TurnController(TurnModel model)
        {
            this.model = model ?? throw new ArgumentNullException(nameof(model));
        }

        public void StartGame()
        {
            if (isTransitioning)
            {
                UnityEngine.Debug.LogWarning("Cannot start game during transition");
                return;
            }

            isTransitioning = true;

            model.SetPhase(TurnPhase.Draw);

            GameStarted?.Invoke();
            PhaseChanged?.Invoke(model.CurrentPhase);

            isTransitioning = false;
        }

        public void NextPhase()
        {
            if (isTransitioning)
            {
                UnityEngine.Debug.LogWarning("Phase transition already in progress");
                return;
            }

            isTransitioning = true;

            TurnPhase currentPhase = model.CurrentPhase;
            bool shouldSwitchTurn = currentPhase == TurnPhase.End;
            TurnPhase nextPhase = GetNextPhase(currentPhase);
            if (shouldSwitchTurn)
            {
                model.NextPlayer();
                TurnSwitched?.Invoke(model.CurrentPlayerIndex);
            }

            model.SetPhase(nextPhase);
            PhaseChanged?.Invoke(model.CurrentPhase);

            isTransitioning = false;
        }

        private TurnPhase GetNextPhase(TurnPhase current)
        {
            return current switch
            {
                TurnPhase.Draw => TurnPhase.Main,
                TurnPhase.Main => TurnPhase.Battle,
                TurnPhase.Battle => TurnPhase.End,
                TurnPhase.End => TurnPhase.Draw,
                _ => TurnPhase.Draw
            };
        }
        public void ForceSetPhase(TurnPhase phase)
        {
            if (isTransitioning)
            {
                UnityEngine.Debug.LogWarning("Cannot force phase during transition");
                return;
            }

            isTransitioning = true;
            model.SetPhase(phase);
            PhaseChanged?.Invoke(model.CurrentPhase);
            isTransitioning = false;
        }
    }
}