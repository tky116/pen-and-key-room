using UnityEngine;
using System.Threading;
using Cysharp.Threading.Tasks;

/// <summary>
/// 一定時間ごとにプレイヤーの独り言（リマインダー）を表示するクラス
/// </summary>
public class PlayerReminder : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private DialogueTrigger dialogueTrigger;
    [SerializeField, Range(30f, 600f)] private float reminderInterval = 60f; // ダイアログ表示間隔（秒）

    private CancellationTokenSource _cancellationTokenSource;

    private void Start()
    {
        StartReminderLoop().Forget();
    }

    /// <summary>
    /// リマインダーループを開始する
    /// </summary>
    /// <returns></returns>
    private async UniTask StartReminderLoop()
    {
        _cancellationTokenSource = new CancellationTokenSource();

        // 1秒待機
        await UniTask.Delay(1000, cancellationToken: _cancellationTokenSource.Token);

        // 最初の1回目はすぐに表示
        if (dialogueTrigger != null)
        {
            dialogueTrigger.ShowDialogueAtIndex(0);
        }

        // その後一定間隔ごとに表示
        while (!_cancellationTokenSource.Token.IsCancellationRequested)
        {
            await UniTask.Delay((int)(reminderInterval * 1000), cancellationToken: _cancellationTokenSource.Token);
            if (dialogueTrigger != null)
            {
                dialogueTrigger.ShowRandomDialogue();
            }
        }
    }

    private void OnDestroy()
    {
        _cancellationTokenSource?.Cancel();
        _cancellationTokenSource?.Dispose();
    }
}
