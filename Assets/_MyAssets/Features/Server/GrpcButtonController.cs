using UnityEngine;
using TMPro;
using Cysharp.Threading.Tasks;
using System.Threading;
using System;

/// <summary>
/// gRPC通信を行うボタンの制御
/// </summary>
public class GrpcButtonController : ButtonColorChanger
{
    [Header("GRPC Settings")]
    [SerializeField] private GrpcClient grpcClient;
    [SerializeField] private Transform inkPoolSynced;

    [Header("Button References")]
    [SerializeField] private GameObject buttonInteractable;
    [SerializeField] private TextMeshProUGUI statusText;

    [Header("AI Upload Settings")]
    [SerializeField] private float uploadIntervalSeconds = 3f;

    [Header("Button Colors")]
    [SerializeField] private Color enabledColor = new Color(1f, 1f, 1f, 1f);
    [SerializeField] private Color disabledColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);

    private bool isProcessing = false;
    private float lastUploadTime = 0f;
    private CancellationTokenSource updateCheckCts;
    private bool isInitialized = false;

    private void Start()
    {
        base.Awake();
        InitializeAsync().Forget();
    }

    private async UniTaskVoid InitializeAsync()
    {
        if (isInitialized) return;

        updateCheckCts = new CancellationTokenSource();
        await StartButtonStateCheck();
        isInitialized = true;
    }

    private void OnEnable()
    {
        if (isInitialized)
        {
            updateCheckCts?.Dispose();
            updateCheckCts = new CancellationTokenSource();
            StartButtonStateCheck().Forget();
        }
    }

    private void OnDisable()
    {
        updateCheckCts?.Cancel();
    }

    private void OnDestroy()
    {
        updateCheckCts?.Cancel();
        updateCheckCts?.Dispose();
        updateCheckCts = null;
        isInitialized = false;
    }

    /// <summary>
    /// ボタンの状態を継続的にチェック
    /// </summary>
    private async UniTask StartButtonStateCheck()
    {
        try
        {
            while (!updateCheckCts.Token.IsCancellationRequested)
            {
                if (this == null || !this.gameObject.activeInHierarchy) break;

                UpdateButtonState();
                await UniTask.Yield(updateCheckCts.Token);
            }
        }
        catch (OperationCanceledException)
        {
            // キャンセルは正常な動作なので無視
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Error in button state check: {ex.Message}");
        }
    }

    /// <summary>
    /// ボタンクリック時の処理
    /// </summary>
    public async void OnButtonClicked()
    {
        if (!CanUpload()) return;

        try
        {
            isProcessing = true;
            UpdateButtonState();

            await grpcClient.SendDrawingDataAndWait();
            lastUploadTime = Time.time;
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Error in button processing: {ex.Message}");
            SetStatusText("エラーが発生しました", Color.red);
        }
        finally
        {
            isProcessing = false;
            UpdateButtonState();
        }
    }

    /// <summary>
    /// アップロード可能な状態かどうかを判定
    /// </summary>
    private bool CanUpload()
    {
        if (isProcessing) return false;
        if (Time.time - lastUploadTime < uploadIntervalSeconds) return false;
        if (inkPoolSynced == null) return false;

        var lineRenderers = inkPoolSynced.GetComponentsInChildren<LineRenderer>();
        return lineRenderers != null && lineRenderers.Length > 0;
    }

    /// <summary>
    /// ボタンの状態を更新
    /// </summary>
    private void UpdateButtonState()
    {
        if (this == null || !this.gameObject.activeInHierarchy) return;

        bool canUpload = CanUpload();

        // インタラクション制御
        if (buttonInteractable != null)
        {
            buttonInteractable.SetActive(canUpload);
        }

        // ボタンの色を更新
        if (targetImage != null)
        {
            targetImage.color = canUpload ? enabledColor : disabledColor;
        }

        // ステータステキスト更新
        UpdateStatusText(canUpload);
    }

    /// <summary>
    /// ステータステキストを更新
    /// </summary>
    private void UpdateStatusText(bool canUpload)
    {
        if (statusText == null || grpcClient == null) return;

        if (isProcessing)
        {
            // 処理中はGrpcClientのステータスを表示
            statusText.text = grpcClient.StatusMessage;
            statusText.color = grpcClient.StatusColor;
        }
        else if (!canUpload)
        {
            if (Time.time - lastUploadTime < uploadIntervalSeconds)
            {
                float remainingTime = uploadIntervalSeconds - (Time.time - lastUploadTime);
                SetStatusText($"休憩中... {remainingTime:F1}秒", Color.white);
            }
            else
            {
                var lineRenderers = inkPoolSynced?.GetComponentsInChildren<LineRenderer>();
                if (lineRenderers == null || lineRenderers.Length == 0)
                {
                    SetStatusText("描いてね！", Color.white);
                }
                else
                {
                    SetStatusText("送信できます！", Color.white);
                }
            }
        }
        else
        {
            SetStatusText("送信できます！", Color.white);
        }
    }

    /// <summary>
    /// ステータステキストを設定
    /// </summary>
    private void SetStatusText(string text, Color color)
    {
        if (statusText != null)
        {
            statusText.text = text;
            statusText.color = color;
        }
    }

    /// <summary>
    /// ボタンが選択された時の色変更処理
    /// </summary>
    public override void OnButtonSelect()
    {
        if (CanUpload())
        {
            base.OnButtonSelect();
        }
    }

    /// <summary>
    /// ボタンが選択解除された時の色変更処理
    /// </summary>
    public override void OnButtonUnselect()
    {
        if (CanUpload())
        {
            base.OnButtonUnselect();
        }
        else
        {
            // 選択解除時に無効状態の色に戻す
            if (targetImage != null)
            {
                targetImage.color = disabledColor;
            }
        }
    }
}
