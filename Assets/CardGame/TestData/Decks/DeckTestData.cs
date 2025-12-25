using System.Collections.Generic;
using UnityEngine;
using Models.Cards;
using Enums;

namespace Testing
{
    [CreateAssetMenu(
        fileName = "New Test Deck",
        menuName = "Card Game/Test/Test Deck"
    )]
    public sealed class DeckTestData : ScriptableObject
    {
        public List<CardBase> cards;

        [Header("Prefabs per Card Type")]
        public GameObject monsterPrefab;
        public GameObject spellPrefab;
        public GameObject trapPrefab;
        public GameObject GetPrefab(CardBase card)
        {
            return card.CardType switch
            {
                CardType.Monster => monsterPrefab,
                CardType.Spell => spellPrefab,
                CardType.Trap => trapPrefab,
                _ => null
            };
        }
    }
}
