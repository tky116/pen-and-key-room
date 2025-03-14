using UnityEngine;

public class QuadNormalDebugger : MonoBehaviour
{
    private void OnDrawGizmos()
    {
        Vector3 quadPosition = transform.position;

        // 現在の法線方向（transform.up）
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(quadPosition, quadPosition + transform.up * 0.5f);
        Gizmos.DrawSphere(quadPosition + transform.up * 0.5f, 0.02f);

        // 期待される法線方向（Y軸）も表示して比較
        Gizmos.color = Color.green;
        Gizmos.DrawLine(quadPosition, quadPosition + Vector3.up * 0.5f);
        Gizmos.DrawSphere(quadPosition + Vector3.up * 0.5f, 0.02f);

        // デバッグ情報をシーンビューに表示
#if UNITY_EDITOR
        UnityEditor.Handles.Label(quadPosition,
            $"Rotation: {transform.rotation.eulerAngles}\n" +
            $"Normal (Up): {transform.up}\n" +
            $"Forward: {transform.forward}");
#endif
    }
}
