using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// VRペンの描画を管理するクラス
/// </summary>
public class VRPenDrawingManager : MonoBehaviour
{
    /// <summary>
    /// アクションの種類
    /// </summary>
    private enum ActionType
    {
        Draw,   // 描画
        Erase,  // 消去
        Delete  // 完全削除（Redoできない操作）
    }

    [SerializeField] private Material inkMaterial;
    [SerializeField] private LineRenderer lineRendererPrefab;
    [SerializeField] private TrailRenderer trailRendererPrefab;
    [SerializeField] private float inkWidth = 0.01f;
    [SerializeField] private Transform inkPoolSynced;
    [SerializeField] private MeshRenderer penBodyRenderer;

    [Header("Line Quality")]
    [SerializeField] private bool useBezierSmoothing = true;
    [SerializeField] private float minPointDistance = 0.0005f;
    [SerializeField] private float curveStrength = 0.4f;
    [SerializeField] private int smoothingSegments = 10;

    [Header("Time-based Interpolation")]
    [SerializeField] private float targetPointsPerSecond = 120f;

    [Header("Undo/Redo Settings")]
    [SerializeField] private int maxUndoSteps = 20;  // 最大Undoステップ数

    private TrailRenderer activeTrail;
    private List<LineRenderer> completedLines = new List<LineRenderer>();
    private List<Vector3> currentLinePoints = new List<Vector3>();
    private Vector3 lastRecordedPoint;
    private Color currentColor = Color.black;
    private bool isDrawing;
    private bool isClearingDrawings;
    private bool waitForTriggerRelease;
    private float timeSinceLastPoint = 0f;
    private Vector3 lastPosition;

    /// <summary>
    /// 線のデータ
    /// </summary>
    [System.Serializable]
    private class LineData
    {
        public Vector3[] points;
        public Color color;
        public float width;
        public bool wasDeleted;
        public string name;

        public LineData(LineRenderer line)
        {
            Vector3[] positions = new Vector3[line.positionCount];
            line.GetPositions(positions);
            this.points = positions;
            this.color = line.material.color;
            this.width = line.startWidth;
            this.name = line.gameObject.name;
            this.wasDeleted = false;
        }
    }

    /// <summary>
    /// 元に戻す、やり直しのアクション
    /// </summary>
    private class UndoRedoAction
    {
        public LineData lineData;
        public bool isErase;

        public UndoRedoAction(LineData data, bool erase)
        {
            lineData = data;
            isErase = erase;
        }
    }

    // Stack初期化時に最大サイズを指定
    private Stack<UndoRedoAction> undoStack;
    private Stack<UndoRedoAction> redoStack;

    public bool IsDrawing => isDrawing;
    public bool WaitForTriggerRelease => waitForTriggerRelease;

    // Undo/Redoの状態確認用プロパティ
    public bool CanUndo => undoStack?.Count > 0;
    public bool CanRedo => redoStack?.Count > 0;
    public int CurrentUndoCount => undoStack?.Count ?? 0;
    public int CurrentRedoCount => redoStack?.Count ?? 0;

    private void Awake()
    {
        undoStack = new Stack<UndoRedoAction>(maxUndoSteps);
        redoStack = new Stack<UndoRedoAction>(maxUndoSteps);
    }

    public void Initialize(Transform raycastOrigin)
    {
        if (penBodyRenderer != null)
        {
            penBodyRenderer.material.color = currentColor;
        }
    }

    /// <summary>
    /// 描画開始
    /// </summary>
    public void StartDrawing(Vector3 position, Quaternion rotation)
    {
        if (isClearingDrawings) return;

        redoStack.Clear();
        isDrawing = true;
        currentLinePoints.Clear();

        GameObject trailObj = Instantiate(trailRendererPrefab.gameObject, inkPoolSynced);
        trailObj.name = "ActiveTrail";
        activeTrail = trailObj.GetComponent<TrailRenderer>();
        activeTrail.startWidth = inkWidth;
        activeTrail.endWidth = inkWidth;
        Material trailMaterial = new Material(inkMaterial);
        trailMaterial.color = currentColor;
        activeTrail.material = trailMaterial;

        Vector3 localPosition = inkPoolSynced.InverseTransformPoint(position);
        localPosition.z = 0f;

        lastPosition = localPosition;
        lastRecordedPoint = localPosition;
        currentLinePoints.Add(lastRecordedPoint);

        activeTrail.transform.localPosition = lastRecordedPoint;
        activeTrail.transform.localRotation = Quaternion.identity;

        timeSinceLastPoint = 0f;
    }

    /// <summary>
    /// 描画更新
    /// </summary>
    public void UpdateDrawing(Vector3 position, Quaternion rotation)
    {
        if (!isDrawing || activeTrail == null) return;

        Vector3 localPosition = inkPoolSynced.InverseTransformPoint(position);
        localPosition.z = 0f;

        timeSinceLastPoint += Time.deltaTime;
        float targetTimeInterval = 1f / targetPointsPerSecond;
        int pointsToAdd = Mathf.FloorToInt(timeSinceLastPoint / targetTimeInterval);
        pointsToAdd = Mathf.Min(pointsToAdd, 20);

        if (pointsToAdd > 0)
        {
            float currentDistance = Vector3.Distance(lastPosition, localPosition);
            float speed = currentDistance / Time.deltaTime;
            float speedMultiplier = Mathf.Clamp(speed / 3.0f, 0.2f, 1.0f);

            const float maxMovement = 0.1f;
            if (currentDistance > maxMovement)
            {
                localPosition = lastPosition + Vector3.ClampMagnitude(localPosition - lastPosition, maxMovement);
                currentDistance = maxMovement;
            }

            Vector3 direction = currentDistance > 0.0001f ?
                (localPosition - lastPosition) / currentDistance : Vector3.zero;

            float controlFactor = Mathf.Lerp(0.2f, 0.5f, speedMultiplier);
            Vector3 control1 = lastPosition + direction * currentDistance * controlFactor;
            Vector3 control2 = localPosition - direction * currentDistance * controlFactor;

            float baseMinDistance = minPointDistance * 0.5f;
            float speedLerpFactor = 1f - speedMultiplier;

            for (int i = 1; i <= pointsToAdd; i++)
            {
                float t = (float)i / pointsToAdd;
                t = Mathf.Lerp(t, EaseInOutQuad(t), Mathf.Clamp01(speedMultiplier * 1.5f));

                Vector3 dynamicControl1 = Vector3.Lerp(lastPosition, control1, speedLerpFactor);
                Vector3 dynamicControl2 = Vector3.Lerp(localPosition, control2, speedLerpFactor);

                Vector3 interpolatedPosition = CalculateCubicBezierPoint(t, lastRecordedPoint, dynamicControl1, dynamicControl2, localPosition);
                interpolatedPosition.z = 0f;

                float distanceFromLast = Vector3.Distance(lastRecordedPoint, interpolatedPosition);
                float dynamicMinDistance = Mathf.Lerp(baseMinDistance, minPointDistance, speedMultiplier);

                if (distanceFromLast >= dynamicMinDistance)
                {
                    currentLinePoints.Add(interpolatedPosition);
                    lastRecordedPoint = interpolatedPosition;
                    activeTrail.transform.localPosition = lastRecordedPoint;
                }
            }

            timeSinceLastPoint -= pointsToAdd * targetTimeInterval;
            timeSinceLastPoint = Mathf.Max(0f, timeSinceLastPoint);
        }

        lastPosition = localPosition;
    }

    private float EaseInOutQuad(float t)
    {
        return t < 0.5f ? 2f * t * t : -1f + (4f - 2f * t) * t;
    }

    /// <summary>
    /// 描画終了
    /// </summary>
    public void FinishDrawing(Vector3 position, Quaternion rotation)
    {
        if (!isDrawing) return;
        isDrawing = false;

        if (position != inkPoolSynced.TransformPoint(lastRecordedPoint))
        {
            currentLinePoints.Add(inkPoolSynced.InverseTransformPoint(position));
        }

        if (currentLinePoints.Count < 2)
        {
            if (activeTrail != null)
            {
                activeTrail.time = 0f;
                activeTrail.Clear();
                Destroy(activeTrail.gameObject);
                activeTrail = null;
            }
            return;
        }

        GameObject lineObj = Instantiate(lineRendererPrefab.gameObject, inkPoolSynced);
        lineObj.name = $"DrawnLine_{completedLines.Count}";
        lineObj.layer = LayerMask.NameToLayer("Line");

        LineRenderer lineRenderer = lineObj.GetComponent<LineRenderer>();
        Material lineMaterial = new Material(inkMaterial);
        lineMaterial.color = currentColor;
        lineRenderer.material = lineMaterial;
        lineRenderer.startWidth = inkWidth;
        lineRenderer.endWidth = inkWidth;

        Vector3[] smoothedLocalPoints = CalculateSmoothLine(currentLinePoints);
        lineRenderer.positionCount = smoothedLocalPoints.Length;
        lineRenderer.SetPositions(smoothedLocalPoints);

        // メッシュコライダーの設定
        SetupMeshCollider(lineObj, lineRenderer);

        // PushUndoActionを使用
        LineData lineData = new LineData(lineRenderer);
        PushUndoAction(new UndoRedoAction(lineData, false));

        completedLines.Add(lineRenderer);

        if (activeTrail != null)
        {
            activeTrail.time = 0f;
            activeTrail.Clear();
            Destroy(activeTrail.gameObject);
            activeTrail = null;
        }
    }

    /// <summary>
    /// アクションをUndoスタックに追加
    /// </summary>
    private void PushUndoAction(UndoRedoAction action, ActionType type = ActionType.Draw)
    {
        // 完全削除操作の場合は両方のスタックをクリア
        if (type == ActionType.Delete)
        {
            undoStack.Clear();
            redoStack.Clear();
            return;
        }

        // 通常の操作（描画/消去）の場合
        if (undoStack.Count >= maxUndoSteps)
        {
            var tempArray = undoStack.ToArray();
            undoStack.Clear();

            for (int i = 0; i < tempArray.Length - 1; i++)
            {
                undoStack.Push(tempArray[tempArray.Length - 2 - i]);
            }
        }

        undoStack.Push(action);

        // 新しい描画/消去操作が行われた場合のみRedoスタックをクリア
        if (type != ActionType.Delete)
        {
            redoStack.Clear();
        }
    }

    /// <summary>
    /// ベジェ曲線を使ったスムージング
    /// </summary>
    private Vector3[] CalculateSmoothLine(List<Vector3> points)
    {
        if (!useBezierSmoothing || points.Count < 3)
            return points.ToArray();

        List<Vector3> smoothedPoints = new List<Vector3>();
        smoothedPoints.Add(points[0]);

        for (int i = 1; i < points.Count - 1; i++)
        {
            Vector3 p0 = points[i - 1];
            Vector3 p1 = points[i];
            Vector3 p2 = points[i + 1];

            Vector3 dir1 = (p1 - p0).normalized;
            Vector3 dir2 = (p2 - p1).normalized;

            float dot = Vector3.Dot(dir1, dir2);
            float curveMultiplier = Mathf.Lerp(curveStrength * 1.5f, curveStrength, (dot + 1f) * 0.5f);

            Vector3 control1 = p1 + dir1 * Vector3.Distance(p0, p1) * curveMultiplier;
            Vector3 control2 = p1 + dir2 * Vector3.Distance(p1, p2) * curveMultiplier;

            for (int j = 0; j <= smoothingSegments; j++)
            {
                float t = j / (float)smoothingSegments;
                Vector3 smoothedPoint = CalculateCubicBezierPoint(t, p0, control1, control2, p2);
                smoothedPoints.Add(smoothedPoint);
            }
        }

        smoothedPoints.Add(points[points.Count - 1]);
        return smoothedPoints.ToArray();
    }

    /// <summary>
    /// 3次ベジェ曲線の計算
    /// </summary>
    private Vector3 CalculateCubicBezierPoint(float t, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
    {
        float u = 1 - t;
        float tt = t * t;
        float uu = u * u;
        float uuu = uu * u;
        float ttt = tt * t;

        return (uuu * p0) + (3 * uu * t * p1) + (3 * u * tt * p2) + (ttt * p3);
    }

    /// <summary>
    /// データから線を生成
    /// </summary>
    private LineRenderer CreateLineFromData(LineData data)
    {
        GameObject lineObj = Instantiate(lineRendererPrefab.gameObject, inkPoolSynced);
        lineObj.name = data.name;
        lineObj.layer = LayerMask.NameToLayer("Line");

        LineRenderer lineRenderer = lineObj.GetComponent<LineRenderer>();
        Material lineMaterial = new Material(inkMaterial);
        lineMaterial.color = data.color;
        lineRenderer.material = lineMaterial;
        lineRenderer.startWidth = data.width;
        lineRenderer.endWidth = data.width;

        // データはすでにローカル座標なので、そのまま設定
        lineRenderer.positionCount = data.points.Length;
        lineRenderer.SetPositions(data.points);

        // メッシュコライダーの設定
        SetupMeshCollider(lineObj, lineRenderer);

        return lineRenderer;
    }

    /// <summary>
    /// メッシュコライダーのセットアップ
    /// </summary>
    private void SetupMeshCollider(GameObject lineObj, LineRenderer lineRenderer)
    {
        bool originalUseWorldSpace = lineRenderer.useWorldSpace;
        lineRenderer.useWorldSpace = true;

        Vector3[] worldPoints = new Vector3[lineRenderer.positionCount];
        for (int i = 0; i < lineRenderer.positionCount; i++)
        {
            worldPoints[i] = lineObj.transform.TransformPoint(lineRenderer.GetPosition(i));
        }
        lineRenderer.SetPositions(worldPoints);

        Mesh mesh = new Mesh();
        lineRenderer.BakeMesh(mesh, true);

        Vector3[] vertices = mesh.vertices;
        for (int i = 0; i < vertices.Length; i++)
        {
            vertices[i] = lineObj.transform.InverseTransformPoint(vertices[i]);
        }
        mesh.vertices = vertices;
        mesh.RecalculateBounds();

        MeshCollider meshCollider = lineObj.GetComponent<MeshCollider>();
        if (meshCollider == null)
        {
            meshCollider = lineObj.AddComponent<MeshCollider>();
        }
        meshCollider.sharedMesh = mesh;
        meshCollider.convex = false;

        lineRenderer.useWorldSpace = originalUseWorldSpace;
        if (!originalUseWorldSpace)
        {
            Vector3[] localPoints = new Vector3[worldPoints.Length];
            for (int i = 0; i < worldPoints.Length; i++)
            {
                localPoints[i] = lineObj.transform.InverseTransformPoint(worldPoints[i]);
            }
            lineRenderer.SetPositions(localPoints);
        }
    }

    /// <summary>
    /// ペンの色を設定
    /// </summary>
    public void SetColor(Color color)
    {
        currentColor = color;
        if (penBodyRenderer != null) penBodyRenderer.material.color = color;
        if (activeTrail != null) activeTrail.material.color = color;
    }

    /// <summary>
    /// 線を消去
    /// </summary>
    public void EraseLine(GameObject line)
    {
        LineRenderer lineRenderer = line.GetComponent<LineRenderer>();
        if (lineRenderer != null && completedLines.Contains(lineRenderer))
        {
            LineData lineData = new LineData(lineRenderer);
            lineData.wasDeleted = true;
            PushUndoAction(new UndoRedoAction(lineData, true), ActionType.Erase);

            completedLines.Remove(lineRenderer);
            Destroy(lineRenderer.gameObject);
        }
    }

    /// <summary>
    /// 最後に描画された線を元に戻す
    /// </summary>
    public void UndoLastDrawing()
    {
        if (!CanUndo)
        {
            Debug.Log("Can't Undo - Stack empty");
            return;
        }

        UndoRedoAction lastAction = undoStack.Pop();
        Debug.Log($"Undo Action - isErase: {lastAction.isErase}, LineData: {lastAction.lineData.name}");

        if (lastAction.isErase)
        {
            // 消去操作を取り消す → 線を復元
            LineRenderer newLine = CreateLineFromData(lastAction.lineData);
            completedLines.Add(newLine);
            redoStack.Push(new UndoRedoAction(lastAction.lineData, true));
        }
        else
        {
            // 描画操作を取り消す → 線を削除
            LineRenderer lineToRemove = completedLines.Find(line => line.gameObject.name == lastAction.lineData.name);
            if (lineToRemove != null)
            {
                completedLines.Remove(lineToRemove);
                Destroy(lineToRemove.gameObject);
                redoStack.Push(new UndoRedoAction(lastAction.lineData, false));
            }


        }

        // Redoスタックが上限を超えた場合、古いものを削除
        if (redoStack.Count > maxUndoSteps)
        {
            var tempArray = redoStack.ToArray();
            redoStack.Clear();

            // 新しい操作から順にPush（最も古い操作を除外）
            for (int i = 0; i < maxUndoSteps; i++)
            {
                redoStack.Push(tempArray[i]);
            }
        }
    }

    /// <summary>
    /// 最後に元に戻した線を再度描画
    /// </summary>
    public void RedoLastDrawing()
    {
        if (!CanRedo)
        {
            Debug.Log("Can't Redo - Stack empty");
            return;
        }

        UndoRedoAction redoAction = redoStack.Pop();
        Debug.Log($"Redo Action - isErase: {redoAction.isErase}, LineData: {(redoAction.lineData != null ? redoAction.lineData.name : "null")}");

        if (redoAction.isErase)
        {
            Debug.Log("Attempting to redo erase operation");
            // 消去操作をやり直す → 線を削除
            LineRenderer lineToRemove = completedLines.Find(line => line.gameObject.name == redoAction.lineData.name);
            if (lineToRemove != null)
            {
                completedLines.Remove(lineToRemove);
                Destroy(lineToRemove.gameObject);
                undoStack.Push(new UndoRedoAction(redoAction.lineData, true));
            }
        }
        else
        {
            Debug.Log("Attempting to redo draw operation");
            // 描画操作をやり直す → 線を復元
            LineRenderer newLine = CreateLineFromData(redoAction.lineData);
            completedLines.Add(newLine);
            undoStack.Push(new UndoRedoAction(redoAction.lineData, false));
        }

        Debug.Log($"After Redo - Undo Count: {undoStack.Count}, Redo Count: {redoStack.Count}");

        // Undoスタックが上限を超えた場合、古いものを削除
        if (undoStack.Count > maxUndoSteps)
        {
            var tempArray = undoStack.ToArray();
            undoStack.Clear();

            // 新しい操作から順にPush（最も古い操作を除外）
            for (int i = 0; i < maxUndoSteps; i++)
            {
                undoStack.Push(tempArray[i]);
            }
        }
    }

    /// <summary>
    /// 全ての描画を消去
    /// </summary>
    public void ClearAllDrawings()
    {
        isClearingDrawings = true;

        if (isDrawing)
        {
            isDrawing = false;
            if (activeTrail != null)
            {
                Destroy(activeTrail.gameObject);
                activeTrail = null;
            }
        }

        foreach (LineRenderer line in completedLines)
        {
            if (line != null && line.gameObject != null)
            {
                Destroy(line.gameObject);
            }
        }

        completedLines.Clear();
        currentLinePoints.Clear();

        // Clear All は完全削除操作として扱う
        PushUndoAction(null, ActionType.Delete);

        isClearingDrawings = false;
        waitForTriggerRelease = true;
    }
}
