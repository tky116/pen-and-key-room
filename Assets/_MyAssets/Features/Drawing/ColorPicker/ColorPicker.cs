using UnityEngine;

/// <summary>
/// カラーピッカー本体
/// </summary>
public class ColorPicker : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private VRPenDrawingManager drawingManager;
    [SerializeField] private ColorDisplay colorDisplay;
    [SerializeField] private ColorMap colorMap;
    [SerializeField] private BaseColorSlider brightnessSlider;
    [SerializeField] private AlphaSlider alphaSlider;

    private Color currentColor = Color.black;

    private void Start()
    {
        UpdateDisplays(currentColor);
    }

    /// <summary>
    /// カラーマップからのプレビュー更新（ホバー/移動時）
    /// </summary>
    public void UpdatePreviewFromColorMap(Vector2 normalizedPosition)
    {
        float h = normalizedPosition.x;
        float s = normalizedPosition.y;
        float v = brightnessSlider.GetValue();
        float a = alphaSlider.GetValue();
        Color previewColor = Color.HSVToRGB(h, s, v);
        previewColor.a = a;
        colorDisplay.UpdatePreview(previewColor);  // プレビューのみ更新
    }

    /// <summary>
    /// カラーマップからの更新（選択時）
    /// </summary>
    public void UpdateFromColorMap(Vector2 normalizedPosition)
    {
        float h = normalizedPosition.x;
        float s = normalizedPosition.y;
        float v = brightnessSlider.GetValue();
        float a = alphaSlider.GetValue();
        currentColor = Color.HSVToRGB(h, s, v);
        currentColor.a = a;
        UpdateDisplays(currentColor);  // 現在の色を更新
    }

    /// <summary>
    /// 明度スライダーからのプレビュー更新
    /// </summary>
    public void UpdatePreviewFromBrightnessSlider(float normalizedValue)
    {
        float h, s, v;
        Color.RGBToHSV(currentColor, out h, out s, out v);
        Color previewColor = Color.HSVToRGB(h, s, normalizedValue);
        previewColor.a = currentColor.a;

        // プレビュー時はカーソルは動かさない
        colorDisplay.UpdatePreview(previewColor);
    }

    /// <summary>
    /// アルファスライダーからのプレビュー更新
    /// </summary>
    public void UpdatePreviewFromAlphaSlider(float normalizedValue)
    {
        Color previewColor = currentColor;
        previewColor.a = normalizedValue;
        colorDisplay.UpdatePreview(previewColor);
    }

    /// <summary>
    /// 明度スライダーからの更新（選択時）
    /// </summary>
    public void UpdateFromBrightnessSlider(float normalizedValue)
    {
        float h, s, v;
        Color.RGBToHSV(currentColor, out h, out s, out v);
        currentColor = Color.HSVToRGB(h, s, normalizedValue);

        // カラーマップのカーソルも更新
        colorMap.UpdateCursor(new Vector2(h, s));

        UpdateDisplays(currentColor);
    }

    /// <summary>
    /// アルファスライダーからの更新（選択時）
    /// </summary>
    public void UpdateFromAlphaSlider(float normalizedValue)
    {
        currentColor.a = normalizedValue;
        UpdateDisplays(currentColor);
    }

    /// <summary>
    /// カラーディスプレイを更新
    /// </summary>
    /// <param name="color"></param>
    private void UpdateDisplays(Color color)
    {
        colorDisplay.UpdateDisplay(color);
        if (drawingManager != null)
        {
            drawingManager.SetColor(color);
        }
        else
        {
            Debug.LogWarning("VRPen reference is not set in ColorPicker");
        }
    }

    /// <summary>
    /// 現在の色を取得
    /// </summary>
    /// <returns></returns>
    public Color GetCurrentColor() => currentColor;

    /// <summary>
    /// 現在の色を設定
    /// </summary>
    /// <param name="newColor"></param>
    public void SetCurrentColor(Color newColor)
    {
        currentColor = newColor;
        float h, s, v;
        Color.RGBToHSV(newColor, out h, out s, out v);

        colorMap.UpdateCursor(new Vector2(h, s));
        brightnessSlider.UpdateCursor(v);
        alphaSlider.UpdateCursor(newColor.a);
        UpdateDisplays(newColor);
    }
}
