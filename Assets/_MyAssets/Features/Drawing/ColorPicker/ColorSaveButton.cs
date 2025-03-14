using UnityEngine;
using UnityEngine.UI;

public class ColorSaveButton : MonoBehaviour
{
    [SerializeField] private GameObject interactable;
    [SerializeField] private Image buttonImage;
    [SerializeField] private Color enabledColor = Color.white;
    [SerializeField] private Color disabledColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);

    /// <summary>
    /// ボタンのインタラクションを設定
    /// </summary>
    /// <param name="enabled"></param>
    public void SetInteractable(bool enabled)
    {
        if (buttonImage != null)
        {
            interactable.SetActive(enabled);
            buttonImage.color = enabled ? enabledColor : disabledColor;
        }
    }
}
