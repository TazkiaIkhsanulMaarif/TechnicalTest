using System;
using System.Collections.Generic;

namespace Actions
{
    public abstract class GameAction
    {
        public abstract void Execute(Action onFinished);
    }

    public sealed class GameActionQueue
    {
        private readonly Queue<GameAction> _queue = new Queue<GameAction>();
        private bool _isProcessing;

        public bool IsProcessing => _isProcessing;

        public void Enqueue(GameAction action)
        {
            if (action == null) throw new ArgumentNullException(nameof(action));

            _queue.Enqueue(action);

            if (!_isProcessing)
            {
                ProcessNext();
            }
        }

        public void ClearPending()
        {
            _queue.Clear();
        }

        private void ProcessNext()
        {
            if (_queue.Count == 0)
            {
                _isProcessing = false;
                return;
            }

            _isProcessing = true;
            GameAction current = _queue.Dequeue();

            void OnCurrentFinished()
            {
                ProcessNext();
            }

            current.Execute(OnCurrentFinished);
        }
    }
}
