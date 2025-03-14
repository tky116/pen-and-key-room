using Oculus.Interaction.Locomotion;
using Oculus.Interaction;
using UnityEngine;

/*
 * 参考 https://xr.systemfriend.co.jp/2024-05-07/
 */

public class TeleportController : MonoBehaviour
{
    [SerializeField]
    private ActiveStateGate teleportActiveStateGate;

    [SerializeField]
    private TeleportInteractor teleportInteractor;

    private OVRPlayerController ovrPlayerController;
    private CharacterController characterController;

    private float halfHeight;
    private bool previousState = false;

    private void Start()
    {
        SetComponent();
        SetWhenLocomotionPerformed(true);
    }

    private void OnDisable()
    {
        SetWhenLocomotionPerformed(false);
    }

    void LateUpdate()
    {
        CheckTeleportActiveState();
    }

    /// <summary>
    /// OVRPlayerControllerにアタッチする事を前提に各種コンポーネントを取得
    /// </summary>
    private void SetComponent()
    {
        this.ovrPlayerController = this.GetComponent<OVRPlayerController>();
        this.characterController = this.GetComponent<CharacterController>();

        this.halfHeight = this.characterController.height / 2;
    }

    /// <summary>
    /// テレポート時に実行するイベントの設定・解除を指定します
    /// </summary>
    /// <param name="state">trueで設定、falseで解除します</param>
    private void SetWhenLocomotionPerformed(bool state)
    {
        if (state)
        {
            this.teleportInteractor.WhenLocomotionPerformed += OnTeleportOcurred;
        }
        else
        {
            this.teleportInteractor.WhenLocomotionPerformed -= OnTeleportOcurred;
        }
    }

    /// <summary>
    /// 干渉を避ける為にテレポート時は一旦OVRPlayerController、CharacterControllerを無効化します
    /// </summary>
    private void CheckTeleportActiveState()
    {
        if (this.teleportActiveStateGate.Active)
        {
            if (!this.previousState)
            {
                this.previousState = true;
                this.ovrPlayerController.enabled = false;
                this.characterController.enabled = false;
            }
        }
        else
        {
            if (this.previousState)
            {
                this.previousState = false;
                this.ovrPlayerController.enabled = true;
                this.characterController.enabled = true;
            }
        }
    }

    /// <summary>
    /// テレポート後にOVRPlayerControllerの位置を追従させます
    /// </summary>
    /// <param name="locEvent"></param>
    private void OnTeleportOcurred(LocomotionEvent locEvent)
    {
        this.transform.position = new Vector3(locEvent.Pose.position.x, locEvent.Pose.position.y + halfHeight, locEvent.Pose.position.z);

        this.ovrPlayerController.enabled = true;
        this.characterController.enabled = true;
    }
}
