using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// ペンの消しゴムモード切り替えを統合的に管理するクラス
/// </summary>
public class VRPenModeManager : MonoBehaviour
{
    [Header("Pen References")]
    [SerializeField] private VRPenStabilizer leftPenStabilizer;
    [SerializeField] private VRPenStabilizer rightPenStabilizer;

    [Header("Events")]
    public UnityEvent<bool> onEraserModeChanged;

    private bool isEraserMode = false;
    public bool IsEraserMode => isEraserMode;

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (leftPenStabilizer == null && rightPenStabilizer == null)
        {
            Debug.LogWarning("At least one VRPenStabilizer should be assigned!");
        }
    }
#endif

    private void Start()
    {
        // 初期状態を設定
        SyncStabilizersState(false);
    }

    /// <summary>
    /// 消しゴムモードを切り替える
    /// </summary>
    public void ToggleEraserMode()
    {
        SetMode(!isEraserMode);
    }

    /// <summary>
    /// 特定のモードに設定
    /// </summary>
    /// <param name="newEraserMode">設定するモード</param>
    public void SetMode(bool newEraserMode)
    {
        // 状態が変わる場合のみ処理を実行
        if (isEraserMode != newEraserMode)
        {
            isEraserMode = newEraserMode;
            SyncStabilizersState(isEraserMode);
            onEraserModeChanged?.Invoke(isEraserMode);
        }
    }

    /// <summary>
    /// 全てのスタビライザーの状態を同期
    /// </summary>
    private void SyncStabilizersState(bool eraserMode)
    {
        // 左手用ペンの更新
        if (leftPenStabilizer != null && leftPenStabilizer.IsEraserMode != eraserMode)
        {
            leftPenStabilizer.ToggleEraserMode();
        }

        // 右手用ペンの更新
        if (rightPenStabilizer != null && rightPenStabilizer.IsEraserMode != eraserMode)
        {
            rightPenStabilizer.ToggleEraserMode();
        }
    }
}
