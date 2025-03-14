using UnityEngine;

public class VRPenController : MonoBehaviour
{
    [SerializeField] private bool isLeftHanded;
    [SerializeField] private MetaQuest3Input metaQuestInput;
    [SerializeField] private Transform raycastOrigin;
    [SerializeField] private VRPenModeManager penModeManager;
    [SerializeField] private VRPenDrawingManager drawingManager;
    [SerializeField] private VRPenRaycastManager raycastManager;
    [SerializeField] private VRPenStabilizer penStabilizer;
    [SerializeField] private Collider eraserCollider;

    private bool previousSecondaryButtonState = false; // 前フレームのボタン状態
    private MetaQuest3Controller activeController;

    private void Start()
    {
        InitializeComponents();
    }

    /// <summary>
    /// コンポーネントの初期化
    /// </summary>
    private void InitializeComponents()
    {
        raycastManager.Initialize(raycastOrigin, metaQuestInput);
        drawingManager.Initialize(raycastOrigin);
        if (penStabilizer != null)
        {
            penStabilizer.Initialize(metaQuestInput);
        }
    }

    private void Update()
    {
        // 利き手のコントローラを取得
        activeController = isLeftHanded ? metaQuestInput.LeftQuestController : metaQuestInput.RightQuestController;
        if (!activeController.IsValid) return;

        if (drawingManager == null) return;

        bool currentSecondaryButtonState = activeController.SecondaryButton;
        if (currentSecondaryButtonState && !previousSecondaryButtonState)
        {
            penModeManager.ToggleEraserMode();
        }
        previousSecondaryButtonState = currentSecondaryButtonState;

        bool isHitting = raycastManager.UpdateRaycast();

        if (penModeManager.IsEraserMode)
        {
            HandleErasing();
        }
        else
        {
            HandleDrawingInput(isHitting);
        }
    }

    /// <summary>
    /// 描画の入力を処理する
    /// </summary>
    /// <param name="isHitting"></param>
    private void HandleDrawingInput(bool isHitting)
    {

        // トリガーを離した時に描画を終了
        if (drawingManager.IsDrawing && !activeController.TriggerButton)
        {
            drawingManager.FinishDrawing(raycastManager.HitPoint, raycastOrigin.rotation);
            return;
        }

        // 描画開始・更新の条件：
        // 1. Canvasに接触していること（isHitting）
        // 2. トリガーが押されていること
        if (isHitting && activeController.TriggerButton)
        {
            if (!drawingManager.IsDrawing)
            {
                drawingManager.StartDrawing(raycastManager.HitPoint, raycastOrigin.rotation);
            }
            else
            {
                drawingManager.UpdateDrawing(raycastManager.HitPoint, raycastOrigin.rotation);
            }
        }
        else if (drawingManager.IsDrawing)
        {
            drawingManager.FinishDrawing(raycastManager.HitPoint, raycastOrigin.rotation);
        }
    }

    /// <summary>
    /// 消しゴムの処理
    /// </summary>
    private void HandleErasing()
    {
        int layerMask = 1 << LayerMask.NameToLayer("Line");

        Collider[] colliders = Physics.OverlapBox(
            eraserCollider.bounds.center,
            eraserCollider.bounds.extents,
            Quaternion.identity,
            layerMask
        );

        // 線に触れているかどうかを追跡
        bool isTouchingLines = colliders.Length > 0;

        // 線に触れている間は振動
        if (isTouchingLines)
        {
            activeController.SendHapticImpulse(0.1f, 0.05f);
        }

        foreach (Collider col in colliders)
        {
            // トリガーボタンを押したときのみ消去
            if (activeController.TriggerButton)
            {
                LineRenderer targetLine = col.GetComponent<LineRenderer>();
                if (targetLine == null)
                {
                    targetLine = col.transform.GetComponent<LineRenderer>();
                }

                if (targetLine != null)
                {
                    // completedLinesリストから削除
                    if (drawingManager != null)
                    {
                        drawingManager.EraseLine(targetLine.gameObject);
                    }
                    else
                    {
                        // drawingManagerが参照できない場合は直接削除
                        Destroy(targetLine.gameObject);
                    }

                    // 消去時の振動
                    //activeController.SendHapticImpulse(0.3f, 0.1f);
                }
            }
        }
    }


    private void OnDrawGizmos()
    {
        if (eraserCollider == null) return;

        Gizmos.color = Color.red; // 衝突範囲を赤色で可視化
        Gizmos.matrix = eraserCollider.transform.localToWorldMatrix;
        Gizmos.DrawWireCube(Vector3.zero, eraserCollider.bounds.size);
    }
}
