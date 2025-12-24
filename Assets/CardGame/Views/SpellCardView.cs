using UnityEngine;
using UnityEngine.UI;
using CardGame.Models.Cards;
using TMPro;
using UnityEngine.Events;

public sealed class SpellCardView : MonoBehaviour
{
    [Header("UI Components")]
    [SerializeField] private Image cardImage;
    [SerializeField] private TMP_Text cardNameText;
    [SerializeField] private TMP_Text spellTypeText;

    private SpellCard cardData;

    public UnityEvent OnReveal;

    public void SetData(SpellCard card)
    {
        cardData = card;

        if (cardImage != null && card.Visual != null)
            cardImage.sprite = card.Visual.Sprite;

        if (cardNameText != null)
            cardNameText.text = card.CardName;

        if (spellTypeText != null)
            spellTypeText.text = $"Type: {card.SpellType}";
    }

    public void RevealCard()
    {
        OnReveal.Invoke();
    }
}
