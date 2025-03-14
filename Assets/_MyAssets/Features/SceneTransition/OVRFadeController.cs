using Cysharp.Threading.Tasks;
using UnityEngine;

public class OVRFadeController : MonoBehaviour
{
    private OVRScreenFade screenFade;

    private void Awake()
    {
        UpdateScreenFadeReference();
    }

    private void UpdateScreenFadeReference()
    {
        if (screenFade == null)
        {
            screenFade = Object.FindFirstObjectByType<OVRScreenFade>();
        }
    }

    public async UniTask FadeOutAsync()
    {
        UpdateScreenFadeReference();
        if (screenFade == null)
        {
            Debug.LogWarning("OVRScreenFade が設定されていないため、フェードアウトをスキップします。");
            return;
        }

        screenFade.FadeOut();
        await UniTask.Delay((int)(screenFade.fadeTime * 1000));
    }

    public async UniTask FadeInAsync()
    {
        UpdateScreenFadeReference();
        if (screenFade == null)
        {
            Debug.LogWarning("OVRScreenFade が設定されていないため、フェードインをスキップします。");
            return;
        }

        screenFade.FadeIn();
        await UniTask.Delay((int)(screenFade.fadeTime * 1000));
    }
}
