using UnityEngine;

/// <summary>
/// CameraRig用：ヘッドセットの動きのみ追従
/// </summary>
public class CameraRigUIPositionManager : BaseUIPositionManager
{
    public override Vector3 GetBasePosition()
    {
        return centerEyeAnchor.position;
    }
}
