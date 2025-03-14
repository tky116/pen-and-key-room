using UnityEngine;
using UnityEngine.UI;
using Oculus.Interaction;
using Cysharp.Threading.Tasks;

/// <summary>
/// カラースライダーの基底クラス
/// </summary>
public abstract class BaseColorSlider : MonoBehaviour
{
    [SerializeField] protected Image sliderImage;
    [SerializeField] protected ColorPickerCursor cursor;
    [SerializeField] protected ColorPicker colorPicker;
    [SerializeField] protected Vector2Int size = new Vector2Int(24, 256);

    protected abstract void GenerateTexture();

    protected abstract float GetInitialValue();

    protected abstract void UpdatePreview(float normalizedValue);

    protected abstract void UpdateCurrent(float normalizedValue);

    protected virtual void Start()
    {
        InitializeAsync().Forget();
    }

    private async UniTaskVoid InitializeAsync()
    {
        try
        {
            // より多くのフレームを待つ
            await UniTask.DelayFrame(2);

            // レイアウトの強制更新
            Canvas.ForceUpdateCanvases();
            LayoutRebuilder.ForceRebuildLayoutImmediate(sliderImage.rectTransform);

            // さらにフレームを待つ
            await UniTask.DelayFrame(1);

            // RectTransform が準備できていることを確認
            if (sliderImage.rectTransform.rect.width > 0 &&
                sliderImage.rectTransform.rect.height > 0)
            {
                GenerateTexture();
                SetInitialValue();
            }
            else
            {
                Debug.LogWarning("RectTransform not properly initialized");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to initialize slider: {e.Message}");
        }
    }

    /// <summary>
    /// 初期値の設定
    /// </summary>
    protected virtual void SetInitialValue()
    {
        float initialValue = GetInitialValue();
        UpdateCursor(initialValue);
        UpdatePreview(initialValue);
        UpdateCurrent(initialValue);
    }

    /// <summary>
    /// ホバー時のポインターイベント処理
    /// </summary>
    /// <param name="evt"></param>
    public void OnHoverEvent(PointerEvent evt)
    {
        Vector3 hitPoint = evt.Pose.position;
        float normalizedValue = CalculateNormalizedValue(hitPoint);
        UpdatePreview(normalizedValue);
    }

    /// <summary>
    /// 移動中のポインターイベント処理
    /// </summary>
    /// <param name="evt"></param>
    public void OnMoveEvent(PointerEvent evt)
    {
        Vector3 hitPoint = evt.Pose.position;
        float normalizedValue = CalculateNormalizedValue(hitPoint);
        UpdatePreview(normalizedValue);
    }

    /// <summary>
    /// 選択時のポインターイベント処理
    /// </summary>
    /// <param name="evt"></param>
    public void OnSelectEvent(PointerEvent evt)
    {
        Vector3 hitPoint = evt.Pose.position;
        float normalizedValue = CalculateNormalizedValue(hitPoint);
        UpdateCursor(normalizedValue);
        UpdateCurrent(normalizedValue);
    }

    /// <summary>
    /// スライダーの値を取得
    /// </summary>
    /// <returns></returns>
    public float GetValue()
    {
        return (cursor.transform.localPosition.y + sliderImage.rectTransform.rect.height * 0.5f)
            / sliderImage.rectTransform.rect.height;
    }

    /// <summary>
    /// カーソルの位置を更新
    /// </summary>
    /// <param name="normalizedValue"></param>
    public void UpdateCursor(float normalizedValue)
    {
        // 値の範囲を制限
        normalizedValue = Mathf.Clamp01(normalizedValue);

        // RectTransform が有効であることを確認
        if (sliderImage != null && sliderImage.rectTransform != null)
        {
            float height = sliderImage.rectTransform.rect.height;
            if (!Mathf.Approximately(height, 0))
            {
                Vector2 position = new Vector2(
                    0,
                    (normalizedValue * height) - (height * 0.5f)
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
    /// ノーマライズされた値を計算
    /// </summary>
    /// <param name="position"></param>
    /// <returns></returns>
    private float CalculateNormalizedValue(Vector3 position)
    {
        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            sliderImage.rectTransform,
            position,
            null,
            out localPoint
        );
        return (localPoint.y + sliderImage.rectTransform.rect.height * 0.5f)
            / sliderImage.rectTransform.rect.height;
    }

    /// <summary>
    /// テクスチャリソースの破棄
    /// </summary>
    protected virtual void OnDestroy()
    {
        // テクスチャリソースを明示的に破棄
        if (sliderImage != null && sliderImage.sprite != null)
        {
            if (sliderImage.sprite.texture != null)
            {
                Destroy(sliderImage.sprite.texture);
            }
            Destroy(sliderImage.sprite);
        }
    }
}
