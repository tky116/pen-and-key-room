using UnityEngine;

/// <summary>
/// XRコントローラーのインターフェース
/// </summary>
public interface IXRController
{
    bool IsLeftHand { get; }    // 左手かどうか
    bool IsValid { get; }       // 有効かどうか
    bool IsTracked { get; }     // トラッキングされているかどうか

    // ボタン入力
    bool PrimaryButton { get; }     // プライマリボタン
    bool SecondaryButton { get; }   // セカンダリボタン
    bool MenuButton { get; }        // メニューボタン
    bool TriggerButton { get; }     // トリガーボタン
    bool GripButton { get; }        // グリップボタン

    // アナログ入力
    Vector2 ThumbstickValue { get; }    // スティックの値
    float TriggerValue { get; }         // トリガーの値
    float GripValue { get; }            // グリップの値

    // 位置情報
    Vector3 Position { get; }       // 位置
    Quaternion Rotation { get; }    // 回転
}
