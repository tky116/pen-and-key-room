using UnityEngine;

[RequireComponent(typeof(MeshCollider))]
public class InkColliderGizmos : MonoBehaviour
{
    private MeshCollider meshCollider;

    private void OnDrawGizmos()
    {
        meshCollider = GetComponent<MeshCollider>();
        if (meshCollider == null || meshCollider.sharedMesh == null) return;

        Gizmos.color = Color.green; // 緑色で表示
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.DrawWireMesh(meshCollider.sharedMesh);
    }
}
