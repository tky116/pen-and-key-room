using UnityEngine;
using UnityEngine.XR;

/// <summary>
/// MetaQuest3のHMD
/// </summary>
public class MetaQuestHMD : XRDeviceBase, IXRHMD
{
    public bool IsUserPresent { get; private set; } // ユーザがいるかどうか
    public Vector3 CenterEyePosition { get; private set; }  // 中心の目の位置
    public Quaternion CenterEyeRotation { get; private set; }   // 中心の目の回転

    /// <summary>
    /// XRデバイスを初期化する
    /// </summary>
    public override void UpdateState()
    {
        base.UpdateState();
        if (!IsValid) return;

        _device.TryGetFeatureValue(CommonUsages.userPresence, out bool userPresence);
        IsUserPresent = userPresence;
        _device.TryGetFeatureValue(CommonUsages.centerEyePosition, out Vector3 centerEyePos);
        CenterEyePosition = centerEyePos;
        _device.TryGetFeatureValue(CommonUsages.centerEyeRotation, out Quaternion centerEyeRot);
        CenterEyeRotation = centerEyeRot;
    }
}
