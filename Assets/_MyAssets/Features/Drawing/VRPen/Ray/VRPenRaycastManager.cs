using UnityEngine;

public class VRPenRaycastManager : MonoBehaviour
{
    private const string DRAWING_SURFACE_LAYER = "DrawingSurface";

    [SerializeField] private bool isLeftHanded;
    [SerializeField] private float rayLength = 0.01f;
    [SerializeField] private int rayChecksPerFrame = 3;
    [SerializeField] private GameObject rayVisualizerPrefab;

    private LineRenderer rayVisualizer;
    private Transform raycastOrigin;
    private MetaQuest3Input metaQuestInput;
    private bool wasHitting;

    public Vector3 HitPoint { get; private set; }

    public void Initialize(Transform origin, MetaQuest3Input input)
    {
        raycastOrigin = origin;
        metaQuestInput = input;
        if (rayVisualizer != null)
        {
            Destroy(rayVisualizer.gameObject);
            rayVisualizer = null;
        }
        SetupRayVisualizer();
    }

    /// <summary>
    /// レイキャストのセットアップ
    /// </summary>
    private void SetupRayVisualizer()
    {
        if (rayVisualizerPrefab != null)
        {
            var visualizer = Instantiate(rayVisualizerPrefab, transform);
            rayVisualizer = visualizer.GetComponent<LineRenderer>();
        }
    }

    /// <summary>
    /// レイキャストの更新
    /// </summary>
    /// <returns></returns>
    public bool UpdateRaycast()
    {
        Vector3 startPos = raycastOrigin.position;
        Vector3 direction = -raycastOrigin.forward;
        float stepLength = rayLength / rayChecksPerFrame;

        if (rayVisualizer != null)
        {
            rayVisualizer.SetPosition(0, raycastOrigin.position);
            rayVisualizer.SetPosition(1, raycastOrigin.position + direction * rayLength);
        }

        bool isHitting = false;
        for (int i = 0; i < rayChecksPerFrame; i++)
        {
            Vector3 currentPos = startPos + direction * (stepLength * i);
            Ray ray = new Ray(currentPos, direction);
            if (Physics.Raycast(ray, out RaycastHit hit, stepLength, LayerMask.GetMask(DRAWING_SURFACE_LAYER)))
            {
                if (Vector3.Dot(hit.normal, direction) < 0)
                {
                    HitPoint = hit.point;
                    isHitting = true;
                    break;
                }
            }
        }

        if (isHitting != wasHitting)
        {
            MetaQuest3Controller activeController = isLeftHanded
                ? metaQuestInput.LeftQuestController
                : metaQuestInput.RightQuestController;

            activeController.SendHapticImpulse(0.5f, 0.1f);
        }
        wasHitting = isHitting;

        return isHitting;
    }

    private void OnDestroy()
    {
        if (rayVisualizer != null)
        {
            Destroy(rayVisualizer.gameObject);
        }
    }
}
