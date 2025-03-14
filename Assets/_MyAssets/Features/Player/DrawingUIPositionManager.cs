using UnityEngine;

/// <summary>
/// プレイヤーの位置に合わせてUIを配置する
/// </summary>
public class DrawingUIPositionManager : MonoBehaviour
{
    [Header("Reference Settings")]
    [SerializeField] private OVRCameraRig cameraRig;
    [SerializeField] private OVRPlayerController playerController;
    [SerializeField] private Transform targetUI;
    [SerializeField] private DrawingAnchorHeightAdjuster canvasAdjuster;

    [Header("Position Settings")]
    [SerializeField] private Vector3 uiOffset = new Vector3(0, 0, 2f);
    [SerializeField] private bool autoUpdatePosition = false;

    private Transform centerEyeAnchor;
    private bool isInitialized = false;
    private Vector3 lastPlayerPosition;
    private Quaternion lastPlayerRotation;

    private void Start()
    {
        Initialize();
    }

    private void Initialize()
    {
        if (!ValidateReferences()) return;

        centerEyeAnchor = cameraRig.centerEyeAnchor;
        if (centerEyeAnchor == null)
        {
            Debug.LogError("CenterEyeAnchor の取得に失敗しました。");
            return;
        }

        isInitialized = true;
        lastPlayerPosition = playerController.transform.position;
        lastPlayerRotation = playerController.transform.rotation;
        UpdateUIPosition();
    }

    private bool ValidateReferences()
    {
        if (cameraRig == null || targetUI == null || playerController == null || canvasAdjuster == null)
        {
            Debug.LogError($"必要な参照が不足しています: {gameObject.name}");
            return false;
        }
        return true;
    }

    private void Update()
    {
        if (!isInitialized || !autoUpdatePosition) return;

        // プレイヤーの移動または回転があった場合のみ更新
        if (HasPlayerMoved() || HasPlayerRotated())
        {
            UpdateUIPosition();
            lastPlayerPosition = playerController.transform.position;
            lastPlayerRotation = playerController.transform.rotation;
        }
    }

    private bool HasPlayerMoved()
    {
        return Vector3.Distance(lastPlayerPosition, playerController.transform.position) > 0.01f;
    }

    private bool HasPlayerRotated()
    {
        return Quaternion.Angle(lastPlayerRotation, playerController.transform.rotation) > 1f;
    }

    private void OnEnable()
    {
        if (isInitialized)
        {
            UpdateUIPosition();
        }
    }

    public void UpdateUIPosition()
    {
        if (!isInitialized) return;

        Vector3 playerForward = playerController.transform.forward;
        playerForward.y = 0;
        playerForward.Normalize();

        Vector3 targetPosition = playerController.transform.position +
                               (playerForward * uiOffset.z) +
                               (Vector3.up * (centerEyeAnchor.position.y - playerController.transform.position.y));

        targetUI.position = targetPosition;
        targetUI.rotation = Quaternion.LookRotation(playerForward);

        canvasAdjuster.UpdatePosition();
    }

    /// <summary>
    /// 自動更新を有効または無効にする
    /// </summary>
    /// <param name="enable"></param>
    public void SetAutoUpdate(bool enable)
    {
        autoUpdatePosition = enable;
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (cameraRig == null)
            Debug.LogError($"Missing OVRCameraRig reference in {gameObject.name}");
        if (playerController == null)
            Debug.LogError($"Missing OVRPlayerController reference in {gameObject.name}");
        if (targetUI == null)
            Debug.LogError($"Missing Target UI reference in {gameObject.name}");
        if (canvasAdjuster == null)
            Debug.LogError($"Missing DrawingAnchorHeightAdjuster reference in {gameObject.name}");
    }
#endif
}
