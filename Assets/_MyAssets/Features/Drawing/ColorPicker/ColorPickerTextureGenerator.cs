using UnityEngine;

/// <summary>
/// カラーピッカー用テクスチャの生成
/// </summary>
public static class ColorPickerTextureGenerator
{
    /// <summary>
    /// テクスチャからスプライトを生成
    /// </summary>
    private static Sprite CreateSprite(Texture2D texture)
    {
        return Sprite.Create(
            texture,
            new Rect(0, 0, texture.width, texture.height),
            new Vector2(0.5f, 0.5f)
        );
    }

    /// <summary>
    /// カラーマップのスプライトを生成
    /// </summary>
    /// <param name="size">カラーマップのサイズ(256 * 256)</param>
    /// <returns></returns>
    public static Sprite GenerateColorMap(Vector2Int size)
    {
        try
        {
            Texture2D texture = new Texture2D(size.x, size.y, TextureFormat.RGBA32, false);
            texture.wrapMode = TextureWrapMode.Clamp;
            texture.filterMode = FilterMode.Bilinear;
            Color[] colors = new Color[size.x * size.y];

            for (int y = 0; y < size.y; y++)
            {
                float saturation = (float)y / size.y;
                for (int x = 0; x < size.x; x++)
                {
                    float hue = (float)x / size.x;
                    colors[y * size.x + x] = Color.HSVToRGB(hue, saturation, 1.0f);
                }
            }

            texture.SetPixels(colors);
            texture.Apply();

            // スプライトの生成を確認
            var sprite = Sprite.Create(
                texture,
                new Rect(0, 0, texture.width, texture.height),
                new Vector2(0.5f, 0.5f),
                100.0f  // ピクセル当たりの単位を設定
            );

            if (sprite == null)
            {
                Debug.LogError("Failed to create sprite from texture");
                return null;
            }

            return sprite;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error generating color map: {e}");
            return null;
        }
    }

    /// <summary>
    /// 明度スライダーのスプライトを生成（上が白、下が黒）
    /// </summary>
    public static Sprite GenerateBrightnessSlider(Vector2Int size)
    {
        Texture2D texture = new Texture2D(size.x, size.y, TextureFormat.RGBA32, false);
        Color[] colors = new Color[size.x * size.y];
        float radius = size.x * 0.5f;

        for (int y = 0; y < size.y; y++)
        {
            float brightness = ((float)y / size.y);
            for (int x = 0; x < size.x; x++)
            {
                float alpha = CalculateRoundedAlpha(x, y, size, radius);
                colors[y * size.x + x] = new Color(brightness, brightness, brightness, alpha);
            }
        }

        texture.SetPixels(colors);
        texture.Apply();
        return CreateSprite(texture);
    }

    /// <summary>
    /// 透明度スライダーのスプライトを生成（上が黒、下が白）
    /// </summary>
    public static Sprite GenerateAlphaSlider(Vector2Int size)
    {
        Texture2D texture = new Texture2D(size.x, size.y, TextureFormat.RGBA32, false);
        Color[] colors = new Color[size.x * size.y];
        float radius = size.x * 0.5f;
        int checkerSize = 8;

        for (int y = 0; y < size.y; y++)
        {
            float gradientColor = ((float)y / size.y);

            for (int x = 0; x < size.x; x++)
            {
                // チェッカーパターン
                bool isWhite = ((x / checkerSize) + (y / checkerSize)) % 2 == 0;
                Color baseColor = isWhite ? Color.white : new Color(0.8f, 0.8f, 0.8f);

                float alpha = CalculateRoundedAlpha(x, y, size, radius);
                colors[y * size.x + x] = Color.Lerp(baseColor, Color.black, gradientColor) * alpha;
            }
        }

        texture.SetPixels(colors);
        texture.Apply();
        return CreateSprite(texture);
    }

    /// <summary>
    /// 端を丸くするためのアルファ値を計算
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="size">スライダーのサイズ(24 * 256)</param>
    /// <param name="radius">半径</param>
    /// <returns></returns>
    private static float CalculateRoundedAlpha(int x, int y, Vector2Int size, float radius)
    {
        if (y < radius) // 上端
        {
            float dist = Vector2.Distance(
                new Vector2(x, y),
                new Vector2(size.x * 0.5f, radius)
            );
            return dist <= radius ? 1f : 0f;
        }
        else if (y > size.y - radius) // 下端
        {
            float dist = Vector2.Distance(
                new Vector2(x, y),
                new Vector2(size.x * 0.5f, size.y - radius)
            );
            return dist <= radius ? 1f : 0f;
        }
        else // 中央部分
        {
            float distFromCenter = Mathf.Abs(x - size.x * 0.5f);
            return distFromCenter <= radius ? 1f : 0f;
        }
    }
}
