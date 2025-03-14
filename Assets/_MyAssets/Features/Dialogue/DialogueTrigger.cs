using UnityEngine;
using Cysharp.Threading.Tasks;

/// <summary>
/// ダイアログを表示するトリガー
/// </summary>
public class DialogueTrigger : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private DialogueData dialogueData;

    /// <summary>
    /// ダイアログデータを設定する
    /// </summary>
    public void SetDialogueData(DialogueData newData)
    {
        dialogueData = newData;
    }

    /// <summary>
    /// UnityEventから呼び出すための同期メソッド
    /// </summary>
    public void ShowRandomDialogue()
    {
        DisplayRandomDialogue().Forget();
    }

    /// <summary>
    /// ダイアログをランダムで表示する（非同期処理）
    /// </summary>
    private async UniTask DisplayRandomDialogue()
    {
        if (DialogueUI.Instance == null || dialogueData == null) return;

        if (dialogueData.dialogueLines.Length > 0)
        {
            var randomLine = dialogueData.dialogueLines[Random.Range(0, dialogueData.dialogueLines.Length)];
            await DialogueUI.Instance.ShowMessage(randomLine.message, randomLine.textColor);
        }
    }

    /// <summary>
    /// UnityEventから呼び出すための同期メソッド（インデックス指定）
    /// </summary>
    public void ShowDialogueAtIndex(int index)
    {
        DisplayDialogueAtIndex(index).Forget();
    }

    /// <summary>
    /// 指定したインデックスのダイアログを表示する（非同期処理）
    /// </summary>
    private async UniTask DisplayDialogueAtIndex(int index)
    {
        if (DialogueUI.Instance == null || dialogueData == null) return;

        if (index >= 0 && index < dialogueData.dialogueLines.Length)
        {
            var line = dialogueData.dialogueLines[index];
            await DialogueUI.Instance.ShowMessage(line.message, line.textColor);
        }
    }
}
