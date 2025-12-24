using CardGame.Controllers;
using CardGame.Models.Cards;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace CardGame.Views
{
    public sealed class PlayerView : MonoBehaviour
    {
        [Header("UI")]
        [SerializeField] private TMP_Text lifePointText;
        [SerializeField] private Transform handParent;
        [SerializeField] private Transform[] monsterFieldSlots = new Transform[3];
        [SerializeField] private Transform[] spellFieldSlots = new Transform[3];

        [Header("Feedback Events (hook Feel/MMFeedbacks)")]
        [SerializeField] private UnityEvent onDamageTaken;
        [SerializeField] private UnityEvent onCardDrawn;
        [SerializeField] private UnityEvent onCardPlayed;
        [SerializeField] private UnityEvent onCardRemoved;
        [SerializeField] private UnityEvent onPlayerDied;

        private PlayerController controller;

        public void Initialize(PlayerController playerController)
        {
            if (controller != null)
            {
                Unsubscribe(controller);
            }

            controller = playerController;

            if (controller == null)
                return;

            controller.HandChanged += UpdateHandUI;
            controller.LifePointChanged += UpdateLifePointUI;
            controller.CardPlaced += HandleCardPlaced;
            controller.CardRemovedFromField += HandleCardRemovedFromField;
            controller.PlayerDied += HandlePlayerDeath;

            UpdateLifePointUI(controller.Player.Model.LifePoint);
            UpdateHandUI();
            UpdateFieldUI();
        }

        private void OnDestroy()
        {
            if (controller != null)
            {
                Unsubscribe(controller);
            }
        }

        private void Unsubscribe(PlayerController c)
        {
            c.HandChanged -= UpdateHandUI;
            c.LifePointChanged -= UpdateLifePointUI;
            c.CardPlaced -= HandleCardPlaced;
            c.CardRemovedFromField -= HandleCardRemovedFromField;
            c.PlayerDied -= HandlePlayerDeath;
        }

        private void UpdateLifePointUI(int lifePoint)
        {
            if (lifePointText != null)
                lifePointText.text = lifePoint.ToString();

            Debug.Log(lifePointText);
            // onDamageTaken?.Invoke();
        }

        private void UpdateHandUI()
        {
            if (controller == null || handParent == null)
                return;

            foreach (Transform child in handParent)
            {
                Destroy(child.gameObject);
            }

            foreach (CardBase card in controller.Player.Model.Hand)
            {
                var go = new GameObject(card.CardName ?? "Card");
                go.transform.SetParent(handParent, false);
            }

            onCardDrawn?.Invoke();
        }

        private void UpdateFieldUI()
        {
            if (controller == null)
                return;

            var model = controller.Player.Model;

            for (int i = 0; i < monsterFieldSlots.Length; i++)
            {
                Transform slot = monsterFieldSlots[i];
                if (slot == null) continue;

                foreach (Transform child in slot)
                    Destroy(child.gameObject);

                CardBase card = (i < model.MonsterField.Count) ? model.MonsterField[i] : null;
                if (card != null)
                {
                    var go = new GameObject(card.CardName ?? "Monster");
                    go.transform.SetParent(slot, false);
                }
            }

            for (int i = 0; i < spellFieldSlots.Length; i++)
            {
                Transform slot = spellFieldSlots[i];
                if (slot == null) continue;

                foreach (Transform child in slot)
                    Destroy(child.gameObject);

                CardBase card = (i < model.SpellTrapField.Count) ? model.SpellTrapField[i] : null;
                if (card != null)
                {
                    var go = new GameObject(card.CardName ?? "SpellTrap");
                    go.transform.SetParent(slot, false);
                }
            }
        }

        private void HandleCardPlaced(CardBase card, int slotIndex, bool isMonster)
        {
            onCardPlayed?.Invoke();
            UpdateFieldUI();
        }

        private void HandleCardRemovedFromField(int slotIndex, bool isMonster, CardBase card)
        {
            onCardRemoved?.Invoke();
            UpdateFieldUI();
        }

        private void HandlePlayerDeath()
        {
            onPlayerDied?.Invoke();
        }
    }
}
