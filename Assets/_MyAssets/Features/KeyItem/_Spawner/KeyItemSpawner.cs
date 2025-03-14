using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Collections.Generic;

public class KeyItemSpawner : MonoBehaviour
{
    [Header("Key Item Settings")]
    [SerializeField] private List<GameObject> keyItems;

    [Header("Visual Settings")]
    [SerializeField] private Transform blackHoleModel;
    [SerializeField] private float appearDuration = 2.0f;

    [Header("Audio Settings")]
    [SerializeField] private AudioSource spawnerAudioSource;
    [SerializeField] private float spawnerVolume = 1.0f;
    [SerializeField] private AudioSource itemSpawnAudioSource;
    //[SerializeField] private float itemSpawnVolume = 1.0f;
    [SerializeField] private float audioFadeDuration = 1.0f;

    [Header("Test Settings (Spaceで実行)")]
    [SerializeField] private bool spawnTest = false;
    [SerializeField] private string testItemName = "";

    private Vector3 initialScaleDistortion;
    private Vector3 initialScaleHole;
    private GameObject[] childObjects;
    private Dictionary<string, Vector3> originalItemPositions = new Dictionary<string, Vector3>();

    private void Start()
    {
        initialScaleHole = Vector3.one;

        blackHoleModel.localScale = Vector3.zero;

        childObjects = new GameObject[transform.childCount];
        for (int i = 0; i < transform.childCount; i++)
        {
            childObjects[i] = transform.GetChild(i).gameObject;
            childObjects[i].SetActive(false);
        }
    }

    private void Update()
    {
        if (spawnTest && !string.IsNullOrEmpty(testItemName))
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                ManageSpawnSequence(testItemName).Forget();
            }
        }
    }

    /// <summary>
    /// スポーンシーケンスの管理
    /// </summary>
    /// <param name="itemName"></param>
    /// <returns></returns>
    public async UniTask<bool> ManageSpawnSequence(string itemName)
    {
        try
        {
            SetChildrenActive(true);

            // SpawnerAudioをフェードイン開始
            if (spawnerAudioSource != null)
            {
                Debug.Log("Start Spawner Audio");
                await FadeInAudio(spawnerAudioSource, spawnerVolume, audioFadeDuration);
            }

            // ビジュアルエフェクトの開始
            await AppearBlackHole();

            // アイテム自体を非アクティブ化（重力無効）
            SetItemActive(itemName, false);

            bool success = await MoveItemToSpawnPosition(itemName);
            if (!success)
            {
                Debug.LogError($"[ManageSpawnSequence] Failed to move item '{itemName}'.");
                SetChildrenActive(false);
                return false;
            }

            if (itemSpawnAudioSource != null)
            {
                itemSpawnAudioSource.Play();

                // 再生が終わるまで待機
                await UniTask.WaitUntil(() => !itemSpawnAudioSource.isPlaying);

                // 0.5秒待つ
                await UniTask.Delay(500);

                // 音声が終了したらアイテムをアクティブ化（重力有効）
                SetItemActive(itemName, true);
            }

            await DisappearBlackHole();

            if (spawnerAudioSource != null)
            {
                await FadeOutAudio(spawnerAudioSource, audioFadeDuration);
            }

            SetChildrenActive(false);
            return true;
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Error in spawn sequence: {ex.Message}");
            return false;
        }
    }

    private void SetChildrenActive(bool active)
    {
        foreach (var obj in childObjects)
        {
            obj.SetActive(active);
        }
    }

    private async UniTask AppearBlackHole()
    {
        float elapsed = 0f;
        while (elapsed < appearDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / appearDuration;
            blackHoleModel.localScale = Vector3.Lerp(Vector3.zero, initialScaleHole, t);
            await UniTask.Yield();
        }
        blackHoleModel.localScale = initialScaleHole;
    }

    /// <summary>
    /// アイテムをスポーン位置に移動
    /// </summary>
    /// <param name="itemName"></param>
    /// <returns></returns>
    private async UniTask<bool> MoveItemToSpawnPosition(string itemName)
    {
        GameObject item = FindItem(itemName);
        if (item == null)
        {
            Debug.LogError($"[MoveItemToSpawnPosition] Item '{itemName}' not found in the list.");
            return false;
        }

        if (!originalItemPositions.ContainsKey(itemName))
        {
            originalItemPositions[itemName] = item.transform.position;
        }

        item.transform.position = blackHoleModel.position;

        float spawnAnimationDuration = 1.5f;
        await AnimateVector3(Vector3.zero, item.transform.localScale, spawnAnimationDuration,
            value => item.transform.localScale = value);

        return true;
    }

    private GameObject FindItem(string itemName)
    {
        return keyItems.Find(item => item.name == itemName);
    }

    private async UniTask DisappearBlackHole()
    {
        float elapsed = 0f;
        Vector3 currentScaleHole = blackHoleModel.localScale;

        while (elapsed < appearDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / appearDuration;
            blackHoleModel.localScale = Vector3.Lerp(currentScaleHole, Vector3.zero, t);
            await UniTask.Yield();
        }

        blackHoleModel.localScale = Vector3.zero;
    }

    private async UniTask AnimateVector3(Vector3 from, Vector3 to, float duration, System.Action<Vector3> updateValue)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            updateValue(Vector3.Lerp(from, to, t));
            await UniTask.Yield();
        }
        updateValue(to);
    }

    /// <summary>
    /// オーディオのフェードイン
    /// </summary>
    private async UniTask FadeInAudio(AudioSource audioSource, float targetVolume, float duration)
    {
        if (audioSource == null) return;

        audioSource.volume = 0;
        audioSource.Play();
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            audioSource.volume = Mathf.Lerp(0, targetVolume, elapsed / duration);
            await UniTask.Yield();
        }
        audioSource.volume = targetVolume;
    }

    /// <summary>
    /// オーディオのフェードアウト
    /// </summary>
    private async UniTask FadeOutAudio(AudioSource audioSource, float duration)
    {
        if (audioSource == null) return;

        float startVolume = audioSource.volume;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            audioSource.volume = Mathf.Lerp(startVolume, 0, elapsed / duration);
            await UniTask.Yield();
        }
        audioSource.volume = 0;
        audioSource.Stop();
    }

    /// <summary>
    /// アイテムをアクティブ / 非アクティブにする
    /// </summary>
    private void SetItemActive(string itemName, bool isActive)
    {
        GameObject item = FindItem(itemName);
        if (item == null) return;

        item.SetActive(isActive);
    }
}
