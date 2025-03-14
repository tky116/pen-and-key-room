using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// シーン内の特定の型のオブジェクトを取得する
/// </summary>
public class SoundManager : MonoBehaviour
{
    private static SoundManager instance;

    public static SoundManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindFirstObjectByType<SoundManager>();
                if (instance == null)
                {
                    GameObject go = new GameObject("SoundManager");
                    instance = go.AddComponent<SoundManager>();
                    DontDestroyOnLoad(go);
                }
            }
            return instance;
        }
    }

    [System.Serializable]
    public class SoundData
    {
        public string name;
        public AudioClip clip;
        public bool loop = false;
        public float volume = 1f;
        public float spatialBlend = 0f; // 0: 2D, 1: 3D
        public float minDistance = 1f;
        public float maxDistance = 500f;
    }

    public List<SoundData> bgmList = new List<SoundData>();
    public List<SoundData> seList = new List<SoundData>();

    private Dictionary<string, AudioSource> activeSources = new Dictionary<string, AudioSource>();

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        // シーンBGMの存在確認
        SoundData sceneBGM = bgmList.Find(x => x.name == "SceneBGM");

        // BGMが設定されている場合のみ再生
        if (sceneBGM != null && sceneBGM.clip != null)
        {
            PlayBGM("SceneBGM");
        }
    }

    /// <summary>
    /// BGMを再生（ループ再生）
    /// </summary>
    /// <param name="name"></param>
    public void PlayBGM(string name)
    {
        SoundData data = bgmList.Find(x => x.name == name);
        if (data == null) return;

        if (activeSources.ContainsKey(name))
        {
            if (!activeSources[name].isPlaying)
                activeSources[name].Play();
            return;
        }

        AudioSource source = gameObject.AddComponent<AudioSource>();
        source.clip = data.clip;
        source.loop = true;
        source.volume = data.volume;
        source.spatialBlend = data.spatialBlend;
        source.Play();

        activeSources.Add(name, source);
    }

    /// <summary>
    /// SEを再生（一回のみ）
    /// </summary>
    /// <param name="name"></param>
    public void PlaySE(string name)
    {
        SoundData data = seList.Find(x => x.name == name);
        if (data == null) return;

        AudioSource source = gameObject.AddComponent<AudioSource>();
        source.clip = data.clip;
        source.loop = data.loop;
        source.volume = data.volume;
        source.spatialBlend = data.spatialBlend;
        source.minDistance = data.minDistance;
        source.maxDistance = data.maxDistance;
        source.Play();

        if (!data.loop)
        {
            Destroy(source, data.clip.length);
        }
        else
        {
            activeSources.Add(name, source);
        }
    }

    /// <summary>
    /// 3D空間で音を再生
    /// </summary>
    /// <param name="name"></param>
    /// <param name="position"></param>
    public void Play3DSoundAtPosition(string name, Vector3 position)
    {
        SoundData data = seList.Find(x => x.name == name);
        if (data == null) return;

        GameObject soundObject = new GameObject($"3DSound_{name}");
        soundObject.transform.position = position;

        AudioSource source = soundObject.AddComponent<AudioSource>();
        source.clip = data.clip;
        source.loop = data.loop;
        source.volume = data.volume;
        source.spatialBlend = 1f; // 完全な3Dサウンド
        source.minDistance = data.minDistance;
        source.maxDistance = data.maxDistance;
        source.rolloffMode = AudioRolloffMode.Linear;
        source.Play();

        if (!data.loop)
        {
            Destroy(soundObject, data.clip.length);
        }
    }

    /// <summary>
    /// 特定の音を停止
    /// </summary>
    /// <param name="name"></param>
    public void StopSound(string name)
    {
        if (activeSources.ContainsKey(name))
        {
            Destroy(activeSources[name]);
            activeSources.Remove(name);
        }
    }

    /// <summary>
    /// 全ての音を停止
    /// </summary>
    public void StopAllSounds()
    {
        foreach (var source in activeSources.Values)
        {
            Destroy(source);
        }
        activeSources.Clear();
    }
}

//// 使用例：焚火の音を実装するコンポーネント
//public class BonfireSound : MonoBehaviour
//{
//    private AudioSource source;
//    public float maxVolume = 1f;
//    public float fadeDistance = 5f;

//    void Start()
//    {
//        // 3D音源として初期化
//        source = gameObject.AddComponent<AudioSource>();
//        source.spatialBlend = 1f; // 完全な3Dサウンド
//        source.loop = true;
//        source.volume = maxVolume;
//        source.minDistance = 1f;
//        source.maxDistance = fadeDistance;
//        source.rolloffMode = AudioRolloffMode.Linear;

//        // サウンドマネージャーから音源を取得
//        SoundManager.SoundData bonfireSound = SoundManager.Instance.seList.Find(x => x.name == "bonfire");
//        if (bonfireSound != null)
//        {
//            source.clip = bonfireSound.clip;
//            source.Play();
//        }
//    }
//}
