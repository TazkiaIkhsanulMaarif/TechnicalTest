using UnityEngine;

namespace Actions
{
    public sealed class GameActionQueueRunner : MonoBehaviour
    {
        public static GameActionQueueRunner Instance { get; private set; }

        public GameActionQueue Queue { get; } = new GameActionQueue();

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }
}
