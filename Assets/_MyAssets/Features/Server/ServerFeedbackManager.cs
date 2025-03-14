using UnityEngine;

/// <summary>
/// サーバー処理結果のプレイヤーフィードバックの種類
/// </summary>
public enum ServerFeedbackType
{
    /// <summary>AI処理とアイテム生成の成功</summary>
    Success,
    /// <summary>AI処理失敗</summary>
    Failure
}

/// <summary>
/// サーバー処理結果をプレイヤーにフィードバックするマネージャー
/// </summary>
public class ServerFeedbackManager : MonoBehaviour
{
    public static ServerFeedbackManager Instance { get; private set; }

    [Header("Feedback Messages")]
    [Tooltip("AI処理成功時のメッセージ")]
    [SerializeField] private DialogueData successDialogueData;
    [Tooltip("AI処理失敗時のメッセージ")]
    [SerializeField] private DialogueData failureDialogueData;

    [Header("References")]
    [SerializeField] private DialogueTrigger dialogueTrigger;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// サーバー処理結果をプレイヤーにフィードバック
    /// </summary>
    /// <param name="type">フィードバックの種類</param>
    public void ShowFeedback(ServerFeedbackType type)
    {
        if (dialogueTrigger == null) return;

        DialogueData dialogueData = type switch
        {
            ServerFeedbackType.Success => successDialogueData,
            ServerFeedbackType.Failure => failureDialogueData,
            _ => null
        };

        if (dialogueData != null)
        {
            dialogueTrigger.SetDialogueData(dialogueData);
            dialogueTrigger.ShowRandomDialogue();
        }
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        // 必須コンポーネントのチェック
        if (dialogueTrigger == null)
            Debug.LogError($"Missing DialogueTrigger reference in {gameObject.name}");

        // 必須DialogueDataのチェック
        if (successDialogueData == null)
            Debug.LogError($"Missing Success Dialogue Data in {gameObject.name}");
        if (failureDialogueData == null)
            Debug.LogError($"Missing Failure Dialogue Data in {gameObject.name}");
    }
#endif
}
