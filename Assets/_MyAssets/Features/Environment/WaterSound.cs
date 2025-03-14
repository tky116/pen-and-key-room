using UnityEngine;
using Cysharp.Threading.Tasks;
using System;
using System.Threading;

public class WaterSound : MonoBehaviour
{
    [SerializeField] private AudioClip[] waterSounds;
    [SerializeField] private AudioSource audioSource;

    private CancellationTokenSource cancellationTokenSource;

    private void Awake()
    {
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }
    }

    private void OnEnable()
    {
        cancellationTokenSource = new CancellationTokenSource();
        PlayRandomSoundsAsync(cancellationTokenSource.Token).Forget();
    }

    private void OnDisable()
    {
        cancellationTokenSource?.Cancel();
        cancellationTokenSource?.Dispose();
        cancellationTokenSource = null;
    }

    private async UniTaskVoid PlayRandomSoundsAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                // ランダムな待機時間（10〜20秒）
                float waitTime = UnityEngine.Random.Range(10f, 20f);
                await UniTask.Delay(TimeSpan.FromSeconds(waitTime), cancellationToken: cancellationToken);

                // ランダムな音を選択して再生
                if (waterSounds.Length > 0 && !cancellationToken.IsCancellationRequested)
                {
                    int randomIndex = UnityEngine.Random.Range(0, waterSounds.Length);
                    audioSource.clip = waterSounds[randomIndex];
                    audioSource.Play();
                }
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }
    }
}
