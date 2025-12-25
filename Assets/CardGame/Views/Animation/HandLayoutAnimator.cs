using System;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using Models.Cards;

namespace Views
{
    public sealed class HandLayoutAnimator
    {
        private readonly Transform deckPosition;
        private readonly Transform handParent;
        private readonly Func<CardBase, GameObject> prefabSelector;

        private readonly float horizontalSpacing;
        private readonly float maxTotalWidth;
        private readonly float maxTiltAngle;
        private readonly float drawDuration;
        private readonly AnimationCurve drawCurve;
        private float hoverYOffsetConfig;
        private float hoverScaleConfig;
        private readonly List<Transform> cardTransforms = new List<Transform>();
        private Transform hoveredCard;
        private bool isLocked;
        private Sequence currentLayoutSequence;

        public Transform DeckPosition => deckPosition;
        public Transform HandParent => handParent;
        public bool IsLocked
        {
            get => isLocked;
            set => isLocked = value;
        }

        public HandLayoutAnimator(
            Transform deckPosition,
            Transform handParent,
            Func<CardBase, GameObject> prefabSelector,
            float horizontalSpacing,
            float maxTotalWidth,
            float maxTiltAngle,
            float drawDuration,
            AnimationCurve drawCurve)
        {
            this.deckPosition = deckPosition;
            this.handParent = handParent;
            this.prefabSelector = prefabSelector;
            this.horizontalSpacing = horizontalSpacing;
            this.maxTotalWidth = maxTotalWidth;
            this.maxTiltAngle = maxTiltAngle;
            this.drawDuration = drawDuration;
            this.drawCurve = drawCurve ?? AnimationCurve.EaseInOut(0, 0, 1, 1);
            hoverYOffsetConfig = maxTiltAngle;
            hoverScaleConfig = 1.1f;
        }

        public void SetHoverConfig(float hoverYOffset, float hoverScale)
        {
            hoverYOffsetConfig = hoverYOffset;
            hoverScaleConfig = hoverScale <= 0f ? 1.1f : hoverScale;
        }
        public int GetIndexForLocalX(float localX)
        {
            int total = cardTransforms.Count;

            if (total == 0 && handParent != null)
            {
                int childCount = handParent.childCount;
                for (int i = 0; i < childCount; i++)
                {
                    cardTransforms.Add(handParent.GetChild(i));
                }
                total = cardTransforms.Count;
            }

            if (total <= 0)
                return -1;

            int bestIndex = -1;
            float bestDistance = float.MaxValue;

            for (int i = 0; i < total; i++)
            {
                Transform t = cardTransforms[i];
                if (t == null)
                    continue;

                float cardX = t.localPosition.x;
                float distance = Mathf.Abs(localX - cardX);

                if (distance < bestDistance)
                {
                    bestDistance = distance;
                    bestIndex = i;
                }
            }

            return bestIndex;
        }

        public void AddCard(Transform cardTransform, int index = -1)
        {
            if (cardTransform == null)
                return;

            if (cardTransforms.Contains(cardTransform))
                return;

            if (index < 0 || index > cardTransforms.Count)
            {
                cardTransforms.Add(cardTransform);
            }
            else
            {
                cardTransforms.Insert(index, cardTransform);
            }
        }

        public void RemoveCard(Transform cardTransform)
        {
            if (cardTransform == null)
                return;

            cardTransforms.Remove(cardTransform);

            if (hoveredCard == cardTransform)
            {
                hoveredCard = null;
            }
        }

        public void SetHoveredCard(Transform cardTransform)
        {
            if (isLocked)
                return;

            hoveredCard = cardTransform;
            ReflowLayout();
        }

        public void SetHoveredIndex(int index)
        {
            if (isLocked)
            {
                Debug.Log($"[HandLayoutAnimator] SetHoveredIndex({index}) ignored because layout is locked");
                return;
            }

            if (index < 0 || index >= cardTransforms.Count)
            {
                hoveredCard = null;
            }
            else
            {
                hoveredCard = cardTransforms[index];
                Debug.Log($"[HandLayoutAnimator] SetHoveredIndex({index}) called, hoveredCard={hoveredCard.name}");
            }

            ReflowLayout();
        }

        public void ClearHoveredCard()
        {
            if (isLocked)
            {
                Debug.Log("[HandLayoutAnimator] ClearHoveredCard ignored because layout is locked");
                return;
            }

            Debug.Log("[HandLayoutAnimator] ClearHoveredCard called");

            hoveredCard = null;
            ReflowLayout();
        }

        public void ReflowLayout(Action onComplete = null)
        {
            if (handParent == null || isLocked)
            {
                Debug.Log($"[HandLayoutAnimator] ReflowLayout skipped. handParentNull={handParent == null}, isLocked={isLocked}");
                onComplete?.Invoke();
                return;
            }

            Debug.Log($"[HandLayoutAnimator] ReflowLayout started. childCount={handParent.childCount}, cachedCount={cardTransforms.Count}");

            if (cardTransforms.Count == 0)
            {
                int childCount = handParent.childCount;
                for (int i = 0; i < childCount; i++)
                {
                    cardTransforms.Add(handParent.GetChild(i));
                }
            }

            LayoutHand(cardTransforms, onComplete ?? (() => { }));
        }

        public GameObject GetPrefabForCard(CardBase card)
        {
            return prefabSelector != null ? prefabSelector(card) : null;
        }
        public Vector3 GetLocalPositionForIndex(int index, int total)
        {
            if (total <= 0)
                return Vector3.zero;

            if (total == 1)
                return Vector3.zero;

            float spacing = horizontalSpacing;
            if (spacing <= 0f)
                spacing = 1f;

            float totalWidth = spacing * (total - 1);
            if (maxTotalWidth > 0f && totalWidth > maxTotalWidth)
            {
                spacing = maxTotalWidth / (total - 1);
                totalWidth = spacing * (total - 1);
            }

            float startX = -totalWidth * 0.5f;
            float x = startX + spacing * index;

            return new Vector3(x, 0f, 0f);
        }
        public Quaternion GetLocalRotationForIndex(int index, int total)
        {
            return Quaternion.identity;
        }

        public void AnimateDraw(GameObject cardObject, int targetIndex, int totalAfterDraw, Action onComplete)
        {
            if (cardObject == null)
            {
                onComplete?.Invoke();
                return;
            }

            if (deckPosition == null || handParent == null)
            {
                Transform t = cardObject.transform;
                t.SetParent(handParent, worldPositionStays: false);
                t.localPosition = GetLocalPositionForIndex(targetIndex, totalAfterDraw);
                t.localRotation = GetLocalRotationForIndex(targetIndex, totalAfterDraw);
                onComplete?.Invoke();
                return;
            }

            Transform cardTransform = cardObject.transform;
            cardTransform.SetParent(handParent, worldPositionStays: true);

            Vector3 targetLocalPos = GetLocalPositionForIndex(targetIndex, totalAfterDraw);
            Quaternion targetLocalRot = GetLocalRotationForIndex(targetIndex, totalAfterDraw);

            float duration = Mathf.Max(0.01f, drawDuration);

            Sequence seq = DOTween.Sequence();

            seq.Join(cardTransform.DOLocalMove(targetLocalPos, duration).SetEase(drawCurve));
            seq.Join(cardTransform.DOLocalRotateQuaternion(targetLocalRot, duration).SetEase(drawCurve));

            seq.OnComplete(() => onComplete?.Invoke());
        }

        public void LayoutHand(IReadOnlyList<Transform> cardTransforms, Action onComplete)
        {
            if (cardTransforms == null || cardTransforms.Count == 0)
            {
                onComplete?.Invoke();
                return;
            }

            float duration = Mathf.Max(0.01f, drawDuration);

            if (currentLayoutSequence != null && currentLayoutSequence.IsActive())
            {
                currentLayoutSequence.Kill();
            }

            currentLayoutSequence = DOTween.Sequence();
            Sequence seq = currentLayoutSequence;

            int total = cardTransforms.Count;

            int hoveredIndex = -1;
            if (hoveredCard != null)
            {
                for (int i = 0; i < total; i++)
                {
                    if (cardTransforms[i] == hoveredCard)
                    {
                        hoveredIndex = i;
                        break;
                    }
                }
            }
            float hoverYOffset = hoverYOffsetConfig;
            if (Mathf.Abs(hoverYOffset) < 0.0001f)
            {
                hoverYOffset = 0.5f;
            }

            float hoverScale = hoverScaleConfig;
            if (hoverScale <= 0f)
            {
                hoverScale = 1.1f;
            }

            for (int i = 0; i < total; i++)
            {
                Transform t = cardTransforms[i];
                if (t == null)
                    continue;

                Vector3 targetLocalPos = GetLocalPositionForIndex(i, total);
                Quaternion targetLocalRot = GetLocalRotationForIndex(i, total);
                Vector3 targetScale = Vector3.one;

                if (hoveredIndex >= 0 && i == hoveredIndex)
                {
                    targetLocalPos.y += hoverYOffset;
                    targetScale *= hoverScale;

                    Debug.Log($"[HandLayoutAnimator] Hover applied on card '{t.name}' at index {i}, targetLocalPos={targetLocalPos}, targetScale={targetScale}");
                    if (t.parent != null && t.GetSiblingIndex() != t.parent.childCount - 1)
                    {
                        t.SetAsLastSibling();
                    }
                }
                t.DOKill();

                seq.Join(t.DOLocalMove(targetLocalPos, duration).SetEase(drawCurve));
                seq.Join(t.DOLocalRotateQuaternion(targetLocalRot, duration).SetEase(drawCurve));
                seq.Join(t.DOScale(targetScale, duration).SetEase(drawCurve));
            }

            seq.OnComplete(() =>
            {
                currentLayoutSequence = null;
                onComplete?.Invoke();
            });
        }

        public void LayoutCurrentHand(Action onComplete)
        {
            if (handParent == null)
            {
                onComplete?.Invoke();
                return;
            }

            int childCount = handParent.childCount;
            if (childCount == 0)
            {
                onComplete?.Invoke();
                return;
            }

            cardTransforms.Clear();
            for (int i = 0; i < childCount; i++)
            {
                cardTransforms.Add(handParent.GetChild(i));
            }

            LayoutHand(cardTransforms, onComplete);
        }
    }
}
