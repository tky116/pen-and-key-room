using System.Collections.Generic;
using UnityEngine;

public class VRPenStabilizer : MonoBehaviour
{
    [SerializeField] private bool isLeftHanded;
    [SerializeField] private Transform controllerAnchor;
    [SerializeField] private Transform rayOrigin;
    [SerializeField] private Rigidbody penRigidbody;

    [Header("Rotation Offset")]
    [SerializeField] private Vector3 positionOffset = Vector3.zero;
    [SerializeField] private Vector3 rotationOffset = new Vector3(-90f, 0f, 0f);

    [SerializeField] private float followSpeed = 100f;
    [SerializeField] private float maxVelocity = 10f;
    [SerializeField] private float damping = 5f;
    [SerializeField] private int stabilizationFrames = 10;
    [SerializeField] private float verticalSmoothingStrength = 0.8f;

    private Queue<Vector3> positionHistory;
    private MetaQuest3Input metaQuestInput;
    private bool wasHittingSurface;
    private bool isEraserMode = false;
    private Vector3 penRotation = new Vector3(-90f, 0f, 0f);
    private Vector3 eraserRotation = new Vector3(90f, 0f, 0f);

    public bool IsEraserMode => isEraserMode;

    public void Initialize(MetaQuest3Input input)
    {
        metaQuestInput = input;
        SetupRigidbody();
        positionHistory = new Queue<Vector3>();
    }

    /// <summary>
    /// Rigidbodyの設定
    /// </summary>
    private void SetupRigidbody()
    {
        penRigidbody.useGravity = false;
        penRigidbody.interpolation = RigidbodyInterpolation.Interpolate;
        penRigidbody.collisionDetectionMode = CollisionDetectionMode.Continuous;
        penRigidbody.constraints = RigidbodyConstraints.FreezeRotation;
        penRigidbody.linearDamping = damping;
    }

    private void FixedUpdate()
    {
        // 利き手のコントローラを取得
        var activeController = isLeftHanded ? metaQuestInput.LeftQuestController : metaQuestInput.RightQuestController;
        if (!activeController.IsValid) return;

        bool isHitting = CheckSurfaceContact();
        if (isHitting != wasHittingSurface)
        {
            activeController.SendHapticImpulse(0.5f, 0.1f);
            if (!isHitting)
            {
                positionHistory.Clear();
            }
        }
        wasHittingSurface = isHitting;

        UpdatePenPosition(isHitting);
    }

    /// <summary>
    /// サーフェスとの接触をチェックする
    /// </summary>
    /// <returns></returns>
    private bool CheckSurfaceContact()
    {
        return Physics.Raycast(rayOrigin.position, -transform.up, 0.01f);
    }

    /// <summary>
    /// ペンの位置を更新する
    /// </summary>
    /// <param name="isHitting"></param>
    private void UpdatePenPosition(bool isHitting)
    {
        Vector3 targetPosition = controllerAnchor.position + controllerAnchor.rotation * positionOffset;

        if (isHitting)
        {
            if (positionHistory.Count >= stabilizationFrames)
            {
                positionHistory.Dequeue();
            }
            positionHistory.Enqueue(targetPosition);

            Vector3 smoothedPosition = CalculateSmoothedPosition();
            targetPosition.y = Mathf.Lerp(targetPosition.y, smoothedPosition.y, verticalSmoothingStrength);
        }

        Vector3 moveDirection = targetPosition - penRigidbody.position;
        float distance = moveDirection.magnitude;

        if (distance > 0.001f)
        {
            Vector3 desiredVelocity = Vector3.ClampMagnitude(moveDirection.normalized * distance * followSpeed, maxVelocity);

            if (isHitting)
            {
                desiredVelocity.y = Mathf.Lerp(penRigidbody.linearVelocity.y, desiredVelocity.y, verticalSmoothingStrength);
            }

            Vector3 acceleration = (desiredVelocity - penRigidbody.linearVelocity) / Time.fixedDeltaTime;
            acceleration = Vector3.ClampMagnitude(acceleration, followSpeed * 2f);

            penRigidbody.linearVelocity += acceleration * Time.fixedDeltaTime;
        }
        else
        {
            penRigidbody.linearVelocity = Vector3.zero;
        }

        // 回転の更新
        Quaternion targetRotation = controllerAnchor.rotation * Quaternion.Euler(rotationOffset);
        penRigidbody.MoveRotation(targetRotation);
    }

    /// <summary>
    /// 平滑化された位置を計算する
    /// </summary>
    /// <returns></returns>
    private Vector3 CalculateSmoothedPosition()
    {
        if (positionHistory.Count == 0) return transform.position;

        Vector3 sum = Vector3.zero;
        foreach (Vector3 position in positionHistory)
        {
            sum += position;
        }
        return sum / positionHistory.Count;
    }

    /// <summary>
    /// 消しゴムモードの切り替え
    /// </summary>
    public void ToggleEraserMode()
    {
        isEraserMode = !isEraserMode;
        rotationOffset = isEraserMode ? eraserRotation : penRotation;
    }
}
