using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using Actions;
using Models.Cards;

namespace Views
{
    public sealed class CardInteractionView : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        [Header("Events (opsional)")]
        [SerializeField] private UnityEvent onBeginDrag;
        [SerializeField] private UnityEvent onEndDrag;

        [Header("Interaction State")]
        [SerializeField] private bool isInteractable = true;

        private CardInteractionAnimator animator;
        private CardBase cardData;
        private Transform originalParent;
        private int originalSiblingIndex;
        private Vector3 originalPosition;
        private Vector3 originalScale;
        private CanvasGroup canvasGroup;
        private bool droppedSuccessfully;
        private bool isDragging;

        public void SetAnimator(CardInteractionAnimator value)
        {
            animator = value;
        }

        public void SetCardData(CardBase card)
        {
            cardData = card;
        }

        public CardBase CardData => cardData;

        private void Awake()
        {
            canvasGroup = GetComponent<CanvasGroup>();
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (cardData == null || eventData == null)
                return;
            if (!isInteractable)
                return;
            if (GameActionQueueRunner.Instance != null && GameActionQueueRunner.Instance.Queue.IsProcessing)
                return;

            droppedSuccessfully = false;
            isDragging = true;

            originalParent = transform.parent;
            originalSiblingIndex = transform.GetSiblingIndex();
            originalPosition = transform.position;
            originalScale = transform.localScale;

            if (canvasGroup != null)
            {
                canvasGroup.blocksRaycasts = false;
            }

            if (animator != null)
            {
                transform.localScale = Vector3.one * 1.1f;
            }

            onBeginDrag?.Invoke();
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (eventData == null)
                return;

            transform.position = eventData.position;
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            isDragging = false;
            if (canvasGroup != null)
            {
                canvasGroup.blocksRaycasts = true;
            }

            if (!droppedSuccessfully && this != null && gameObject != null)
            {
                if (originalParent != null)
                {
                    transform.SetParent(originalParent, worldPositionStays: true);
                    transform.SetSiblingIndex(originalSiblingIndex);
                }

                transform.position = originalPosition;
                transform.localScale = originalScale;
            }

            onEndDrag?.Invoke();
        }
        public void MarkDroppedSuccessfully()
        {
            droppedSuccessfully = true;
        }
        public void SetInteractable(bool value)
        {
            isInteractable = value;
        }
        public bool IsDragging => isDragging;
    }
}
