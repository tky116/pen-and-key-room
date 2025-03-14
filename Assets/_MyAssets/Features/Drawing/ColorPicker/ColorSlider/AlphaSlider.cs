/// <summary>
/// 透明度スライダー
/// </summary>
public class AlphaSlider : BaseColorSlider
{
    /// <summary>
    /// テクスチャ生成
    /// </summary>
    protected override void GenerateTexture()
    {
        // 既存のスプライトとテクスチャを明示的に破棄
        if (sliderImage.sprite != null)
        {
            if (sliderImage.sprite.texture != null)
            {
                Destroy(sliderImage.sprite.texture);
            }
            Destroy(sliderImage.sprite);
        }

        // 新しいスプライトを生成し、識別用の名前を設定
        var newSprite = ColorPickerTextureGenerator.GenerateAlphaSlider(size);
        if (newSprite != null && newSprite.texture != null)
        {
            newSprite.texture.name = "AlphaSliderTexture_" + GetInstanceID();
        }
        sliderImage.sprite = newSprite;
    }

    /// <summary>
    /// 初期値の取得
    /// </summary>
    /// <returns></returns>
    protected override float GetInitialValue()
    {
        return 1f;
    }

    /// <summary>
    /// プレビュー更新
    /// </summary>
    /// <param name="normalizedValue"></param>
    protected override void UpdatePreview(float normalizedValue)
    {
        colorPicker.UpdatePreviewFromAlphaSlider(normalizedValue);
    }

    /// <summary>
    /// 現在の色を更新
    /// </summary>
    /// <param name="normalizedValue"></param>
    protected override void UpdateCurrent(float normalizedValue)
    {
        colorPicker.UpdateFromAlphaSlider(normalizedValue);
    }
}
