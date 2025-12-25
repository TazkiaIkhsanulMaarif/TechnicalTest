using UnityEngine;
using Enums;

namespace Models.Cards
{
    [CreateAssetMenu(
        fileName = "New Spell Card",
        menuName = "Card Game/Cards/Spell"
    )]
    public sealed class SpellCard : CardBase
    {
        [Header("Spell Info")]
        [SerializeField] private SpellType spellType;

        [Header("Effect")]
        [SerializeField] private SpellEffectType effectType;

        [Tooltip("Generic numeric parameter used by the selected effect (e.g. ATK threshold or percentage).")]
        [SerializeField] private int effectValue;

        public SpellType SpellType => spellType;
        public SpellEffectType EffectType => effectType;
        public int EffectValue => effectValue;

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
