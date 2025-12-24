using UnityEngine;
using CardGame.Enums;

namespace CardGame.Models.Cards
{
    [CreateAssetMenu(
        fileName = "New Trap Card",
        menuName = "Card Game/Cards/Trap"
    )]
    public sealed class TrapCard : CardBase
    {
        [Header("Trap Info")]
        [SerializeField] private TrapTriggerType triggerType;

        public TrapTriggerType TriggerType => triggerType;

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
