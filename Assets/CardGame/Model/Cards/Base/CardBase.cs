using UnityEngine;
using CardGame.Enums;

namespace CardGame.Models.Cards
{
    public abstract class CardBase : ScriptableObject
    {
        [Header("Base Info")]
        [SerializeField] private string cardName;
        [SerializeField] private CardType cardType;

        [Header("Visual")]
        [SerializeField] private CardVisual visual;

        public string CardName => cardName;
        public CardType CardType => cardType;

        public CardVisual Visual => visual;

        public abstract void OnActivate();
    }
}
