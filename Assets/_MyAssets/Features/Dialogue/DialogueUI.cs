using UnityEngine;
using TMPro;
using Cysharp.Threading.Tasks;
using System.Threading;

/// <summary>
/// ダイアログUIの表示を制御するクラス
/// </summary>
public class DialogueUI : MonoBehaviour
{
    public static DialogueUI Instance { get; private set; }

    [Header("References")]
    [SerializeField] private DialogueUIPositionManager positionManager;
    [SerializeField] private GameObject dialogueCanvas;
    [SerializeField] private TextMeshProUGUI messageText;

    [Header("Settings")]
    [SerializeField, Range(1f, 10f)] private float displayDuration = 4f;

    private CancellationTokenSource _cancellationTokenSource;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            messageText.color = Color.white;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        if (dialogueCanvas != null)
        {
            dialogueCanvas.SetActive(false);
        }
    }

    public async UniTask ShowMessage(string message, Color textColor)
    {
        // 前回の表示をキャンセル
        _cancellationTokenSource?.Cancel();
        _cancellationTokenSource = new CancellationTokenSource();

        if (messageText != null)
        {
            messageText.text = message;
            messageText.color = new Color(textColor.r, textColor.g, textColor.b, 1f);
        }
        if (dialogueCanvas != null)
        {
            dialogueCanvas.SetActive(true);
        }
        positionManager.UpdateUIPosition();

        try
        {
            // 指定時間待機
            await UniTask.Delay((int)(displayDuration * 1000), cancellationToken: _cancellationTokenSource.Token);
            Hide();
        }
        catch (System.OperationCanceledException)
        {
            // キャンセルされた場合は何もしない
        }
    }

    public void Hide()
    {
        _cancellationTokenSource?.Cancel();
        if (dialogueCanvas != null)
        {
            dialogueCanvas.SetActive(false);
        }
    }

    private void OnDestroy()
    {
        _cancellationTokenSource?.Cancel();
        _cancellationTokenSource?.Dispose();
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (positionManager == null)
            Debug.LogError($"Missing DialogueUIPositionManager reference in {gameObject.name}");
        if (dialogueCanvas == null)
            Debug.LogError($"Missing dialogueCanvas reference in {gameObject.name}");
        if (messageText == null)
            Debug.LogError($"Missing TextMeshProUGUI reference in {gameObject.name}");
    }
#endif
}
