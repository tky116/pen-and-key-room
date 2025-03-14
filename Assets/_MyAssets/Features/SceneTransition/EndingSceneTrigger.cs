using UnityEngine;
using Cysharp.Threading.Tasks;

public class EndingSceneTrigger : SceneTrigger
{
    private bool _isTriggered;

    protected override bool CheckTransitionCondition() => _isTriggered;

    public void TriggerEnding()
    {
        _isTriggered = true;
        QuitGameSequence().Forget();
    }

    private async UniTaskVoid QuitGameSequence()
    {
        if (SceneTransitionManager.Instance == null)
        {
            Debug.LogError("SceneTransitionManager.Instance が null です！");
            return;
        }

        // フェードアウト
        if (SceneTransitionManager.Instance.fadeController != null)
        {
            await SceneTransitionManager.Instance.fadeController.FadeOutAsync();
        }

        Debug.Log("ゲームを終了します…");

        // 少し待機
        await UniTask.Delay(1000);

        // ゲーム終了
        QuitGame();
    }

    private void QuitGame()
    {
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}
