using UnityEngine;
using Cysharp.Threading.Tasks;

public abstract class SceneTrigger : MonoBehaviour
{
    [SerializeField] private string nextSceneName; // 遷移するシーン名
    private bool isTransitioning = false;

    private void Update()
    {
        if (isTransitioning) return;

        if (CheckTransitionCondition())
        {
            TriggerSceneTransition().Forget();
        }
    }

    /// <summary>
    /// 各シーンごとの遷移条件（各シーンで実装）
    /// </summary>
    protected abstract bool CheckTransitionCondition();

    /// <summary>
    /// シーン遷移を実行（共通の条件 + シーンごとの条件）
    /// </summary>
    private async UniTaskVoid TriggerSceneTransition()
    {
        if (SceneTransitionManager.Instance == null)
        {
            Debug.LogError("SceneTransitionManager.Instance が null です！シーン内に SceneTransitionManager が存在しているか確認してください。");
            return;
        }

        isTransitioning = true;
        UniTask additionalCondition;

        try
        {
            additionalCondition = UniTask.WaitUntil(() => CheckTransitionCondition());

            if (additionalCondition.Equals(default(UniTask)))
            {
                additionalCondition = UniTask.CompletedTask;
            }
        }
        catch
        {
            Debug.LogWarning("追加条件が設定されていません。デフォルトの CompletedTask を使用します。");
            additionalCondition = UniTask.CompletedTask;
        }

        await SceneTransitionManager.Instance.TransitionToScene(nextSceneName, additionalCondition);
    }
}
