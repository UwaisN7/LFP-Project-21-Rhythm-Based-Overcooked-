using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class OrderCardUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Image recipeIcon;
    [SerializeField] private TMP_Text recipeName;
    [SerializeField] private TMP_Text timeRemainingText;

    public void Setup(PizzaRecipe recipe)
    {
        recipeIcon.sprite = recipe.recipeIcon;
        recipeName.text = recipe.recipeName;
    }

    public void UpdateTimer(float timeRemaining)
    {
        int seconds = Mathf.CeilToInt(timeRemaining);

        int minutes = seconds / 60;
        seconds %= 60;

        timeRemainingText.text =
            minutes.ToString("00") + ":" + seconds.ToString("00");
    }
}
