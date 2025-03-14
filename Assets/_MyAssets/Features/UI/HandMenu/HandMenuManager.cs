using Oculus.Interaction;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// ハンドメニューの管理クラス
/// </summary>
public class HandMenuManager : MonoBehaviour
{
    [SerializeField] private MetaQuest3Input xrInput;   // MetaQuest3の入力
    [SerializeField] private Canvas handMainMenuCanvas; // Canvasコンポーネント
    [SerializeField] private GameObject mainMenu;       // Mainオブジェクト
    [SerializeField] private GameObject settingsMenu;   // Settingsオブジェクト
    [SerializeField] private GameObject helpMenu;       // Helpオブジェクト
    [SerializeField] private GameObject[] buttons;      // すべてのボタンオブジェクトを格納
    [SerializeField] private AudioTrigger menuOpenSound;   // メニューを開く音
    [SerializeField] private AudioTrigger menuCloseSound;  // メニューを閉じる音

    private void Start()
    {
        // 初期状態: HandMainMenuを非表示
        handMainMenuCanvas.enabled = false;
        SetButtonsActive(false);
        mainMenu.SetActive(true);
        settingsMenu.SetActive(false);
        helpMenu.SetActive(false);
        // メニューボタンのトグルイベントを登録
        if (xrInput != null)
        {
            xrInput.LeftQuestController.onMenuButtonToggled.AddListener(ToggleMenu);
        }
    }

    private void OnDestroy()
    {
        // イベントの登録解除
        if (xrInput != null)
        {
            xrInput.LeftQuestController.onMenuButtonToggled.RemoveListener(ToggleMenu);
        }
    }

    /// <summary>
    /// メニューの開閉を切り替える
    /// </summary>
    private void ToggleMenu(bool isToggled)
    {
        // 描画モード中はメニューを表示しない
        if (isToggled)
        {
            DrawingObjectManager[] drawingManagers = FindObjectsByType<DrawingObjectManager>(FindObjectsSortMode.None);
            foreach (var manager in drawingManagers)
            {
                if (manager != null && manager.IsDrawingMode)
                {
                    // トグル状態をリセット
                    xrInput.LeftQuestController.ResetToggles();
                    return;
                }
            }
        }

        handMainMenuCanvas.enabled = isToggled;
        SetButtonsActive(isToggled);
        if (isToggled)
        {
            menuOpenSound?.PlayAudio();
            // メニューを開いたら Main を表示（Settings, Help は閉じる）
            mainMenu.SetActive(true);
            settingsMenu.SetActive(false);
            helpMenu.SetActive(false);
        }
        else
        {
            menuCloseSound?.PlayAudio();
        }
    }

    /// <summary>
    /// ボタンの GameObject を有効/無効化する
    /// </summary>
    private void SetButtonsActive(bool isActive)
    {
        foreach (GameObject button in buttons)
        {
            if (button != null)
            {
                button.SetActive(isActive);
            }
        }
    }

    /// <summary>
    /// Playボタン - メニューを閉じる
    /// </summary>
    public void OnPlayButtonPressed()
    {
        handMainMenuCanvas.enabled = false;
        SetButtonsActive(false);
        xrInput.LeftQuestController.ResetToggles(); // トグルをリセット
    }

    /// <summary>
    /// Settingsボタン - 設定メニューを開く
    /// </summary>
    public void OnSettingsButtonPressed()
    {
        mainMenu.SetActive(false);
        settingsMenu.SetActive(true);
        helpMenu.SetActive(false);
    }

    /// <summary>
    /// Helpボタン - ヘルプメニューを開く
    /// </summary>
    public void OnHelpButtonPressed()
    {
        mainMenu.SetActive(false);
        settingsMenu.SetActive(false);
        helpMenu.SetActive(true);
    }

    /// <summary>
    /// 戻るボタン - メインメニューに戻る
    /// </summary>
    public void OnBackButtonPressed()
    {
        settingsMenu.SetActive(false);
        helpMenu.SetActive(false);
        mainMenu.SetActive(true);
    }

    /// <summary>
    /// Quitボタン - オープニングシーンへ遷移
    /// </summary>
    public void OnQuitButtonPressed()
    {
        SceneManager.LoadScene("Opening");
    }

    /// <summary>
    /// メニューが開いているか確認
    /// </summary>
    public bool IsMenuOpen => handMainMenuCanvas.enabled;
}
