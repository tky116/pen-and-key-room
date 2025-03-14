using UnityEngine;
using UnityEngine.Video;

public class VideoPlayerController : MonoBehaviour
{
    [Header("Video Setup")]
    [SerializeField] private VideoPlayer videoPlayer;
    [SerializeField] private GameObject videoPlayerObject;

    [Header("Video Clips")]
    [SerializeField] private VideoClip helpColorPickerClip;

    private bool isPlaying = false;

    private void Start()
    {
        // 初期設定
        if (videoPlayer != null)
        {
            videoPlayer.playOnAwake = false;
            videoPlayer.isLooping = true;
            videoPlayer.audioOutputMode = VideoAudioOutputMode.None; // 音声なし
        }

        // 初期状態ではVideoPlayerObject自体を非アクティブに
        if (videoPlayerObject != null)
        {
            videoPlayerObject.SetActive(false);
        }
    }

    // カラーピッカーヘルプボタン用の関数 - 最初のビデオを再生/停止
    public void ToggleVideoHelpColorPicker()
    {
        ToggleVideoPlayback(helpColorPickerClip);
    }


    // 動画の再生/停止を切り替える
    private void ToggleVideoPlayback(VideoClip clip)
    {
        if (videoPlayer == null || videoPlayerObject == null)
            return;

        // 現在再生中であれば停止する
        if (isPlaying)
        {
            StopVideo();
        }
        // 停止中であれば、指定されたクリップを再生する
        else
        {
            PlayVideo(clip);
        }
    }

    // 動画を再生する
    private void PlayVideo(VideoClip clip)
    {
        videoPlayer.clip = clip;
        videoPlayerObject.SetActive(true);
        videoPlayer.Play();
        isPlaying = true;
    }

    // 動画を停止する
    private void StopVideo()
    {
        videoPlayer.Stop();
        videoPlayerObject.SetActive(false);
        isPlaying = false;
    }
}
