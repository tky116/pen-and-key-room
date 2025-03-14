using UnityEngine;
using UnityEngine.XR;

/// <summary>
/// XRデバイスの基底クラス
/// </summary>
public abstract class XRDeviceBase
{
    protected InputDevice _device;  // XRデバイス
    public bool IsValid => _device.isValid; // XRデバイスが有効かどうか
    public bool IsTracked { get; protected set; }   // トラッキングされているかどうか
    public Vector3 Position { get; protected set; } // 位置
    public Quaternion Rotation { get; protected set; }  // 回転

    /// <summary>
    /// 状態を更新する
    /// </summary>
    public virtual void UpdateState()
    {
        if (!IsValid) return;

        _device.TryGetFeatureValue(CommonUsages.isTracked, out bool isTracked);
        IsTracked = isTracked;
        _device.TryGetFeatureValue(CommonUsages.devicePosition, out Vector3 position);
        Position = position;
        _device.TryGetFeatureValue(CommonUsages.deviceRotation, out Quaternion rotation);
        Rotation = rotation;
    }

    /// <summary>
    /// XRデバイスを初期化する
    /// </summary>
    /// <param name="device"></param>
    public virtual void Initialize(InputDevice device)
    {
        _device = device;
    }
}
