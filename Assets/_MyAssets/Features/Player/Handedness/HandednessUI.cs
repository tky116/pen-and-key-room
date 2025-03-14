using UnityEngine;

/// <summary>
/// 利き手設定UIを管理するコンポーネント
/// </summary>
public class HandednessUI : MonoBehaviour
{
    [SerializeField] private CyclicOptionUI optionUI;

    private void Start()
    {
        // GameSettingsから直接読み込み
        bool isLeftHanded = GameSettings.LoadHandedness();
        optionUI.SetOption(isLeftHanded ? "left" : "right");
        optionUI.OnOptionChanged += HandleOptionChanged;
    }

    private void OnDestroy()
    {
        if (optionUI != null)
        {
            optionUI.OnOptionChanged -= HandleOptionChanged;
        }
    }

    private void HandleOptionChanged(string identifier)
    {
        if (identifier != "right" && identifier != "left") return;
        bool isLeftHanded = identifier == "left";
        GameSettings.SaveHandedness(isLeftHanded);

        if (PlayerManager.Instance != null)
        {
            PlayerManager.Instance.ToggleChangeHandedness();
        }
    }
}
