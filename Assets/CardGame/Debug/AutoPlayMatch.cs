using System.Collections;
using CardGame.Controllers;
using CardGame.Enums;
using CardGame.Testing;
using CardGame.Views;
using UnityEngine;

namespace CardGame.Debugging
{
    /// <summary>
    /// Automatically advances phases in TurnView to play a full match without user input.
    /// Relies on existing AI (SimplePhaseAI) and game rules.
    /// Attach this to a GameObject, assign TurnView and GameManagers.
    /// </summary>
    public sealed class AutoPlayMatch : MonoBehaviour
    {
        [Header("Core")]
        [SerializeField] private TurnView turnView;
        [SerializeField] private GameManager[] players; // index 0 = Player 1, 1 = Player 2, etc.

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

            // small delay to let game initialize
            yield return new WaitForSeconds(phaseDelaySeconds);

            while (isRunning)
            {
                // Check for winner
                int winnerIndex = GetWinnerIndex();
                if (winnerIndex >= 0)
                {
                    Debug.Log($"[AutoPlayMatch] Match finished. Winner: {players[winnerIndex].PlayerLabel}.");
                    yield break;
                }

                // Advance to next phase
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

            // If exactly one alive, declare winner
            if (aliveCount == 1)
            {
                return lastAliveIndex;
            }

            return -1;
        }
    }
}
