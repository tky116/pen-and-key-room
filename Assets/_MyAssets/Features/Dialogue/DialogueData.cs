using UnityEngine;

/// <summary>
/// ダイアログデータを定義するScriptableObject
/// </summary>
[CreateAssetMenu(fileName = "DialogueData", menuName = "Dialogue/Dialogue Data")]
public class DialogueData : ScriptableObject
{
    [System.Serializable]
    public class DialogueLine
    {
        [TextArea(3, 10)]
        public string message;

        /// <summary>
        /// テキストの色。デフォルトは不透明な白色 (R:1, G:1, B:1, A:1)
        /// </summary>
        public Color textColor = new Color(1f, 1f, 1f, 1f);
    }
    public DialogueLine[] dialogueLines;
}
