using UnityEngine;

/// <summary>
/// UIの位置を管理する基底クラス
/// </summary>
public abstract class BaseUIPositionManager : MonoBehaviour
{
    [Header("Reference Settings")]
    [SerializeField] protected OVRCameraRig cameraRig;
    [SerializeField] protected Transform targetUI;

    [Header("UI Position Settings")]
    [SerializeField] protected Vector3 offsetFromCamera = new Vector3(0, 0, 2f);
    [SerializeField] protected bool followPlayer = true;
    [SerializeField] protected bool lookAtPlayer = true;

    protected Transform centerEyeAnchor;
    protected bool isInitialized = false;

    public virtual void Initialize()
    {
        if (cameraRig == null)
        {
            Debug.LogError("OVRCameraRig の参照が見つかりません。");
            return;
        }

        centerEyeAnchor = cameraRig.centerEyeAnchor;

        if (centerEyeAnchor == null)
        {
            Debug.LogError("CenterEyeAnchor の取得に失敗しました。");
            return;
        }

        if (targetUI == null)
        {
            Debug.LogError("Target UI の参照が見つかりません。");
            return;
        }

        isInitialized = true;
        UpdatePosition();
    }

    protected virtual void Start()
    {
        if (!isInitialized)
        {
            Initialize();
        }
    }

    protected virtual void LateUpdate()
    {
        if (!followPlayer || centerEyeAnchor == null || targetUI == null) return;
        UpdatePosition();
    }

    protected virtual void UpdatePosition()
    {
        if (centerEyeAnchor == null || targetUI == null) return;

        Vector3 basePosition = GetBasePosition();

        Vector3 targetPosition = basePosition +
                               (centerEyeAnchor.forward * offsetFromCamera.z) +
                               (centerEyeAnchor.up * offsetFromCamera.y) +
                               (centerEyeAnchor.right * offsetFromCamera.x);

        targetUI.position = targetPosition;

        if (lookAtPlayer)
        {
            targetUI.rotation = Quaternion.LookRotation(
                targetUI.position - centerEyeAnchor.position
            );
        }
    }

    // 派生クラスで基準位置の取得方法を実装
    public abstract Vector3 GetBasePosition();
}
