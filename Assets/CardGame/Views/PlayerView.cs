using Controllers;
using Models.Cards;
using Models.Player;
using Actions;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace Views
{
    public sealed class PlayerView : MonoBehaviour
    {
        [Header("UI")]
        [SerializeField] private TMP_Text lifePointText;
        [Header("Field Slots (per slot views)")]
        [SerializeField] private FieldSlotsView[] monsterFieldSlotViews;
        [SerializeField] private FieldSlotsView[] spellFieldSlotViews;

        [Header("Feedback Events (hook Feel/MMFeedbacks)")]
        [SerializeField] private UnityEvent onDamageTaken;
        [SerializeField] private UnityEvent onCardDrawn;
        [SerializeField] private UnityEvent onCardPlayed;
        [SerializeField] private UnityEvent onCardRemoved;
        [SerializeField] private UnityEvent onPlayerDied;

        [Header("Hand View (optional - for animated draw)")]
        private HandLayoutAnimator handView;

        [SerializeField] private HandHoverInputView handHoverInputView;
        private CardInteractionAnimator cardInteractionAnimator;

        private PlayerController controller;

        private bool revealOnDraw = true;

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
            controller.CardDrawn += HandleCardDrawn;
            controller.LifePointChanged += UpdateLifePointUI;
            controller.CardPlaced += HandleCardPlaced;
            controller.CardRemovedFromField += HandleCardRemovedFromField;
            controller.PlayerDied += HandlePlayerDeath;

            UpdateLifePointUI(controller.Player.Model.LifePoint);
            UpdateHandUI();
            UpdateFieldUI();
            if (monsterFieldSlotViews != null)
            {
                for (int i = 0; i < monsterFieldSlotViews.Length; i++)
                {
                    var slotView = monsterFieldSlotViews[i];
                    if (slotView != null)
                    {
                        slotView.Initialize(controller, i, acceptMonster: true, acceptSpellOrTrap: false);
                    }
                }
            }

            if (spellFieldSlotViews != null)
            {
                for (int i = 0; i < spellFieldSlotViews.Length; i++)
                {
                    var slotView = spellFieldSlotViews[i];
                    if (slotView != null)
                    {
                        slotView.Initialize(controller, i, acceptMonster: false, acceptSpellOrTrap: true);
                    }
                }
            }
        }

        public void SetHandView(HandLayoutAnimator view)
        {
            handView = view;

            if (handHoverInputView != null)
            {
                handHoverInputView.SetHandLayout(handView);
            }
        }

        public void SetCardInteractionAnimator(CardInteractionAnimator animator)
        {
            cardInteractionAnimator = animator;
        }
        public void SetRevealOnDraw(bool value)
        {
            revealOnDraw = value;
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
            c.CardDrawn -= HandleCardDrawn;
            c.LifePointChanged -= UpdateLifePointUI;
            c.CardPlaced -= HandleCardPlaced;
            c.CardRemovedFromField -= HandleCardRemovedFromField;
            c.PlayerDied -= HandlePlayerDeath;
        }

        private void UpdateLifePointUI(int lifePoint)
        {
            if (lifePointText != null)
                lifePointText.text = lifePoint.ToString();
            // onDamageTaken?.Invoke();
        }

        private void UpdateHandUI()
        {
            if (controller == null)
                return;

            onCardDrawn?.Invoke();

            if (handView != null)
            {
                if (GameActionQueueRunner.Instance == null || !GameActionQueueRunner.Instance.Queue.IsProcessing)
                {
                    handView.IsLocked = true;
                    handView.LayoutCurrentHand(() =>
                    {
                        handView.IsLocked = false;
                    });
                }
            }
        }

        private void HandleCardDrawn(CardBase drawnCard)
        {
            if (controller == null || handView == null)
                return;

            var hand = controller.Player.Model.Hand;
            int totalAfterDraw = hand.Count;
            int index = -1;
            for (int i = 0; i < hand.Count; i++)
            {
                if (ReferenceEquals(hand[i], drawnCard))
                {
                    index = i;
                    break;
                }
            }

            if (index < 0)
                index = totalAfterDraw - 1;

            if (GameActionQueueRunner.Instance == null)
                return;
            bool isHumanPlayer = controller.Player is HumanPlayer;
            bool shouldReveal = revealOnDraw && isHumanPlayer;

            var action = new Actions.DrawCardToHandAction(handView, drawnCard, index, totalAfterDraw, shouldReveal, cardInteractionAnimator);
            GameActionQueueRunner.Instance.Queue.Enqueue(action);
        }

        private void UpdateFieldUI()
        {
            if (controller == null)
                return;

            var model = controller.Player.Model;
            if (monsterFieldSlotViews != null)
            {
                for (int i = 0; i < monsterFieldSlotViews.Length; i++)
                {
                    var slotView = monsterFieldSlotViews[i];
                    if (slotView == null)
                        continue;

                    CardBase card = (i < model.MonsterField.Count) ? model.MonsterField[i] : null;
                    slotView.SetCard(card);
                }
            }

            if (spellFieldSlotViews != null)
            {
                for (int i = 0; i < spellFieldSlotViews.Length; i++)
                {
                    var slotView = spellFieldSlotViews[i];
                    if (slotView == null)
                        continue;

                    CardBase card = (i < model.SpellTrapField.Count) ? model.SpellTrapField[i] : null;
                    slotView.SetCard(card);
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

        /// <summary>
        /// Ambil transform kartu monster di slot tertentu (jika ada),
        /// berguna untuk animasi battle.
        /// </summary>
        public Transform GetMonsterCardTransformAt(int slotIndex)
        {
            if (monsterFieldSlotViews == null || slotIndex < 0 || slotIndex >= monsterFieldSlotViews.Length)
                return null;

            var slotView = monsterFieldSlotViews[slotIndex];
            return slotView != null ? slotView.GetCurrentCardTransform() : null;
        }

        private void HandlePlayerDeath()
        {
            onPlayerDied?.Invoke();
        }
    }
}
