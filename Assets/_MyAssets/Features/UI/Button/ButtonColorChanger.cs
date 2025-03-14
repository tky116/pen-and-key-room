using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// ボタンの色を変更するコンポーネント
/// </summary>
public class ButtonColorChanger : MonoBehaviour
{
    [SerializeField] protected Image targetImage;
    [SerializeField] protected Color normalColor = Color.white;
    [SerializeField] protected Color selectedColor = Color.gray;

    protected virtual void Awake()
    {
        // イメージコンポーネントが設定されていない場合は自動取得
        if (targetImage == null)
        {
            targetImage = GetComponent<Image>();
        }

        // 初期色を設定
        if (targetImage != null)
        {
            targetImage.color = normalColor;
        }
    }

    /// <summary>
    /// ボタンが選択された時の色変更処理
    /// </summary>
    public virtual void OnButtonSelect()
    {
        if (targetImage != null)
        {
            targetImage.color = selectedColor;
        }
    }

    /// <summary>
    /// ボタンが選択解除された時の色変更処理
    /// </summary>
    public virtual void OnButtonUnselect()
    {
        if (targetImage != null)
        {
            targetImage.color = normalColor;
        }
    }
}
