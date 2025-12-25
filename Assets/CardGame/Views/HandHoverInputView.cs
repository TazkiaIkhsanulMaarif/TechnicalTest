using UnityEngine;
using UnityEngine.EventSystems;

namespace Views
{
    public sealed class HandHoverInputView : MonoBehaviour, IPointerMoveHandler, IPointerExitHandler
    {
        [SerializeField] private RectTransform handArea;

        private HandLayoutAnimator handLayout;
        private Transform handTransform;
        private int currentHoveredIndex = -1;

        public void SetHandLayout(HandLayoutAnimator layout)
        {
            handLayout = layout;
            handTransform = layout != null ? layout.HandParent : null;
        }

        public void OnPointerMove(PointerEventData eventData)
        {
            bool hasLayout = handLayout != null;
            bool isLocked = hasLayout && handLayout.IsLocked;

            Debug.Log($"[HandHoverInputView] OnPointerMove - hasLayout={hasLayout}, isLocked={isLocked}");

            if (!hasLayout || isLocked)
                return;
            float localX;
            if (handArea != null)
            {
                if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                        handArea,
                        eventData.position,
                        eventData.pressEventCamera,
                        out Vector2 localPoint))
                {
                    return;
                }
                if (handTransform == null)
                    return;

                Vector3 world = handArea.TransformPoint(localPoint);
                Vector3 localInHand = handTransform.InverseTransformPoint(world);
                localX = localInHand.x;
            }
            else
            {
                Camera cam = eventData.pressEventCamera != null
                    ? eventData.pressEventCamera
                    : Camera.main;
                if (cam == null)
                    return;

                Vector3 world = cam.ScreenToWorldPoint(eventData.position);
                Vector3 local = transform.InverseTransformPoint(world);
                localX = local.x;
            }

            int index = handLayout.GetIndexForLocalX(localX);
            Debug.Log($"[HandHoverInputView] localX={localX:F2}, computedIndex={index}, currentHoveredIndex={currentHoveredIndex}");
            if (index < 0)
            {
                if (currentHoveredIndex != -1)
                {
                    currentHoveredIndex = -1;
                    handLayout.ClearHoveredCard();
                }
                return;
            }

            if (index != currentHoveredIndex)
            {
                currentHoveredIndex = index;
                handLayout.SetHoveredIndex(index);
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (handLayout == null)
                return;

            Debug.Log("[HandHoverInputView] OnPointerExit - clearing hover");

            currentHoveredIndex = -1;
            handLayout.ClearHoveredCard();
        }
    }
}
