using UnityEngine;
using UnityEngine.UI;
using Models.Cards;
using TMPro;
using UnityEngine.Events;

public sealed class TrapCardView : MonoBehaviour
{
    [Header("UI Components")]
    [SerializeField] private Image cardImage;
    [SerializeField] private TMP_Text cardNameText;
    [SerializeField] private TMP_Text triggerTypeText;

    private TrapCard cardData;

    public UnityEvent OnReveal;

    public void SetData(TrapCard card)
    {
        cardData = card;

        if (cardImage != null && card.Visual != null)
            cardImage.sprite = card.Visual.Sprite;

        if (cardNameText != null)
            cardNameText.text = card.CardName;

        if (triggerTypeText != null)
            triggerTypeText.text = $"Trigger: {card.TriggerType}";
    }

    public void RevealCard()
    {
        OnReveal.Invoke();
    }
}
