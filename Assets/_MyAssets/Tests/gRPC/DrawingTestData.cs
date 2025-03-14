#if UNITY_EDITOR || DEVELOPMENT_BUILD

using UnityEngine;
using System.Collections.Generic;
using System;
using Cysharp.Threading.Tasks;
using VRAcademyAudition;
using ProtoColor = VRAcademyAudition.Color;

/// <summary>
/// テスト用の描画データを送信する
/// </summary>
public class DrawingTestData : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GrpcClient grpcClient;

    [Header("Test Settings")]
    [SerializeField] private string testSceneId = "GrpcTest";

    [Header("Test Data Colors")]
    [SerializeField] private UnityEngine.Color squareColor = new UnityEngine.Color(1f, 0f, 0f, 1f);    // 赤

    [SerializeField] private UnityEngine.Color circleColor = new UnityEngine.Color(0f, 1f, 0f, 1f);    // 緑
    [SerializeField] private UnityEngine.Color triangleColor = new UnityEngine.Color(0f, 0f, 1f, 1f);  // 青
    [SerializeField] private UnityEngine.Color keyColor = new UnityEngine.Color(1f, 1f, 0f, 1f);       // 黄

    private bool isSending = false;
    private string currentSceneId;

    private void Start()
    {
        currentSceneId = grpcClient.SceneId;
    }

    private void Update()
    {
        if (isSending) return;

        // シーンID切り替え
        if (Input.GetKeyDown(KeyCode.T))
        {
            ToggleSceneId();
        }

        // テストデータ送信
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            SendSquareTestDataAsync().Forget();
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            SendCircleTestDataAsync().Forget();
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            SendTriangleTestDataAsync().Forget();
        }
        else if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            SendKeyTestDataAsync().Forget();
        }

        // クリア
        if (Input.GetKeyDown(KeyCode.Space))
        {
            ClearTestObjects();
        }
    }

    /// <summary>
    /// シーンIDを切り替える
    /// </summary>
    private void ToggleSceneId()
    {
        if (grpcClient.SceneId == currentSceneId)
        {
            grpcClient.SceneId = testSceneId;
            Debug.Log($"シーンIDをテスト用に変更: {testSceneId}");
        }
        else
        {
            grpcClient.SceneId = currentSceneId;
            Debug.Log($"シーンIDを元に戻す: {currentSceneId}");
        }
    }

    /// <summary>
    /// 四角形テストデータを送信
    /// </summary>
    /// <returns></returns>
    private async UniTaskVoid SendSquareTestDataAsync()
    {
        Debug.Log("四角形テストデータの送信開始");

        try
        {
            var squarePoints = new List<Vector3>
            {
                new Vector3(-0.5f, 0.5f, 0f),   // 左上
                new Vector3(0.5f, 0.5f, 0f),    // 右上
                new Vector3(0.5f, -0.5f, 0f),   // 右下
                new Vector3(-0.5f, -0.5f, 0f),  // 左下
                new Vector3(-0.5f, 0.5f, 0f)    // 左上（閉じる）
            };

            await SendTestDataAsync(squarePoints, "square", squareColor);
        }
        catch (Exception ex)
        {
            Debug.LogError($"四角形テストデータ送信エラー: {ex.Message}");
        }
    }

    /// <summary>
    /// 円形テストデータを送信
    /// </summary>
    /// <returns></returns>
    private async UniTaskVoid SendCircleTestDataAsync()
    {
        Debug.Log("円形テストデータの送信開始");

        try
        {
            var points = new List<Vector3>();
            int segments = 32;
            float radius = 0.5f;

            for (int i = 0; i <= segments; i++)
            {
                float angle = (2 * Mathf.PI * i) / segments;
                float x = Mathf.Cos(angle) * radius;
                float y = Mathf.Sin(angle) * radius;
                points.Add(new Vector3(x, y, 0));
            }

            await SendTestDataAsync(points, "circle", circleColor);
        }
        catch (Exception ex)
        {
            Debug.LogError($"円形テストデータ送信エラー: {ex.Message}");
        }
    }

    /// <summary>
    /// 三角形テストデータを送信
    /// </summary>
    /// <returns></returns>
    private async UniTaskVoid SendTriangleTestDataAsync()
    {
        Debug.Log("三角形テストデータの送信開始");

        try
        {
            var points = new List<Vector3>
            {
                new Vector3(0, 0.5f, 0),        // 上
                new Vector3(-0.5f, -0.5f, 0),   // 左下
                new Vector3(0.5f, -0.5f, 0),    // 右下
                new Vector3(0, 0.5f, 0)         // 上（閉じる）
            };

            await SendTestDataAsync(points, "triangle", triangleColor);
        }
        catch (Exception ex)
        {
            Debug.LogError($"三角形テストデータ送信エラー: {ex.Message}");
        }
    }

    /// <summary>
    /// 鍵テストデータを送信
    /// </summary>
    /// <returns></returns>
    private async UniTaskVoid SendKeyTestDataAsync()
    {
        Debug.Log("鍵テストデータの送信開始");

        try
        {
            var points = new List<Vector3>
            {
                // 鍵の形状
                // 鍵の頭部（円）
                new Vector3(0f, 0.4f, 0f),
                new Vector3(0.1f, 0.35f, 0f),
                new Vector3(0.15f, 0.25f, 0f),
                new Vector3(0.1f, 0.15f, 0f),
                new Vector3(0f, 0.1f, 0f),
                new Vector3(-0.1f, 0.15f, 0f),
                new Vector3(-0.15f, 0.25f, 0f),
                new Vector3(-0.1f, 0.35f, 0f),
                new Vector3(0f, 0.4f, 0f),
    
                // 鍵の軸部
                new Vector3(0f, 0.1f, 0f),
                new Vector3(0f, -0.3f, 0f),
    
                // 鍵の歯部
                new Vector3(0.1f, -0.3f, 0f),
                new Vector3(0.1f, -0.2f, 0f),
                new Vector3(0f, -0.2f, 0f)
            };

            await SendTestDataAsync(points, "key", keyColor);
        }
        catch (Exception ex)
        {
            Debug.LogError($"鍵テストデータ送信エラー: {ex.Message}");
        }
    }

    /// <summary>
    /// テストデータを送信
    /// </summary>
    /// <param name="points"></param>
    /// <param name="shapeName"></param>
    /// <param name="color"></param>
    /// <returns></returns>
    private async UniTask SendTestDataAsync(List<Vector3> points, string shapeName, UnityEngine.Color color)
    {
        isSending = true;
        GameObject lineObject = null;

        try
        {
            // テストオブジェクトの作成
            lineObject = new GameObject($"Test_{shapeName}");
            lineObject.tag = "TestDrawing";

            var lineRenderer = lineObject.AddComponent<LineRenderer>();
            lineRenderer.positionCount = points.Count;
            lineRenderer.SetPositions(points.ToArray());
            lineRenderer.startWidth = 0.01f;
            lineRenderer.endWidth = 0.01f;
            lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
            lineRenderer.material.color = color;

            // 中心点を計算
            Vector3 center = CalculateCenter(points);

            // データ送信
            Debug.Log($"テストデータ送信開始 - 形状: {shapeName}");
            Debug.Log($"  シーン: {grpcClient.SceneId}");
            Debug.Log($"  AI処理: {grpcClient.UseAi}");

            var uploadId = await grpcClient.SendDrawingData(new[] { lineRenderer }, center);

            if (!string.IsNullOrEmpty(uploadId))
            {
                Debug.Log($"テストデータ送信完了 - 形状: {shapeName}, UploadID: {uploadId}");
            }
            else
            {
                Debug.LogWarning($"テストデータ送信に問題が発生しました - 形状: {shapeName}");
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"テストデータ送信エラー - 形状: {shapeName}, エラー: {ex.Message}");
            throw;
        }
        finally
        {
            if (lineObject != null)
            {
                Destroy(lineObject);
            }
            isSending = false;
        }
    }

    /// <summary>
    /// ポイントリストから中心点を計算
    /// </summary>
    /// <param name="points"></param>
    /// <returns></returns>
    private Vector3 CalculateCenter(List<Vector3> points)
    {
        if (points == null || points.Count == 0)
            return Vector3.zero;

        Vector3 sum = Vector3.zero;
        foreach (var point in points)
        {
            sum += point;
        }
        return sum / points.Count;
    }

    /// <summary>
    /// テストオブジェクトをクリア
    /// </summary>
    private void ClearTestObjects()
    {
        Debug.Log("テストオブジェクトをクリア");
        var testObjects = GameObject.FindGameObjectsWithTag("TestDrawing");
        foreach (var obj in testObjects)
        {
            Destroy(obj);
        }
    }

    private void OnGUI()
    {
        GUIStyle style = new GUIStyle(GUI.skin.label)
        {
            fontSize = 14,
            normal = { textColor = UnityEngine.Color.white }
        };

        GUI.Label(new Rect(10, 10, 300, 200),
            "テスト操作:\n" +
            "1: 四角形テスト (赤)\n" +
            "2: 円形テスト (緑)\n" +
            "3: 三角形テスト (青)\n" +
            "4: 鍵テスト（黄）\n" +
            "T: シーンID切り替え\n" +
            "Space: クリア\n\n" +
            $"現在のシーン: {grpcClient.SceneId}\n" +
            $"AIの使用: {grpcClient.UseAi}",
            style
        );
    }
}

#endif
