using System.Collections;
using Controllers;
using Enums;
using Testing;
using Views;
using UnityEngine;

namespace Debugging
{
    public sealed class AutoPlayMatch : MonoBehaviour
    {
        [Header("Core")]
        [SerializeField] private TurnView turnView;
        [SerializeField] private GameManager[] players;

        [Header("Timing")]
        [SerializeField] private float phaseDelaySeconds = 1.0f;

        private bool isRunning;

        private void Start()
        {
            if (turnView == null)
            {
                Debug.LogError("[AutoPlayMatch] TurnView is not assigned.");
                return;
            }

            if (players == null || players.Length == 0)
            {
                Debug.LogError("[AutoPlayMatch] Players array is empty.");
                return;
            }

            StartCoroutine(RunMatchCoroutine());
        }

        private IEnumerator RunMatchCoroutine()
        {
            isRunning = true;
            Debug.Log("[AutoPlayMatch] Auto-play match started.");
            yield return new WaitForSeconds(phaseDelaySeconds);

            while (isRunning)
            {
                int winnerIndex = GetWinnerIndex();
                if (winnerIndex >= 0)
                {
                    Debug.Log($"[AutoPlayMatch] Match finished. Winner: {players[winnerIndex].PlayerLabel}.");
                    yield break;
                }
                turnView.NextPhase();

                yield return new WaitForSeconds(phaseDelaySeconds);
            }
        }

        private int GetWinnerIndex()
        {
            if (players == null)
                return -1;

            int aliveCount = 0;
            int lastAliveIndex = -1;

            for (int i = 0; i < players.Length; i++)
            {
                var gm = players[i];
                if (gm == null || gm.Controller == null)
                    continue;

                var model = gm.Controller.Player.Model;
                if (!model.IsDead)
                {
                    aliveCount++;
                    lastAliveIndex = i;
                }
            }
            if (aliveCount == 1)
            {
                return lastAliveIndex;
            }

            return -1;
        }
    }
}
