using UnityEngine;
using Enums;

namespace Models.Cards
{
    [CreateAssetMenu(
        fileName = "New Trap Card",
        menuName = "Card Game/Cards/Trap"
    )]
    public sealed class TrapCard : CardBase
    {
        [Header("Trap Info")]
        [SerializeField] private TrapTriggerType triggerType;

        [Header("Effect")]
        [SerializeField] private TrapEffectType effectType;

        [Tooltip("Generic numeric parameter used by the selected effect (e.g. damage amount or ATK percentage).")]
        [SerializeField] private int effectValue;

        public TrapTriggerType TriggerType => triggerType;
        public TrapEffectType EffectType => effectType;
        public int EffectValue => effectValue;

        private void OnEnable()
        {
            typeof(CardBase)
                .GetField("cardType",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.SetValue(this, CardType.Trap);
        }

        public override void OnActivate()
        {
            Debug.Log($"Trap {CardName} triggered on {triggerType}");
        }
    }
}
