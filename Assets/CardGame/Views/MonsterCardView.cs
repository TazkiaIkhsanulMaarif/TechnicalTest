using UnityEngine;
using UnityEngine.UI;
using CardGame.Models.Cards;
using TMPro;
using UnityEngine.Events;

public sealed class MonsterCardView : MonoBehaviour
{
    [Header("UI Components")]
    [SerializeField] private Image cardImage;
    [SerializeField] private TMP_Text cardNameText;
    [SerializeField] private TMP_Text levelText;
    [SerializeField] private TMP_Text attackText;
    [SerializeField] private TMP_Text defenseText;

    private MonsterCard cardData;

    public UnityEvent OnReveal;

    public void SetData(MonsterCard card)
    {
        cardData = card;

        if (cardImage != null && card.Visual != null)
            cardImage.sprite = card.Visual.Sprite;

        if (cardNameText != null)
            cardNameText.text = card.CardName;

        if (levelText != null)
            levelText.text = $"{card.Level}";

        if (attackText != null)
            attackText.text = $"{card.Attack}";

        if (defenseText != null)
            defenseText.text = $"{card.Defense}";
    }

    public void RevealCard()
    {
        OnReveal.Invoke();
    }
}
