using UnityEngine;

namespace CardGame.Models.Cards
{
    [System.Serializable]
    public sealed class CardVisual
    {
        [SerializeField] private Sprite sprite;
        public Sprite Sprite => sprite;
        public CardVisual(Sprite sprite = null, GameObject prefab = null)
        {
            this.sprite = sprite;
        }
    }
}
