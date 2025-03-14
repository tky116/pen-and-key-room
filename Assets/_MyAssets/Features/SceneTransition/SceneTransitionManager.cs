using UnityEngine;
using UnityEngine.SceneManagement;
using Cysharp.Threading.Tasks;

public class SceneTransitionManager : MonoBehaviour
{
    private static SceneTransitionManager _instance;
    public static SceneTransitionManager Instance => _instance;

    public OVRFadeController fadeController;

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        fadeController = Object.FindFirstObjectByType<OVRFadeController>();
        if (fadeController == null)
        {
            Debug.LogError("OVRFadeController が見つかりません！シーン内に配置してください。");
        }
    }

    /// <summary>
    /// シーン遷移を実行（フェードアウト → シーン変更 → フェードイン）
    /// </summary>
    public async UniTask TransitionToScene(string sceneName, UniTask additionalCondition)
    {
        if (string.IsNullOrEmpty(sceneName)) return;

        // 現在のシーンでフェードアウト
        if (fadeController != null)
        {
            await fadeController.FadeOutAsync();
        }

        // シーンのロード
        var scene = SceneManager.LoadSceneAsync(sceneName);
        scene.allowSceneActivation = false;

        try
        {
            if (additionalCondition.Equals(default(UniTask)))
            {
                additionalCondition = UniTask.CompletedTask;
            }

            await UniTask.WhenAll(
                UniTask.Delay(2000),
                UniTask.WaitUntil(() => scene.progress >= 0.9f),
                additionalCondition
            );
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"シーン遷移中にエラーが発生しました: {ex.Message}");
        }

        // シーンのアクティブ化を許可
        scene.allowSceneActivation = true;

        // シーンが完全にロードされるまで待機
        await UniTask.WaitUntil(() => SceneManager.GetSceneByName(sceneName).isLoaded);

        // シーンをアクティブに設定
        var newScene = SceneManager.GetSceneByName(sceneName);
        await UniTask.NextFrame(); // 1フレーム待機して確実にシーンが準備できるのを待つ
        SceneManager.SetActiveScene(newScene);

        // 新しいシーンでフェードコントローラーを再取得
        fadeController = Object.FindFirstObjectByType<OVRFadeController>();

        // フェードイン
        if (fadeController != null)
        {
            await UniTask.NextFrame(); // コンポーネントの初期化を待つ
            await fadeController.FadeInAsync();
        }

        // シーン遷移前に明示的にリソースを解放
        await Resources.UnloadUnusedAssets(); // メモリ解放
        System.GC.Collect(); // ガベージコレクション
        System.GC.WaitForPendingFinalizers(); // リソース解放を待機
    }
}
