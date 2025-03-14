using UnityEngine;
using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Net.Http;
using Cysharp.Threading.Tasks;
using Grpc.Net.Client;
using VRAcademyAudition;
using System.Diagnostics;
using Color = UnityEngine.Color;
using Debug = UnityEngine.Debug;

/// <summary>
/// クライアントの動作モード
/// </summary>
public enum ClientOperationMode
{
    /// <summary>
    /// オンラインモード：
    /// ・サーバーとの通信を行う
    /// ・ヘルスチェックを定期的に実行し、データ送信時の通信確立を担保
    /// </summary>
    Online,

    /// <summary>
    /// オフラインモード：
    /// ・サーバーとの通信を行わない
    /// </summary>
    Offline
}

/// <summary>
/// サーバーとの通信状態を表す列挙型
/// </summary>
public enum ConnectionState
{
    /// <summary>通信可能な状態</summary>
    Ready,

    /// <summary>サーバーと通信中</summary>
    Connecting,

    /// <summary>通信成功</summary>
    Success,

    /// <summary>通信エラー</summary>
    Error,

    /// <summary>オフライン状態</summary>
    Offline
}

/// <summary>
/// 描画データをgRPCサーバーに送信するクライアント
/// </summary>
public class GrpcClient : MonoBehaviour
{
    [Header("Client Settings")]
    [SerializeField] private ClientOperationMode operationMode = ClientOperationMode.Online;

    [SerializeField] private bool debugMode = false;
    [SerializeField] private bool useAi = true;

    [Header("Server Settings")]
    [Tooltip("オンラインモード時の接続先サーバーアドレス")]
    [SerializeField] private string serverAddress = "http://localhost:50051";

    [Header("Health Check Settings")]
    [Tooltip("ヘルスチェックのタイムアウト時間（秒）")]
    [SerializeField][Range(1, 30)] private float timeoutSeconds = 5f;

    [Tooltip("ヘルスチェックの再試行回数")]
    [SerializeField][Range(1, 3)] private int retryCount = 2;

    [Tooltip("ヘルスチェックの実行間隔（秒）※オンラインモードのみ有効")]
    [SerializeField][Range(10, 600)] private float intervalSeconds = 60f;

    [Tooltip("再接続間隔（秒）※オンラインモードのみ有効")]
    [SerializeField][Range(5, 60)] private float reconnectionInterval = 10f;

    [Header("Scene Settings")]
    [SerializeField] private string sceneId = "TestStage";

    [Header("References")]
    [SerializeField] private VRPenDrawingManager drawingManager;
    [SerializeField] private Transform inkPoolSynced;
    [SerializeField] private KeyItemSpawner keyItemSpawner;

    private GrpcChannel channel;
    private DrawingService.DrawingServiceClient client;
    private bool isInitialized = false;
    private float lastHealthCheckTime;
    private float lastReconnectionAttempt;
    private Dictionary<string, string> defaultMetadata;
    private ConnectionState _connectionState = ConnectionState.Ready;
    private CancellationTokenSource _healthCheckCts;

    public bool UseAi { get => useAi; set => useAi = value; }   // AIを使用するかどうか
    public string SceneId { get => sceneId; set => sceneId = value; }   // シーンID
    public ConnectionState ConnectionState => _connectionState; // サーバーとの通信状態

    // ステータス情報のプロパティ追加
    public string StatusMessage { get; private set; } = "描いてね！";
    public Color StatusColor { get; private set; } = Color.white;

    /// <summary>
    /// ステータス情報を更新
    /// </summary>
    private void UpdateStatus(string message, Color color)
    {
        StatusMessage = message;
        StatusColor = color;
    }

    // 状態に応じたステータス更新メソッド
    private void SetProcessingStatus() => UpdateStatus("送信中...", Color.yellow);
    private void SetAiProcessingStatus() => UpdateStatus("考え中...", Color.yellow);
    private void SetReadyStatus() => UpdateStatus("送信できます", Color.white);
    private void SetDrawPromptStatus() => UpdateStatus("描いてね！", Color.white);
    private void SetErrorStatus(string message = "エラーが発生しました") => UpdateStatus(message, Color.red);
    private void SetWaitingStatus(float remainingTime) => UpdateStatus($"送信待機中... {remainingTime:F1}秒", Color.white);

    private void Awake()
    {
        InitializeDefaultMetadata();
        _healthCheckCts = new CancellationTokenSource();
    }

    private async void Start()
    {
        if (operationMode == ClientOperationMode.Offline)
        {
            SetConnectionState(ConnectionState.Offline);
            return;
        }

        await InitializeClient();
        StartPeriodicHealthCheck().Forget();
    }

    /// <summary>
    /// オブジェクトが破棄される際に実行される処理
    /// </summary>
    private async void OnDestroy()
    {
        _healthCheckCts?.Cancel();
        _healthCheckCts?.Dispose();
        if (channel != null)
        {
            await channel.ShutdownAsync().AsUniTask();
        }
        channel?.Dispose();
    }

    private void Update()
    {
        if (operationMode == ClientOperationMode.Offline) return;
        if (!isInitialized && Time.time - lastReconnectionAttempt >= reconnectionInterval)
        {
            InitializeClient().Forget();
        }
    }

    /// <summary>
    /// クライアントを初期化する
    /// </summary>
    private async UniTask InitializeClient()
    {
        try
        {
            lastReconnectionAttempt = Time.time;
            UpdateStatus("サーバーに接続中...", Color.yellow);

            await Initialize();
            isInitialized = true;
            SetConnectionState(ConnectionState.Ready);
            SetReadyStatus();
        }
        catch (Exception ex)
        {
            LogError($"Failed to initialize: {ex.Message}");
            isInitialized = false;
            SetConnectionState(ConnectionState.Error);
            SetErrorStatus("サーバーに接続できません");
        }
    }

    /// <summary>
    /// gRPCクライアントを初期化する
    /// </summary>
    /// <returns></returns>
    private async UniTask Initialize()
    {
        if (channel != null)
        {
            await channel.ShutdownAsync().AsUniTask();
            channel.Dispose();
            channel = null;
            client = null;
        }

        var handler = new YetAnotherHttpHandler { Http2Only = true };
        channel = GrpcChannel.ForAddress(serverAddress, new GrpcChannelOptions
        {
            HttpHandler = handler,
            DisposeHttpClient = true
        });

        client = new DrawingService.DrawingServiceClient(channel);

        if (!await CheckServerHealthWithRetry())
        {
            throw new Exception("Server health check failed");
        }
    }

    /// <summary>
    /// サーバーのヘルスチェックをリトライする
    /// </summary>
    /// <returns></returns>
    private async UniTask<bool> CheckServerHealthWithRetry()
    {
        var backoffDelay = TimeSpan.FromSeconds(1);
        for (int attempt = 0; attempt < retryCount; attempt++)
        {
            try
            {
                using var cts = CancellationTokenSource.CreateLinkedTokenSource(_healthCheckCts.Token);
                cts.CancelAfter(TimeSpan.FromSeconds(timeoutSeconds));

                var request = new HealthCheckRequest();
                var response = await client.CheckHealthAsync(request)
                    .ResponseAsync
                    .AsUniTask()
                    .AttachExternalCancellation(cts.Token);

                if (response.Status != HealthCheckResponse.Types.ServingStatus.Serving)
                {
                    throw new Exception($"Server not ready: {response.Status}");
                }

                LogDebug($"Server health check passed (attempt {attempt + 1}/{retryCount})");
                return true;
            }
            catch (Exception ex)
            {
                LogError($"Health check failed: {ex.Message} (attempt {attempt + 1}/{retryCount})");
                if (attempt == retryCount - 1) return false;

                await UniTask.Delay(backoffDelay, cancellationToken: _healthCheckCts.Token);
                backoffDelay *= 2; // 指数バックオフ
            }
        }
        return false;
    }

    /// <summary>
    /// ヘルスチェックを実行する
    /// </summary>
    /// <returns></returns>
    private async UniTask StartPeriodicHealthCheck()
    {
        while (!_healthCheckCts.Token.IsCancellationRequested)
        {
            try
            {
                await UniTask.Delay(TimeSpan.FromSeconds(intervalSeconds),
                    cancellationToken: _healthCheckCts.Token);

                if (isInitialized && operationMode == ClientOperationMode.Online)
                {
                    isInitialized = await CheckServerHealthWithRetry();
                    if (isInitialized)
                    {
                        lastHealthCheckTime = Time.time;
                    }
                    else
                    {
                        SetConnectionState(ConnectionState.Error);
                        await UniTask.Delay(TimeSpan.FromSeconds(reconnectionInterval),
                            cancellationToken: _healthCheckCts.Token);
                    }
                }
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }
    }

    /// <summary>
    /// デフォルトのメタデータ
    /// </summary>
    private void InitializeDefaultMetadata()
    {
        defaultMetadata = new Dictionary<string, string>
        {
            { "app_version", Application.version },
            { "unity_version", Application.unityVersion },
            { "platform", Application.platform.ToString() },
            { "operation_mode", operationMode.ToString() }
        };
    }

    /// <summary>
    /// LineRendererのデータをgRPCのLineに変換
    /// </summary>
    public UniTask<string> SendDrawingData(LineRenderer[] lineRenderers, Vector3 center)
    {
        return SendDrawingDataAsync(lineRenderers, center);
    }

    /// <summary>
    /// 描画データをサーバーに送信する
    /// </summary>
    private async UniTask<string> SendDrawingDataAsync(LineRenderer[] lineRenderers, Vector3 center)
    {
        if (!isInitialized) return null;

        // アップロード用にワールド座標へ変換
        List<Line> worldLines = new List<Line>();
        foreach (var lr in lineRenderers)
        {
            worldLines.Add(ConvertToProtoLine(lr, inkPoolSynced));
        }

        var drawingData = new DrawingData
        {
            DrawingId = Guid.NewGuid().ToString(),
            SceneId = sceneId,
            DrawTimestamp = DateTimeOffset.Now.ToUnixTimeMilliseconds(),
            Center = ConvertToVector3Proto(center),
            UseAi = useAi,
            ClientId = SystemInfo.deviceUniqueIdentifier,
            ClientInfo = CreateClientInfo(),
            DrawLines = { worldLines }
        };

        // メタデータの追加
        foreach (var kvp in defaultMetadata)
        {
            drawingData.Metadata.Add(kvp.Key, kvp.Value);
        }
        drawingData.Metadata.Add("created_at_utc", DateTime.UtcNow.ToString("o"));

        using var cts = CancellationTokenSource.CreateLinkedTokenSource(_healthCheckCts.Token);
        cts.CancelAfter(TimeSpan.FromSeconds(timeoutSeconds));

        try
        {
            var uploadResponse = await client.UploadDrawingAsync(drawingData, cancellationToken: cts.Token).ResponseAsync;

            if (uploadResponse?.Success != true)
            {
                throw new Exception(uploadResponse?.Message ?? "Empty response");
            }

            if (drawingData.UseAi)
            {
                await ProcessAiResponseAsync(drawingData, cts.Token);
            }

            return uploadResponse.UploadId;
        }
        catch (Exception)
        {
            throw;
        }
    }

    /// <summary>
    /// 描画データを送信し、アイテム生成の完了を待つ
    /// </summary>
    public async UniTask SendDrawingDataAndWait()
    {
        if (operationMode == ClientOperationMode.Offline)
        {
            LogDrawingData(inkPoolSynced.GetComponentsInChildren<LineRenderer>(), Vector3.zero);
            return;
        }

        try
        {
            var lineRenderers = inkPoolSynced.GetComponentsInChildren<LineRenderer>();
            if (lineRenderers == null || lineRenderers.Length == 0)
            {
                LogWarning("送信する描画データがありません");
                return;
            }

            SetConnectionState(ConnectionState.Connecting);
            SetProcessingStatus();

            var center = CalculateDrawingCenter(lineRenderers);
            var drawingData = CreateDrawingData(lineRenderers, center);

            using var cts = CancellationTokenSource.CreateLinkedTokenSource(_healthCheckCts.Token);
            cts.CancelAfter(TimeSpan.FromSeconds(timeoutSeconds));

            try
            {
                var uploadResponse = await client.UploadDrawingAsync(drawingData, cancellationToken: cts.Token).ResponseAsync;

                if (uploadResponse?.Success == true)
                {
                    // アップロード成功時点で描画をクリア
                    drawingManager?.ClearAllDrawings();
                    SetConnectionState(ConnectionState.Success);

                    // AIレスポンスの処理は別途実行
                    if (useAi)
                    {
                        await ProcessAiResponseAsync(drawingData, cts.Token);
                    }
                }
                else
                {
                    throw new Exception(uploadResponse?.Message ?? "Empty response");
                }
            }
            catch (Exception)
            {
                SetConnectionState(ConnectionState.Error);
                throw;
            }
        }
        catch (Exception ex)
        {
            LogError($"Send error: {ex.Message}");
            SetConnectionState(ConnectionState.Error);
            SetErrorStatus();
            throw;
        }
    }

    /// <summary>
    /// AI処理の結果を処理する
    /// </summary>
    /// <param name="drawingData"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    private async UniTask ProcessAiResponseAsync(DrawingData drawingData, CancellationToken token)
    {
        var stopwatch = Stopwatch.StartNew();
        try
        {
            LogDebug($"AI処理リクエスト送信 - Drawing ID: {drawingData.DrawingId}");

            SetAiProcessingStatus();

            ShapeRecognitionClient aiResponse = null;
            try
            {
                aiResponse = await client.ProcessDrawingAsync(drawingData, cancellationToken: token).ResponseAsync;
                LogDebug($"AI処理応答の詳細: Success={aiResponse?.Success}, PrefabName={aiResponse?.PrefabName}, ErrorMessage={aiResponse?.ErrorMessage}");
            }
            catch (Exception ex)
            {
                // gRPC呼び出し自体でエラーが発生した場合
                LogError($"gRPC呼び出し中にエラーが発生: {ex.GetType().Name}: {ex.Message}");
                if (ex.InnerException != null)
                {
                    LogError($"内部エラー: {ex.InnerException.GetType().Name}: {ex.InnerException.Message}");
                }
                throw;
            }


            // AIの処理に失敗した場合
            if (!aiResponse.Success)
            {
                LogError($"AI processing failed: {aiResponse.ErrorMessage}");
                LogDebug($"AI処理失敗 - 処理時間: {stopwatch.ElapsedMilliseconds} ms");
                ServerFeedbackManager.Instance.ShowFeedback(ServerFeedbackType.Failure);
                return;
            }

            // AIの処理は成功したが、プレハブ名がUnknownの場合（スコアが閾値以下）
            if (aiResponse.PrefabName == "Unknown")
            {
                LogWarning($"AI processing successful but below threshold: {aiResponse.ErrorMessage}");
                LogDebug($"AI処理成功（閾値以下） - 処理時間: {stopwatch.ElapsedMilliseconds} ms");
                ServerFeedbackManager.Instance.ShowFeedback(ServerFeedbackType.Failure);
                return;
            }

            // AIの処理に成功しScoreも閾値以上の場合
            LogDebug("AI processing success:");
            LogDebug($"- Response AI: {aiResponse}");
            LogDebug($"- Drawing ID: {aiResponse.DrawingId}");
            LogDebug($"- Prefab Name: {aiResponse.PrefabName}");
            LogDebug($"AI処理成功 - 総処理時間: {stopwatch.ElapsedMilliseconds} ms");

            bool spawnResult = await keyItemSpawner.ManageSpawnSequence(aiResponse.PrefabName);
            if (!spawnResult)
            {
                LogError("アイテム生成失敗");
                ServerFeedbackManager.Instance.ShowFeedback(ServerFeedbackType.Failure);
            }
            else
            {
                ServerFeedbackManager.Instance.ShowFeedback(ServerFeedbackType.Success);
            }
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            LogError($"AI processing error: {ex.Message}");
            LogDebug($"AI処理エラー発生 - 総処理時間: {stopwatch.ElapsedMilliseconds} ms");
            SetErrorStatus();
            throw;
        }
        finally
        {
            SetReadyStatus();
        }
    }

    /// <summary>
    /// 描画データを生成する
    /// </summary>
    /// <param name="lineRenderers"></param>
    /// <param name="center"></param>
    /// <returns></returns>
    private DrawingData CreateDrawingData(LineRenderer[] lineRenderers, Vector3 center)
    {
        var drawingData = new DrawingData
        {
            DrawingId = Guid.NewGuid().ToString(),
            SceneId = sceneId,
            DrawTimestamp = DateTimeOffset.Now.ToUnixTimeMilliseconds(),
            DrawLines = { },
            Center = ConvertToVector3Proto(center),
            UseAi = useAi,
            ClientId = SystemInfo.deviceUniqueIdentifier,
            ClientInfo = CreateClientInfo()
        };

        // メタデータの追加
        foreach (var kvp in defaultMetadata)
        {
            drawingData.Metadata.Add(kvp.Key, kvp.Value);
        }
        drawingData.Metadata.Add("created_at_utc", DateTime.UtcNow.ToString("o"));

        // 線データをワールド座標に変換して追加
        foreach (var lr in lineRenderers)
        {
            drawingData.DrawLines.Add(ConvertToProtoLine(lr, inkPoolSynced));
        }

        return drawingData;
    }

    /// <summary>
    /// クライアント情報を生成する
    /// </summary>
    /// <returns></returns>
    private ClientInfo CreateClientInfo()
    {
        return new ClientInfo
        {
            // Unity Editor上での実行かどうかで環境を判断
            Type = Application.isEditor ?
                ClientInfo.Types.ClientType.Development :
                ClientInfo.Types.ClientType.Production,
            DeviceId = SystemInfo.deviceUniqueIdentifier,
            DeviceName = SystemInfo.deviceName,
            SystemInfo = $"{SystemInfo.operatingSystem}, {SystemInfo.processorType}",
            AppVersion = Application.version
        };
    }

    /// <summary>
    /// LineRendererのデータをgRPCのLineに変換
    /// </summary>
    /// <param name="lineRenderer"></param>
    /// <returns></returns>
    private Line ConvertToProtoLine(LineRenderer lineRenderer, Transform inkPoolSynced)
    {
        var line = new Line();

        Vector3[] positions = new Vector3[lineRenderer.positionCount];
        lineRenderer.GetPositions(positions);

        for (int i = 0; i < positions.Length; i++)
        {
            positions[i] = inkPoolSynced.TransformPoint(positions[i]);
        }

        foreach (var pos in positions)
        {
            line.Positions.Add(ConvertToVector3Proto(pos));
        }

        line.Width = lineRenderer.startWidth;
        line.Color = new VRAcademyAudition.Color
        {
            R = lineRenderer.material.color.r,
            G = lineRenderer.material.color.g,
            B = lineRenderer.material.color.b,
            A = lineRenderer.material.color.a
        };

        return line;
    }

    /// <summary>
    /// LineRendererのデータをgRPCのLineに変換
    /// </summary>
    /// <param name="vector"></param>
    /// <returns></returns>
    private Vector3Proto ConvertToVector3Proto(Vector3 vector)
    {
        return new Vector3Proto
        {
            X = vector.x,
            Y = vector.y,
            Z = vector.z
        };
    }

    /// <summary>
    /// 描画データの中心点を計算する
    /// </summary>
    /// <param name="lineRenderers"></param>
    /// <returns></returns>
    private Vector3 CalculateDrawingCenter(LineRenderer[] lineRenderers)
    {
        Vector3 sum = Vector3.zero;
        int totalPoints = 0;

        foreach (var line in lineRenderers)
        {
            Vector3[] positions = new Vector3[line.positionCount];
            line.GetPositions(positions);

            foreach (var pos in positions)
            {
                sum += inkPoolSynced.TransformPoint(pos);   // ワールド座標に変換
                totalPoints++;
            }
        }

        return totalPoints > 0 ? sum / totalPoints : Vector3.zero;
    }

    /// <summary>
    /// デバッグログを出力する
    /// </summary>
    /// <param name="message"></param>
    private void LogDebug(string message)
    {
        if (debugMode)
        {
            UnityEngine.Debug.Log($"[{operationMode}Mode] {message}");
        }
    }

    /// <summary>
    /// エラーログを出力する
    /// </summary>
    /// <param name="message"></param>
    private void LogError(string message)
    {
        var logMessage = debugMode ? $"[{operationMode}Mode] {message}" : message;
        UnityEngine.Debug.LogError(logMessage);
    }

    /// <summary>
    /// 警告ログを出力する
    /// </summary>
    /// <param name="message"></param>
    private void LogWarning(string message)
    {
        var logMessage = debugMode ? $"[{operationMode}Mode] {message}" : message;
        UnityEngine.Debug.LogWarning(logMessage);
    }

    /// <summary>
    /// デバッグログに描画データを出力する
    /// </summary>
    /// <param name="lineRenderers"></param>
    /// <param name="center"></param>
    private void LogDrawingData(LineRenderer[] lineRenderers, Vector3 center)
    {
        if (!debugMode) return;

        LogDebug("描画データ:");
        LogDebug($"LineRenderer数: {lineRenderers.Length}");
        LogDebug($"中心点: {center}");

        for (int i = 0; i < lineRenderers.Length; i++)
        {
            var lr = lineRenderers[i];
            Vector3[] positions = new Vector3[lr.positionCount];
            lr.GetPositions(positions);

            LogDebug($"LineRenderer[{i}]: {lr.positionCount}点");
            LogDebug($"  - 開始点: {positions[0]}");
            LogDebug($"  - 終了点: {positions[positions.Length - 1]}");
            LogDebug($"  - 線の太さ: {lr.startWidth}");
            LogDebug($"  - 線の色: {lr.material.color}");
        }
    }

    /// <summary>
    /// 通信状態を設定する
    /// </summary>
    /// <param name="newState"></param>
    private async void SetConnectionState(ConnectionState newState)
    {
        _connectionState = newState;
        //LogDebug($"Connection state: {newState}");

        if (newState != ConnectionState.Connecting)
        {
            await UniTask.Delay(TimeSpan.FromSeconds(2))
                .AttachExternalCancellation(_healthCheckCts.Token);

            if (_connectionState == newState)  // 状態が変わっていない場合のみリセット
            {
                _connectionState = ConnectionState.Ready;
            }
        }
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (drawingManager == null)
            Debug.LogError($"Missing VRPenDrawingManager reference in {gameObject.name}");
        if (inkPoolSynced == null)
            Debug.LogError($"Missing inkPoolSynced reference in {gameObject.name}");
        if (keyItemSpawner == null)
            Debug.LogError($"Missing KeyItemSpawner reference in {gameObject.name}");
    }
#endif
}
