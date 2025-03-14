using UnityEngine;

/// <summary>
/// XRデバイスのHMDインターフェース
/// </summary>
public interface IXRHMD
{
    bool IsValid { get; }       // デバイスが有効かどうか
    bool IsTracked { get; }     // トラッキングされているかどうか
    bool IsUserPresent { get; } // ユーザーがいるかどうか

    Vector3 Position { get; }               // 位置
    Quaternion Rotation { get; }            // 回転
    Vector3 CenterEyePosition { get; }      // 中心の目の位置
    Quaternion CenterEyeRotation { get; }   // 中心の目の回転
}
