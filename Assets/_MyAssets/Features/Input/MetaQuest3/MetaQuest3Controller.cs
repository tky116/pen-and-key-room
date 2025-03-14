using UnityEngine;
using UnityEngine.XR;
using UnityEngine.Events;

/// <summary>
/// MetaQuest3コントローラの入力管理クラス
/// </summary>
public class MetaQuest3Controller : XRDeviceBase, IXRController
{
    public bool IsLeftHand { get; private set; }    // 左手かどうか

    // ボタン入力（値参照）
    public bool PrimaryButton { get; private set; }     // プライマリボタン
    public bool SecondaryButton { get; private set; }   // セカンダリボタン
    public bool MenuButton { get; private set; }        // メニューボタン
    public bool TriggerButton { get; private set; }     // トリガーボタン
    public bool GripButton { get; private set; }        // グリップボタン

    // トグル状態
    public bool PrimaryButtonToggle { get; private set; }    // プライマリボタントグル
    public bool SecondaryButtonToggle { get; private set; }  // セカンダリボタントグル
    public bool MenuButtonToggle { get; private set; }       // メニューボタントグル
    public bool TriggerButtonToggle { get; private set; }    // トリガーボタントグル
    public bool GripButtonToggle { get; private set; }       // グリップボタントグル

    // アナログ入力（値参照）
    public Vector2 ThumbstickValue { get; private set; }    // スティックの値
    public float TriggerValue { get; private set; }         // トリガーの値
    public float GripValue { get; private set; }            // グリップの値

    // イベント（状態参照）
    public UnityEvent<bool> onPrimaryButtonChanged = new UnityEvent<bool>();    // プライマリボタン
    public UnityEvent<bool> onSecondaryButtonChanged = new UnityEvent<bool>();  // セカンダリボタン
    public UnityEvent<bool> onMenuButtonChanged = new UnityEvent<bool>();       // メニューボタン
    public UnityEvent<bool> onTriggerButtonChanged = new UnityEvent<bool>();    // トリガーボタン
    public UnityEvent<bool> onGripButtonChanged = new UnityEvent<bool>();       // グリップボタン
    public UnityEvent<Vector2> onThumbstickChanged = new UnityEvent<Vector2>(); // スティックの値
    public UnityEvent<float> onTriggerValueChanged = new UnityEvent<float>();   // トリガーの値
    public UnityEvent<float> onGripValueChanged = new UnityEvent<float>();      // グリップの値

    // トグルイベント
    public UnityEvent<bool> onPrimaryButtonToggled = new UnityEvent<bool>();    // プライマリボタントグル
    public UnityEvent<bool> onSecondaryButtonToggled = new UnityEvent<bool>();  // セカンダリボタントグル
    public UnityEvent<bool> onMenuButtonToggled = new UnityEvent<bool>();       // メニューボタントグル
    public UnityEvent<bool> onTriggerButtonToggled = new UnityEvent<bool>();    // トリガーボタントグル
    public UnityEvent<bool> onGripButtonToggled = new UnityEvent<bool>();       // グリップボタントグル

    // 前フレームの状態
    private bool _prevPrimaryButton;    // プライマリボタン
    private bool _prevSecondaryButton;  // セカンダリボタン
    private bool _prevMenuButton;       // メニューボタン
    private bool _prevTriggerButton;    // トリガーボタン
    private bool _prevGripButton;       // グリップボタン
    private Vector2 _prevThumbstick;    // スティックの値
    private float _prevTriggerValue;    // トリガーの値
    private float _prevGripValue;       // グリップの値

    /// <summary>
    /// XRデバイスを初期化する
    /// </summary>
    /// <param name="device"></param>
    public override void Initialize(InputDevice device)
    {
        base.Initialize(device);
        IsLeftHand = device.characteristics.HasFlag(InputDeviceCharacteristics.Left);
    }

    /// <summary>
    /// 状態を更新する
    /// </summary>
    public override void UpdateState()
    {
        base.UpdateState();
        if (!IsValid) return;

        // プライマリボタン
        _device.TryGetFeatureValue(CommonUsages.primaryButton, out bool primaryButton);
        if (primaryButton != _prevPrimaryButton)
        {
            PrimaryButton = primaryButton;
            onPrimaryButtonChanged?.Invoke(primaryButton);

            // トグル処理
            if (primaryButton && !_prevPrimaryButton)
            {
                PrimaryButtonToggle = !PrimaryButtonToggle;
                onPrimaryButtonToggled?.Invoke(PrimaryButtonToggle);
            }

            _prevPrimaryButton = primaryButton;
        }

        // セカンダリボタン
        _device.TryGetFeatureValue(CommonUsages.secondaryButton, out bool secondaryButton);
        if (secondaryButton != _prevSecondaryButton)
        {
            SecondaryButton = secondaryButton;
            onSecondaryButtonChanged?.Invoke(secondaryButton);

            // トグル処理
            if (secondaryButton && !_prevSecondaryButton)
            {
                SecondaryButtonToggle = !SecondaryButtonToggle;
                onSecondaryButtonToggled?.Invoke(SecondaryButtonToggle);
            }

            _prevSecondaryButton = secondaryButton;
        }

        // メニューボタン
        _device.TryGetFeatureValue(CommonUsages.menuButton, out bool menuButton);
        if (menuButton != _prevMenuButton)
        {
            MenuButton = menuButton;
            onMenuButtonChanged?.Invoke(menuButton);

            // トグル処理
            if (menuButton && !_prevMenuButton)
            {
                MenuButtonToggle = !MenuButtonToggle;
                onMenuButtonToggled?.Invoke(MenuButtonToggle);
            }

            _prevMenuButton = menuButton;
        }

        // トリガーボタン
        _device.TryGetFeatureValue(CommonUsages.triggerButton, out bool triggerButton);
        if (triggerButton != _prevTriggerButton)
        {
            TriggerButton = triggerButton;
            onTriggerButtonChanged?.Invoke(triggerButton);

            // トグル処理
            if (triggerButton && !_prevTriggerButton)
            {
                TriggerButtonToggle = !TriggerButtonToggle;
                onTriggerButtonToggled?.Invoke(TriggerButtonToggle);
            }

            _prevTriggerButton = triggerButton;
        }

        // グリップボタン
        _device.TryGetFeatureValue(CommonUsages.gripButton, out bool gripButton);
        if (gripButton != _prevGripButton)
        {
            GripButton = gripButton;
            onGripButtonChanged?.Invoke(gripButton);

            // トグル処理
            if (gripButton && !_prevGripButton)
            {
                GripButtonToggle = !GripButtonToggle;
                onGripButtonToggled?.Invoke(GripButtonToggle);
            }

            _prevGripButton = gripButton;
        }

        // スティックの値
        _device.TryGetFeatureValue(CommonUsages.primary2DAxis, out Vector2 thumbstick);
        if (thumbstick != _prevThumbstick)
        {
            ThumbstickValue = thumbstick;
            onThumbstickChanged?.Invoke(thumbstick);
            _prevThumbstick = thumbstick;
        }

        // トリガーの値
        _device.TryGetFeatureValue(CommonUsages.trigger, out float triggerValue);
        if (!Mathf.Approximately(triggerValue, _prevTriggerValue))
        {
            TriggerValue = triggerValue;
            onTriggerValueChanged?.Invoke(triggerValue);
            _prevTriggerValue = triggerValue;
        }

        // グリップの値
        _device.TryGetFeatureValue(CommonUsages.grip, out float gripValue);
        if (!Mathf.Approximately(gripValue, _prevGripValue))
        {
            GripValue = gripValue;
            onGripValueChanged?.Invoke(gripValue);
            _prevGripValue = gripValue;
        }
    }

    /// <summary>
    /// ハプティックインパルスを送信する
    /// </summary>
    /// <param name="amplitude"></param>
    /// <param name="duration"></param>
    public void SendHapticImpulse(float amplitude, float duration)
    {
        if (!IsValid) return;
        _device.SendHapticImpulse(0, amplitude, duration);
    }

    /// <summary>
    /// トグル状態をリセットする
    /// </summary>
    public void ResetToggles()
    {
        PrimaryButtonToggle = false;
        SecondaryButtonToggle = false;
        MenuButtonToggle = false;
        TriggerButtonToggle = false;
        GripButtonToggle = false;

        onPrimaryButtonToggled?.Invoke(false);
        onSecondaryButtonToggled?.Invoke(false);
        onMenuButtonToggled?.Invoke(false);
        onTriggerButtonToggled?.Invoke(false);
        onGripButtonToggled?.Invoke(false);
    }
}
