using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

/// <summary>
/// カラーディスプレイの管理
/// </summary>
public class ColorDisplay : MonoBehaviour
{
    [SerializeField] private Image previewDisplay;
    [SerializeField] private Image currentDisplay;

    /// <summary>
    /// プレビューの色を更新
    /// </summary>
    /// <param name="color"></param>
    public void UpdatePreview(Color color)
    {
        if (previewDisplay != null)
        {
            previewDisplay.color = color;
        }
    }

    /// <summary>
    /// 現在の色を更新
    /// </summary>
    /// <param name="color"></param>
    public void UpdateDisplay(Color color)
    {
        if (currentDisplay != null)
        {
            currentDisplay.color = color;
        }
    }
}
