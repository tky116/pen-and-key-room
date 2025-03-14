using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// カラースロット
/// </summary>
public class ColorSlot : MonoBehaviour
{
    [SerializeField] private Image colorImage;
    [SerializeField] private Outline outline;  // 選択時の視覚的フィードバック用
    private Color savedColor;
    private bool isSelected;

    private void Awake()
    {
        if (colorImage == null) colorImage = GetComponent<Image>();
        if (outline == null) outline = GetComponent<Outline>();
        savedColor = colorImage.color;
        SetSelected(false);
    }

    // 自身のColorSlotを返すメソッド
    public void OnSelected()
    {
        var manager = GetComponentInParent<ColorSlotsManager>();
        if (manager != null)
        {
            manager.OnSlotSelected(this);
        }
    }

    public void SetSelected(bool selected)
    {
        isSelected = selected;
        if (outline != null)
        {
            outline.enabled = selected;
        }
    }

    public bool IsSelected() => isSelected;

    public void SetColor(Color newColor)
    {
        savedColor = newColor;
        colorImage.color = newColor;
    }

    public Color GetColor()
    {
        return savedColor;
    }
}
