using UnityEngine;
using CardGame.Enums;

namespace CardGame.Models.Cards
{
    [CreateAssetMenu(
        fileName = "New Spell Card",
        menuName = "Card Game/Cards/Spell"
    )]
    public sealed class SpellCard : CardBase
    {
        [Header("Spell Info")]
        [SerializeField] private SpellType spellType;

        public SpellType SpellType => spellType;

        private void OnEnable()
        {
            typeof(CardBase)
                .GetField("cardType",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.SetValue(this, CardType.Spell);
        }

        public override void OnActivate()
        {
            Debug.Log($"Spell {CardName} activated with type {spellType}");
        }
    }
}
