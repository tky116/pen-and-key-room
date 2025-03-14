using Oculus.Interaction;
using UnityEngine;

/// <summary>
/// 描画オブジェクトの管理クラス
/// </summary>
public class DrawingObjectManager : MonoBehaviour
{
    [Header("MetaQuest")]
    [SerializeField] private MetaQuest3Input metaQuestInput;         // Questの入力管理

    [Header("Player Movement Control")]
    [SerializeField] private OVRPlayerController playerController;  // プレイヤーコントローラ

    [Header("References")]
    [SerializeField] private GameObject drawingUI;  // 描画UIの親オブジェクト
    [SerializeField] private GameObject vrPen;      // VRペンオブジェクト
    [SerializeField] private GameObject ovrControllerPrefab;          // 通常のコントローラー表示用
    [SerializeField] private GameObject controllerInteraction;        // コントローラのインタラクション用
    [SerializeField] private VRPenRaycastManager raycastManager;      // Raycastマネージャーへの参照
    [SerializeField] private ControllerUIManager controllerUIManager; // コントローラーUI管理
    [SerializeField] private DrawingUIPositionManager drawingUIPositionManager; // UI位置管理

    [Header("Audio")]
    [SerializeField] private AudioTrigger drawingModeEnterSound;  // 描画モード開始音
    [SerializeField] private AudioTrigger drawingModeExitSound;   // 描画モード終了音

    private bool isLeftHanded = false;  // 利き手の状態
    private bool isDrawingMode = false; // 現在のモード
    private float defaultPlayerControllerAcceleration;  // デフォルトの移動速度を保存
    private bool defaultEnableRotation;                 // デフォルトの回転有効状態を保存

    /// <summary>
    /// 描画モードの状態を取得
    /// </summary>
    public bool IsDrawingMode => isDrawingMode;

    private void OnEnable()
    {
        // イベントの購読開始
        PlayerManager.OnHandednessChanged += HandleHandednessChanged;

        // metaQuestInput の初期化チェックを追加
        if (metaQuestInput == null)
        {
            Debug.LogWarning($"MetaQuestInput is not initialized on {gameObject.name}. Will retry in Start.");
            return;
        }

        // 初期状態を設定
        if (PlayerManager.Instance != null)
        {
            isLeftHanded = PlayerManager.Instance.IsLeftHanded;
        }

        // デフォルトのプレイヤーの移動速度と回転速度を保存
        if (playerController != null)
        {
            defaultPlayerControllerAcceleration = playerController.Acceleration;
            defaultEnableRotation = playerController.EnableRotation;
        }

        SubscribeToControllerEvents();
    }

    private void OnDisable()
    {
        PlayerManager.OnHandednessChanged -= HandleHandednessChanged;
        UnsubscribeFromControllerEvents();
    }

    private void Start()
    {
        // 初期状態は通常モード
        SetUIDisplay(false);

        // OnEnable での初期化に失敗していた場合の再試行
        if (metaQuestInput != null &&
            (metaQuestInput.LeftQuestController.onSecondaryButtonToggled.GetPersistentEventCount() == 0 ||
             metaQuestInput.RightQuestController.onSecondaryButtonToggled.GetPersistentEventCount() == 0))
        {
            SubscribeToControllerEvents();
        }
    }

    /// <summary>
    /// セカンダリボタンのトグルイベントハンドラー
    /// </summary>
    private void OnSecondaryButtonToggled(bool toggleState)
    {
        // 利き手の反対側のコントローラーを取得
        var passiveController = isLeftHanded ?
            metaQuestInput.RightQuestController :
            metaQuestInput.LeftQuestController;

        bool isFromPassiveController = (passiveController.SecondaryButtonToggle == toggleState);

        if (isFromPassiveController)
        {
            // メニュー表示中は描画モードを有効にしない
            HandMenuManager menuManager = FindAnyObjectByType<HandMenuManager>();
            if (toggleState && menuManager != null && menuManager.IsMenuOpen)
            {
                passiveController.ResetToggles(); // トグル状態をリセット
                return;
            }

            bool wasDrawingMode = isDrawingMode;  // 前の状態を保存
            isDrawingMode = toggleState;

            // 描画モードの切り替え音を再生
            if (isDrawingMode)
            {
                drawingModeEnterSound?.PlayAudio();
            }
            else
            {
                drawingModeExitSound?.PlayAudio();
            }

            // 描画モードに入る時のみUIの位置を更新
            if (!wasDrawingMode && isDrawingMode && drawingUIPositionManager != null)
            {
                drawingUIPositionManager.UpdateUIPosition();
            }

            // オブジェクトの表示/非表示のみを更新
            UpdateObjectVisibility(isDrawingMode);

            // プレイヤーの移動制御を更新
            UpdatePlayerControl(isDrawingMode);
        }
    }

    /// <summary>
    /// オブジェクトの表示/非表示を切り替える
    /// </summary>
    /// <param name="isVisible"></param>
    private void UpdateObjectVisibility(bool isVisible)
    {
        if (drawingUI != null) drawingUI.SetActive(isVisible);
        if (vrPen != null) vrPen.SetActive(isVisible);
        if (ovrControllerPrefab != null) ovrControllerPrefab.SetActive(!isVisible);
        if (controllerInteraction != null) controllerInteraction.SetActive(!isVisible);

        // Rayの表示切り替え
        if (raycastManager != null)
        {
            var rayVisualizer = raycastManager.transform.Find("RayVisualizer(Clone)");
            if (rayVisualizer != null)
            {
                rayVisualizer.gameObject.SetActive(isVisible);
            }
        }

        // コントローラーUIを更新
        if (controllerUIManager != null)
        {
            controllerUIManager.UpdateControllerUI(isLeftHanded, isVisible);
        }
    }

    /// <summary>
    /// プレイヤーの移動制御を更新
    /// </summary>
    /// <param name="isDrawingMode"></param>
    private void UpdatePlayerControl(bool isDrawingMode)
    {
        if (playerController != null)
        {
            if (isDrawingMode)
            {
                playerController.Acceleration = 0f;
                playerController.EnableRotation = false;
            }
            else
            {
                playerController.Acceleration = defaultPlayerControllerAcceleration;
                playerController.EnableRotation = defaultEnableRotation;
            }
        }
    }

    /// <summary>
    /// コントローラーのイベント購読
    /// </summary>
    private void SubscribeToControllerEvents()
    {
        // null チェックを追加
        if (metaQuestInput?.LeftQuestController == null || metaQuestInput?.RightQuestController == null)
        {
            Debug.LogWarning($"Controllers are not initialized on {gameObject.name}");
            return;
        }

        metaQuestInput.LeftQuestController.onSecondaryButtonToggled.AddListener(OnSecondaryButtonToggled);
        metaQuestInput.RightQuestController.onSecondaryButtonToggled.AddListener(OnSecondaryButtonToggled);
    }

    /// <summary>
    /// コントローラーのイベント解除
    /// </summary>
    private void UnsubscribeFromControllerEvents()
    {
        if (metaQuestInput != null)
        {
            metaQuestInput.LeftQuestController.onSecondaryButtonToggled.RemoveListener(OnSecondaryButtonToggled);
            metaQuestInput.RightQuestController.onSecondaryButtonToggled.RemoveListener(OnSecondaryButtonToggled);
        }
    }

    /// <summary>
    /// 利き手変更イベントのハンドラー
    /// </summary>
    private void HandleHandednessChanged(bool newIsLeftHanded)
    {
        isLeftHanded = newIsLeftHanded;

        // 描画モードを強制的にオフに
        isDrawingMode = false;

        // 分割した関数を使用
        UpdateObjectVisibility(false);
        UpdatePlayerControl(false);

        // コントローラーのイベントを再購読
        UnsubscribeFromControllerEvents();

        // トグル状態をリセット
        if (metaQuestInput != null)
        {
            metaQuestInput.LeftQuestController.ResetToggles();
            metaQuestInput.RightQuestController.ResetToggles();
        }

        SubscribeToControllerEvents();
    }

    /// <summary>
    /// UIの表示状態を設定する
    /// </summary>
    private void SetUIDisplay(bool isVisible)
    {
        // 描画関連のUIとオブジェクトの表示を切り替え
        if (drawingUI != null) drawingUI.SetActive(isVisible);
        if (vrPen != null) vrPen.SetActive(isVisible);
        if (ovrControllerPrefab != null) ovrControllerPrefab.SetActive(!isVisible);
        if (controllerInteraction != null) controllerInteraction.SetActive(!isVisible);

        // プレイヤーの移動を制御
        if (playerController != null)
        {
            if (isVisible)
            {
                // 描画モード時は移動と回転を無効化
                playerController.Acceleration = 0f;
                playerController.EnableRotation = false;
            }
            else
            {
                // 通常モード時はデフォルトに戻す
                playerController.Acceleration = defaultPlayerControllerAcceleration;
                playerController.EnableRotation = defaultEnableRotation;
            }
        }

        // Rayが表示されている場合は表示を切り替え
        if (raycastManager != null)
        {
            var rayVisualizer = raycastManager.transform.Find("RayVisualizer(Clone)");
            if (rayVisualizer != null)
            {
                rayVisualizer.gameObject.SetActive(isVisible);
            }
        }

        // コントローラーUIを更新
        if (controllerUIManager != null)
        {
            controllerUIManager.UpdateControllerUI(isLeftHanded, isVisible);
        }
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        // プレイモード時はチェックをスキップ
        if (Application.isPlaying)
            return;

        // プレハブインスタンス時のみチェックを行う
        if (!UnityEditor.PrefabUtility.IsPartOfPrefabAsset(gameObject))
        {
            // 必須コンポーネントのチェック
            if (metaQuestInput == null)
                Debug.LogError($"Missing MetaQuest3Input reference in {gameObject.name}");
            if (drawingUI == null)
                Debug.LogError($"Missing Drawing UI reference in {gameObject.name}");
            if (vrPen == null)
                Debug.LogError($"Missing VR Pen reference in {gameObject.name}");
            if (controllerUIManager == null)
                Debug.LogError($"Missing Controller UI Manager reference in {gameObject.name}");
            if (drawingUIPositionManager == null)
                Debug.LogError($"Missing DrawingUIPositionManager reference in {gameObject.name}");
        }
    }
#endif
}
