using UnityEngine;

public class DrawingAnchorHeightAdjuster : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform centerEyeAnchor;
    [SerializeField] private Transform drawingCanvas;
    [SerializeField] private Transform penTip;

    [Header("Canvas Settings")]
    [SerializeField] private Vector3 canvasOffset = new Vector3(0, -0.425f, 0f);
    [SerializeField] private float heightAdjustmentLimit = 0.1f;

    private float initialHeight;

    private void Start()
    {
        if (centerEyeAnchor != null)
        {
            initialHeight = canvasOffset.y;
            UpdatePosition();
        }
    }

    public void UpdatePosition()
    {
        if (drawingCanvas != null && centerEyeAnchor != null)
        {
            Vector3 basePosition = centerEyeAnchor.position;

            // 親オブジェクトのローカル座標系でのオフセットを計算
            Vector3 localOffset = drawingCanvas.parent.InverseTransformDirection(
                centerEyeAnchor.forward * canvasOffset.z +
                Vector3.up * canvasOffset.y +
                centerEyeAnchor.right * canvasOffset.x
            );

            drawingCanvas.localPosition = localOffset;
        }
    }

    public void CalibrateCanvas()
    {
        if (penTip != null && drawingCanvas != null && centerEyeAnchor != null)
        {
            float relativeHeight = penTip.position.y - centerEyeAnchor.position.y;
            float clampedHeight = Mathf.Clamp(
                relativeHeight,
                initialHeight - heightAdjustmentLimit,
                initialHeight + heightAdjustmentLimit
            );
            canvasOffset.y = clampedHeight;
            UpdatePosition();
        }
    }
}
