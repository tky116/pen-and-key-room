using UnityEngine;

/// <summary>
/// ダイアログUIの位置を管理するクラス
/// </summary>
public class DialogueUIPositionManager : MonoBehaviour
{
    [Header("Reference Settings")]
    [SerializeField] private OVRPlayerController playerController;
    [SerializeField] private OVRCameraRig cameraRig;
    [SerializeField] private Transform targetUI;
    [Header("Position Settings")]
    [SerializeField] private Vector3 uiOffset = new Vector3(0, -0.275f, 0.5f);
    [SerializeField] private bool autoUpdatePosition = true;
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
        if (cameraRig == null || targetUI == null || playerController == null)
        {
            Debug.LogError($"必要な参照が不足しています: {gameObject.name}");
            return false;
        }
        return true;
    }
    private void Update()
    {
        if (!isInitialized || !autoUpdatePosition) return;
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
        // プレイヤーの前方向を取得（Y軸の回転のみを考慮）
        Vector3 playerForward = playerController.transform.forward;
        playerForward.y = 0;
        playerForward.Normalize();
        // プレイヤーの位置から、高さはカメラの高さを使用
        Vector3 targetPosition = playerController.transform.position +
                               (playerForward * uiOffset.z) +
                               (Vector3.up * (centerEyeAnchor.position.y - playerController.transform.position.y + uiOffset.y)) +
                               (playerController.transform.right * uiOffset.x);
        targetUI.position = targetPosition;
        // プレイヤーの向きに合わせてUIを回転（Y軸のみ）
        targetUI.rotation = Quaternion.LookRotation(playerForward);
    }
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
    }
#endif
}
