using UnityEngine;
using Oculus.Interaction;
using Cysharp.Threading.Tasks;
using System.Threading;

/// <summary>
/// アイテムのアイコンを表示
/// </summary>
public class KeyItemIndicator : MonoBehaviour
{
    [Header("Icon Settings")]
    [SerializeField] private Transform iconTransform;   // アイコンのTransform
    [SerializeField] private float floatHeight = 0.3f;  // アイコンの高さ
    [SerializeField] private Vector3 iconScale = new Vector3(0.1f, 0.1f, 1f);   // アイコンのスケール
    [SerializeField] private bool enableBobbing = true; // 上下運動を有効にする
    [SerializeField] private float bobbingAmount = 0.1f;    // 上下運動の量
    [SerializeField] private float bobbingSpeed = 2f;   // 上下運動の速度

    [Header("Fade Settings")]
    [SerializeField] private bool enableDistanceFade = true;    // 距離によるフェードを有効にする
    [SerializeField] private float fadeStartDistance = 3f;  // フェード開始距離
    [SerializeField] private float fadeEndDistance = 5f;    // フェード終了距離

    [Header("Visibility Settings")]
    [SerializeField] private float minVisibleAngle = 30f; // カメラとの角度がこれ以下なら表示
    [SerializeField] private bool hideWhenBehindObjects = true; // 物体越しの表示制御

    private bool isHeld = false;
    private PointableUnityEventWrapper pointableWrapper;
    private Camera mainCamera;
    private SpriteRenderer iconSpriteRenderer;
    private float bobTimer;
    private CancellationTokenSource _cts;
    private float showDelay = 0.1f;

    private void OnDestroy()
    {
        // TokenSourceの解放
        _cts?.Cancel();
        _cts?.Dispose();
    }

    private void Start()
    {
        mainCamera = Camera.main;
        SetupIcon();
        SetupPointableWrapper();
    }

    private void SetupIcon()
    {
        if (iconTransform != null)
        {
            iconTransform.localScale = iconScale;
            iconSpriteRenderer = iconTransform.GetComponent<SpriteRenderer>();
        }
    }

    private void SetupPointableWrapper()
    {
        pointableWrapper = GetComponent<PointableUnityEventWrapper>();
        if (pointableWrapper != null)
        {
            pointableWrapper.WhenSelect.AddListener(OnItemGrabbed);
            pointableWrapper.WhenUnselect.AddListener(OnItemReleased);
        }
    }

    private void Update()
    {
        if (!isHeld && iconTransform != null && iconTransform.gameObject.activeSelf)
        {
            UpdateIconPositionAndRotation();
            UpdateIconFade();
        }
    }

    /// <summary>
    /// アイコンの位置と向きを更新
    /// </summary>
    private void UpdateIconPositionAndRotation()
    {
        if (mainCamera == null || iconTransform == null) return;

        // ワールド座標の上方向を使用
        Vector3 upDirection = Vector3.up;
        float currentHeight = floatHeight;

        if (enableBobbing)
        {
            bobTimer += Time.deltaTime * bobbingSpeed;
            currentHeight += Mathf.Sin(bobTimer) * bobbingAmount;
        }

        // アイコンの位置を設定（ワールド座標の上方向を使用）
        iconTransform.position = transform.position + upDirection * currentHeight;

        // アイコンをカメラの方向に向ける
        Vector3 cameraToIcon = iconTransform.position - mainCamera.transform.position;
        iconTransform.rotation = Quaternion.LookRotation(-cameraToIcon, mainCamera.transform.up);
    }

    /// <summary>
    /// アイコンの表示を更新
    /// </summary>
    private void UpdateIconVisibility()
    {
        if (!iconTransform || !mainCamera) return;

        bool shouldShow = !isHeld;

        if (shouldShow && hideWhenBehindObjects)
        {
            // アイテムとカメラの間に物体があるか確認
            Vector3 directionToCamera = mainCamera.transform.position - transform.position;
            float distanceToCamera = directionToCamera.magnitude;
            Ray ray = new Ray(transform.position, directionToCamera.normalized);

            if (Physics.Raycast(ray, out RaycastHit hit, distanceToCamera))
            {
                if (hit.collider.gameObject != gameObject)
                {
                    shouldShow = false;
                }
            }
        }

        if (shouldShow)
        {
            // カメラとの角度をチェック
            Vector3 directionToCamera = (mainCamera.transform.position - transform.position).normalized;
            float angle = Vector3.Angle(Vector3.up, directionToCamera);
            shouldShow = angle <= minVisibleAngle;
        }

        iconTransform.gameObject.SetActive(shouldShow);
    }

    /// <summary>
    /// アイコンのフェードを更新
    /// </summary>
    private void UpdateIconFade()
    {
        if (enableDistanceFade && iconSpriteRenderer != null)
        {
            float distanceToCamera = Vector3.Distance(transform.position, mainCamera.transform.position);
            float alpha = 1f;

            if (distanceToCamera > fadeStartDistance)
            {
                alpha = Mathf.Lerp(1f, 0f, (distanceToCamera - fadeStartDistance) / (fadeEndDistance - fadeStartDistance));
            }

            Color iconColor = iconSpriteRenderer.color;
            iconColor.a = alpha;
            iconSpriteRenderer.color = iconColor;
        }
    }

    /// <summary>
    /// アイテムが掴まれた時の処理
    /// </summary>
    /// <param name="evt"></param>
    public void OnItemGrabbed(PointerEvent evt)
    {
        isHeld = true;
        if (iconTransform != null)
        {
            iconTransform.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// アイテムが離された時の処理
    /// </summary>
    /// <param name="evt"></param>
    public void OnItemReleased(PointerEvent evt)
    {
        isHeld = false;

        _cts?.Cancel();
        _cts?.Dispose();
        _cts = new CancellationTokenSource();

        // 遅延表示を開始
        ShowIconAfterDelayAsync(_cts.Token).Forget();
    }

    /// <summary>
    /// 指定時間待機してアイコンを表示
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    private async UniTaskVoid ShowIconAfterDelayAsync(CancellationToken cancellationToken)
    {
        await UniTask.Delay((int)(showDelay * 1000), cancellationToken: cancellationToken);

        // キャンセルされていない、かつアイテムが掴まれていない場合のみ表示
        if (!cancellationToken.IsCancellationRequested && !isHeld && iconTransform != null)
        {
            iconTransform.gameObject.SetActive(true);
        }
    }
}
