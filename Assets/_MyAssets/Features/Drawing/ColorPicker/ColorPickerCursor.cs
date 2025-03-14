using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// カラーピッカーのカーソル
/// </summary>
public class ColorPickerCursor : MonoBehaviour
{
    [SerializeField] private float size = 30f;
    [SerializeField] private float thickness = 2f;
    [SerializeField] private Color innerColor = Color.white;
    [SerializeField] private Color outerColor = Color.black;

    private void Start()
    {
        CreateCursor();
    }

    /// <summary>
    /// カーソルの生成
    /// </summary>
    private void CreateCursor()
    {
        // 外側の円
        var outerCircle = new GameObject("OuterCircle", typeof(Image));
        outerCircle.transform.SetParent(transform, false);
        var outerImage = outerCircle.GetComponent<Image>();
        outerImage.sprite = CreateCircleSprite(size);
        outerImage.color = outerColor;
        // RectTransformのサイズを設定
        var outerRect = outerImage.rectTransform;
        outerRect.sizeDelta = new Vector2(size, size);

        // 内側の円
        var innerCircle = new GameObject("InnerCircle", typeof(Image));
        innerCircle.transform.SetParent(transform, false);
        var innerImage = innerCircle.GetComponent<Image>();
        innerImage.sprite = CreateCircleSprite(size - thickness * 2);
        innerImage.color = innerColor;
        // RectTransformのサイズを設定
        var innerRect = innerImage.rectTransform;
        innerRect.sizeDelta = new Vector2(size - thickness * 2, size - thickness * 2);
    }

    /// <summary>
    /// 円のスプライトを生成
    /// </summary>
    /// <param name="diameter"></param>
    /// <returns></returns>
    private Sprite CreateCircleSprite(float diameter)
    {
        int texSize = Mathf.RoundToInt(diameter);
        Texture2D tex = new Texture2D(texSize, texSize, TextureFormat.RGBA32, false);

        float radius = diameter / 2f;
        float radiusSq = radius * radius;

        for (int y = 0; y < texSize; y++)
        {
            for (int x = 0; x < texSize; x++)
            {
                float dx = x - radius + 0.5f;
                float dy = y - radius + 0.5f;
                float distSq = dx * dx + dy * dy;

                Color color = distSq <= radiusSq ? Color.white : Color.clear;
                tex.SetPixel(x, y, color);
            }
        }

        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, texSize, texSize), new Vector2(0.5f, 0.5f));
    }
}
