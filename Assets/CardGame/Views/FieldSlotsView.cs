using System;
using Controllers;
using Models.Cards;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Views
{
    public sealed class FieldSlotsView : MonoBehaviour, IDropHandler
    {
        [Header("Slot Root (optional)")]
        [SerializeField] private Transform slotRoot;

        private Transform SlotRoot => slotRoot != null ? slotRoot : transform;
        private PlayerController controller;
        private int slotIndex;
        private bool acceptsMonster;
        private bool acceptsSpellOrTrap;

        public void Initialize(PlayerController ownerController, int index, bool acceptMonster, bool acceptSpellOrTrap)
        {
            controller = ownerController;
            slotIndex = index;
            acceptsMonster = acceptMonster;
            acceptsSpellOrTrap = acceptSpellOrTrap;
        }

        public void SetCard(CardBase card)
        {
            var root = SlotRoot;
            if (root == null)
                return;
            if (card == null)
            {
                foreach (Transform child in root)
                {
                    Destroy(child.gameObject);
                }
            }
        }

        /// <summary>
        /// Mengambil transform kartu yang sedang berada di slot ini (jika ada).
        /// </summary>
        public Transform GetCurrentCardTransform()
        {
            var root = SlotRoot;
            if (root == null || root.childCount == 0)
                return null;

            return root.GetChild(0);
        }

        public void OnDrop(PointerEventData eventData)
        {
            if (controller == null || eventData == null)
                return;

            var draggedObject = eventData.pointerDrag;
            if (draggedObject == null)
                return;

            if (!draggedObject.TryGetComponent(out CardInteractionView interactionView))
                return;

            CardBase card = interactionView.CardData;
            if (card == null)
                return;

            bool isMonsterCard = card is MonsterCard;
            bool isSpellOrTrapCard = card is SpellCard || card is TrapCard;

            if ((isMonsterCard && !acceptsMonster) || (isSpellOrTrapCard && !acceptsSpellOrTrap))
            {
                return;
            }

            try
            {
                bool success = false;
                if (isMonsterCard && card is MonsterCard monsterCard)
                {
                    success = controller.SummonMonsterToSlot(monsterCard, slotIndex);
                }
                else
                {
                    controller.PlayCard(card, slotIndex);
                    success = true;
                }

                if (!success)
                {
                    return;
                }

                var root = SlotRoot;
                if (root != null)
                {
                    var t = draggedObject.transform;
                    t.SetParent(root, worldPositionStays: false);
                    t.localPosition = Vector3.zero;
                    t.localRotation = Quaternion.identity;
                    t.localScale = Vector3.one;
                }
                interactionView.MarkDroppedSuccessfully();
                interactionView.SetInteractable(false);
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[FieldSlotsView] Failed to play card on drop: {ex.Message}");
            }
        }
    }
}
