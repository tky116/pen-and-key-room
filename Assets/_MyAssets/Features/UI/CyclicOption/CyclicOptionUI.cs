using UnityEngine;
using TMPro;
using System;

/// <summary>
/// 循環する選択肢のUIを管理するコンポーネント
/// </summary>
public class CyclicOptionUI : MonoBehaviour
{
    [System.Serializable]
    public class OptionData
    {
        public string label;         // 表示するラベル
        public string identifier;    // 選択肢の識別子
    }

    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI optionLabel;   // 選択肢のラベル

    [Header("Options")]
    [SerializeField] private OptionData[] options;  // 選択肢のリスト

    private int currentIndex;  // 現在選択中のインデックス
    public event Action<string> OnOptionChanged;  // 選択肢変更時のイベント

    private void Start()
    {
        bool isLeftHanded = GameSettings.LoadHandedness();
        string currentIdentifier = isLeftHanded ? "left" : "right";

        // 対応するインデックスを探す
        for (int i = 0; i < options.Length; i++)
        {
            if (options[i].identifier == currentIdentifier)
            {
                currentIndex = i;
                break;
            }
        }

        UpdateLabel();
    }

    /// <summary>
    /// 選択肢を切り替える
    /// </summary>
    public void OnSelect()
    {
        CycleOption();
    }

    /// <summary>
    /// 選択肢を循環させる
    /// </summary>
    private void CycleOption()
    {
        currentIndex = (currentIndex + 1) % options.Length;
        UpdateLabel();
        OnOptionChanged?.Invoke(options[currentIndex].identifier);
    }

    /// <summary>
    /// ラベルを更新する
    /// </summary>
    private void UpdateLabel()
    {
        if (options != null && options.Length > 0)
        {
            optionLabel.text = options[currentIndex].label;
        }
    }

    /// <summary>
    /// 選択肢を直接設定する
    /// </summary>
    /// <param name="identifier">選択肢の識別子</param>
    public void SetOption(string identifier)
    {
        for (int i = 0; i < options.Length; i++)
        {
            if (options[i].identifier == identifier)
            {
                currentIndex = i;
                UpdateLabel();
                break;
            }
        }
    }
}
