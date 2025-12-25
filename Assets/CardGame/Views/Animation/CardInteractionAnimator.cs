using UnityEngine;
using DG.Tweening;

namespace Views
{
    public sealed class CardInteractionAnimator
    {
        private readonly float hoverYOffset;
        private readonly float hoverScale;
        private readonly float selectedScale;
        private readonly float tweenDuration;
        private readonly AnimationCurve tweenCurve;

        public CardInteractionAnimator(
            float hoverYOffset,
            float hoverScale,
            float selectedScale,
            float tweenDuration,
            AnimationCurve tweenCurve)
        {
            this.hoverYOffset = hoverYOffset;
            this.hoverScale = hoverScale <= 0f ? 1.05f : hoverScale;
            this.selectedScale = selectedScale <= 0f ? this.hoverScale * 1.05f : selectedScale;
            this.tweenDuration = tweenDuration <= 0f ? 0.15f : tweenDuration;
            this.tweenCurve = tweenCurve ?? AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
        }

        private void KillTweens(Transform target)
        {
            if (target == null)
                return;

            target.DOKill();
        }

        public void ApplyIdle(Transform target, Vector3 baseLocalPosition, Vector3 baseLocalScale)
        {
            if (target == null)
                return;

            KillTweens(target);

            target.DOLocalMove(baseLocalPosition, tweenDuration)
                .SetEase(tweenCurve);

            target.DOScale(baseLocalScale, tweenDuration)
                .SetEase(tweenCurve);
        }

        public void ApplyHover(Transform target, Vector3 baseLocalPosition, Vector3 baseLocalScale)
        {
            if (target == null)
                return;

            KillTweens(target);

            Vector3 targetPos = baseLocalPosition + Vector3.up * hoverYOffset;
            Vector3 targetScale = baseLocalScale * hoverScale;

            target.DOLocalMove(targetPos, tweenDuration)
                .SetEase(tweenCurve);

            target.DOScale(targetScale, tweenDuration)
                .SetEase(tweenCurve);
        }

        public void ApplySelected(Transform target, Vector3 baseLocalPosition, Vector3 baseLocalScale)
        {
            if (target == null)
                return;

            KillTweens(target);

            Vector3 targetPos = baseLocalPosition + Vector3.up * hoverYOffset;
            Vector3 targetScale = baseLocalScale * selectedScale;

            target.DOLocalMove(targetPos, tweenDuration)
                .SetEase(tweenCurve);

            target.DOScale(targetScale, tweenDuration)
                .SetEase(tweenCurve);
        }
    }
}
