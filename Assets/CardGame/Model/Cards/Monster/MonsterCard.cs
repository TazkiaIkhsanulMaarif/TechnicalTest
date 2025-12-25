using UnityEngine;
using Enums;

namespace Models.Cards
{
    [CreateAssetMenu(
        fileName = "New Monster Card",
        menuName = "Card Game/Cards/Monster"
    )]
    public sealed class MonsterCard : CardBase
    {
        [Header("Monster Stats")]
        [SerializeField] private int level;
        [SerializeField] private int attack;
        [SerializeField] private int defense;

        [Header("Role (AI hint / balancing)")]
        [SerializeField] private MonsterRole role = MonsterRole.Balanced;

        public int Level => level;
        public int Attack => attack;
        public int Defense => defense;
        public MonsterRole Role => role;

        private void OnEnable()
        {
            typeof(CardBase)
                .GetField("cardType",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.SetValue(this, CardType.Monster);
        }

        public override void OnActivate()
        {
            Debug.Log($"Monster {CardName} activated");
        }
    }
}
