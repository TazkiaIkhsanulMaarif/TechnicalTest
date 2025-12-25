using System;
using Models.Cards;
using Views;
using UnityEngine;

namespace Actions
{
    public sealed class DrawCardToHandAction : GameAction
    {
        private readonly HandLayoutAnimator handView;
        private readonly CardBase cardModel;
        private readonly int targetIndexInHand;
        private readonly int totalCardsAfterDraw;
        private readonly bool revealOnDraw;
        private readonly CardInteractionAnimator cardInteractionAnimator;

        public DrawCardToHandAction(HandLayoutAnimator handView, CardBase cardModel, int targetIndexInHand, int totalCardsAfterDraw, bool revealOnDraw, CardInteractionAnimator cardInteractionAnimator)
        {
            this.handView = handView;
            this.cardModel = cardModel;
            this.targetIndexInHand = targetIndexInHand;
            this.totalCardsAfterDraw = totalCardsAfterDraw;
            this.revealOnDraw = revealOnDraw;
            this.cardInteractionAnimator = cardInteractionAnimator;
        }

        public override void Execute(Action onFinished)
        {
            if (handView == null || handView.HandParent == null)
            {
                onFinished?.Invoke();
                return;
            }

            GameObject prefab = handView.GetPrefabForCard(cardModel);
            if (prefab == null)
            {
                onFinished?.Invoke();
                return;
            }

            Vector3 spawnPosition = handView.DeckPosition != null
                ? handView.DeckPosition.position
                : handView.HandParent.position;

            Quaternion spawnRotation = handView.DeckPosition != null
                ? handView.DeckPosition.rotation
                : handView.HandParent.rotation;

            GameObject cardGO = UnityEngine.Object.Instantiate(prefab, spawnPosition, spawnRotation, handView.HandParent);

            if (cardModel is MonsterCard monster && cardGO.TryGetComponent(out MonsterCardView monsterView))
            {
                monsterView.SetData(monster);
            }
            else if (cardModel is SpellCard spell && cardGO.TryGetComponent(out SpellCardView spellView))
            {
                spellView.SetData(spell);
            }
            else if (cardModel is TrapCard trap && cardGO.TryGetComponent(out TrapCardView trapView))
            {
                trapView.SetData(trap);
            }

            if (cardGO.TryGetComponent(out CardInteractionView interactionView))
            {
                interactionView.SetAnimator(cardInteractionAnimator);
                interactionView.SetCardData(cardModel);
            }

            handView.IsLocked = true;

            handView.AnimateDraw(cardGO, targetIndexInHand, totalCardsAfterDraw,
                () => handView.LayoutCurrentHand(() =>
                {
                    handView.IsLocked = false;

                    if (revealOnDraw && cardGO != null)
                    {
                        if (cardModel is MonsterCard && cardGO.TryGetComponent(out MonsterCardView mv))
                        {
                            mv.RevealCard();
                        }
                        else if (cardModel is SpellCard && cardGO.TryGetComponent(out SpellCardView sv))
                        {
                            sv.RevealCard();
                        }
                        else if (cardModel is TrapCard && cardGO.TryGetComponent(out TrapCardView tv))
                        {
                            tv.RevealCard();
                        }
                    }

                    onFinished?.Invoke();
                }));
        }
    }
}
