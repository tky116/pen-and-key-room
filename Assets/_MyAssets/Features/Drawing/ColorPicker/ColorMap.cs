using UnityEngine;
using UnityEngine.UI;
using Oculus.Interaction;
using Cysharp.Threading.Tasks;

/// <summary>
/// カラーマップ
/// </summary>
public class ColorMap : MonoBehaviour
{
    [SerializeField] private Image mapImage;
    [SerializeField] private ColorPickerCursor cursor;
    [SerializeField] private ColorPicker colorPicker;
    [SerializeField] private Vector2Int size = new Vector2Int(256, 256);

    private void Start()
    {
        InitializeAsync().Forget();
    }

    /// <summary>
    /// レイアウトが計算された後の初期化
    /// </summary>
    /// <returns></returns>
    private async UniTaskVoid InitializeAsync()
    {
        try
        {
            // レイアウト計算のために複数フレーム待つ
            await UniTask.DelayFrame(2);

            // レイアウトを強制更新
            Canvas.ForceUpdateCanvases();
            LayoutRebuilder.ForceRebuildLayoutImmediate(mapImage.rectTransform);

            await UniTask.DelayFrame(1);

            // テクスチャの生成と設定を確認
            var sprite = ColorPickerTextureGenerator.GenerateColorMap(size);
            if (sprite != null)
            {
                mapImage.sprite = sprite;
                // テクスチャ設定後の追加確認
                mapImage.enabled = true;
                mapImage.preserveAspect = true;

                // サイズの明示的な設定
                var rect = mapImage.rectTransform;
                rect.sizeDelta = new Vector2(size.x, size.y);

                // マテリアルの設定を確認
                if (mapImage.material != null)
                {
                    mapImage.material.mainTexture = sprite.texture;
                }

            }
            else
            {
                Debug.LogError("Failed to generate ColorMap sprite");
            }

            // 初期位置を設定（中央）
            Vector2 initialPosition = new Vector2(0.5f, 0.5f);
            UpdateCursor(initialPosition);
            colorPicker.UpdateFromColorMap(initialPosition);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error in ColorMap InitializeAsync: {e}");
        }
    }

    /// <summary>
    /// ホバー時のポインターイベント処理
    /// </summary>
    public void OnHoverEvent(PointerEvent evt)
    {
        Vector3 hitPoint = evt.Pose.position;
        Vector2 normalizedPos = CalculateNormalizedPosition(hitPoint);
        colorPicker.UpdatePreviewFromColorMap(normalizedPos);  // プレビュー更新
    }

    /// <summary>
    /// 移動中のポインターイベント処理
    /// </summary>
    public void OnMoveEvent(PointerEvent evt)
    {
        Vector3 hitPoint = evt.Pose.position;
        Vector2 normalizedPos = CalculateNormalizedPosition(hitPoint);
        colorPicker.UpdatePreviewFromColorMap(normalizedPos);  // プレビュー更新
    }

    /// <summary>
    /// 選択時のポインターイベント処理
    /// </summary>
    public void OnSelectEvent(PointerEvent evt)
    {
        Vector3 hitPoint = evt.Pose.position;
        Vector2 normalizedPos = CalculateNormalizedPosition(hitPoint);
        UpdateCursor(normalizedPos);  // カーソルを更新
        colorPicker.UpdateFromColorMap(normalizedPos);  // 現在の色として設定
    }

    /// <summary>
    /// カーソルの位置を更新
    /// </summary>
    //public void UpdateCursor(Vector2 normalizedPosition)
    //{
    //    var position = new Vector2(
    //        normalizedPosition.x * mapImage.rectTransform.rect.width - mapImage.rectTransform.rect.width * 0.5f,
    //        normalizedPosition.y * mapImage.rectTransform.rect.height - mapImage.rectTransform.rect.height * 0.5f
    //    );
    //    cursor.transform.localPosition = position;
    //}
    public void UpdateCursor(Vector2 normalizedPosition)
    {
        // 値の範囲を制限
        normalizedPosition = new Vector2(
            Mathf.Clamp01(normalizedPosition.x),
            Mathf.Clamp01(normalizedPosition.y)
        );

        if (mapImage != null && mapImage.rectTransform != null)
        {
            float width = mapImage.rectTransform.rect.width;
            float height = mapImage.rectTransform.rect.height;

            if (!Mathf.Approximately(width, 0) && !Mathf.Approximately(height, 0))
            {
                Vector2 position = new Vector2(
                    normalizedPosition.x * width - width * 0.5f,
                    normalizedPosition.y * height - height * 0.5f
                );

                // NaN チェック
                if (!float.IsNaN(position.x) && !float.IsNaN(position.y))
                {
                    cursor.transform.localPosition = new Vector3(position.x, position.y, 0);
                }
                else
                {
                    Debug.LogWarning("Invalid cursor position calculated");
                }
            }
        }
    }

    /// <summary>
    /// ノーマライズされた座標を計算
    /// </summary>
    /// <param name="position"></param>
    /// <returns></returns>
    private Vector2 CalculateNormalizedPosition(Vector3 position)
    {
        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            mapImage.rectTransform,
            position,
            null,
            out localPoint
        );
        return new Vector2(
            (localPoint.x + mapImage.rectTransform.rect.width * 0.5f) / mapImage.rectTransform.rect.width,
            (localPoint.y + mapImage.rectTransform.rect.height * 0.5f) / mapImage.rectTransform.rect.height
        );
    }
}
