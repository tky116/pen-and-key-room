using System.Collections.Generic;

/// <summary>
/// ボタン名と、通常モード・描画モードでどのボタンを表示するかを一元管理するクラス
/// </summary>
public static class ControllerUIConfig
{
    public enum Mode
    {
        Normal,
        Drawing
    }

    public enum LeftControllerButton
    {
        Primary,    // Xボタン
        Secondary,  // Yボタン
        Menu,       // Menuボタン
        Grip,       // Gripボタン
        Trigger     // Triggerボタン
    }

    public enum RightControllerButton
    {
        Primary,    // Aボタン
        Secondary,  // Bボタン
        Oculus,     // Oculusボタン
        Grip,       // Gripボタン
        Trigger     // Triggerボタン
    }

    private static readonly Dictionary<Mode, Dictionary<bool, List<object>>> buttonVisibility =
        new Dictionary<Mode, Dictionary<bool, List<object>>>
        {
            {
                // 通常モード
                Mode.Normal, new Dictionary<bool, List<object>>
                {
                    // 左利き用
                    { true, new List<object> {
                        LeftControllerButton.Menu,
                        LeftControllerButton.Grip,
                        LeftControllerButton.Trigger,
                        RightControllerButton.Secondary,
                        RightControllerButton.Grip,
                        RightControllerButton.Trigger
                    }},
                    // 右利き用
                    { false, new List<object> {
                        LeftControllerButton.Secondary,
                        LeftControllerButton.Menu,
                        LeftControllerButton.Grip,
                        LeftControllerButton.Trigger,
                        RightControllerButton.Grip,
                        RightControllerButton.Trigger
                    }}
                }
            },
            {
                // 描画モード
                Mode.Drawing, new Dictionary<bool, List<object>>
                {
                    // 左利き用（右コントローラーのみ表示）
                    { true, new List<object> {
                        RightControllerButton.Secondary,
                        RightControllerButton.Trigger
                    }},
                    // 右利き用（左コントローラーのみ表示）
                    { false, new List<object> {
                        LeftControllerButton.Secondary,
                        LeftControllerButton.Trigger
                    }}
                }
            }
        };

    private static readonly Dictionary<Mode, Dictionary<bool, Dictionary<object, string>>> buttonLabels =
        new Dictionary<Mode, Dictionary<bool, Dictionary<object, string>>>
        {
            {
                // 通常モード
                Mode.Normal, new Dictionary<bool, Dictionary<object, string>>
                {
                    // 左利き用
                    { true, new Dictionary<object, string> {
                        { LeftControllerButton.Primary, "" },
                        { LeftControllerButton.Secondary, "" },
                        { LeftControllerButton.Menu, "メニュー" },
                        { LeftControllerButton.Grip, "つかむ" },
                        { LeftControllerButton.Trigger, "選択" },
                        { RightControllerButton.Primary, "" },
                        { RightControllerButton.Secondary, "描画モード" },
                        { RightControllerButton.Oculus, "" },
                        { RightControllerButton.Grip, "つかむ" },
                        { RightControllerButton.Trigger, "選択" }
                    }},
                    // 右利き用
                    { false, new Dictionary<object, string> {
                        { LeftControllerButton.Primary, "" },
                        { LeftControllerButton.Secondary, "描画モード" },
                        { LeftControllerButton.Menu, "メニュー" },
                        { LeftControllerButton.Grip, "つかむ" },
                        { LeftControllerButton.Trigger, "選択" },
                        { RightControllerButton.Primary, "" },
                        { RightControllerButton.Secondary, "" },
                        { RightControllerButton.Oculus, "" },
                        { RightControllerButton.Grip, "つかむ" },
                        { RightControllerButton.Trigger, "選択" }
                    }}
                }
            },
            {
                // 描画モード
                Mode.Drawing, new Dictionary<bool, Dictionary<object, string>>
                {
                    // 左利き用（右コントローラーのみ）
                    { true, new Dictionary<object, string> {
                        { RightControllerButton.Primary, "" },
                        { RightControllerButton.Secondary, "描画終了" },
                        { RightControllerButton.Oculus, "" },
                        { RightControllerButton.Grip, "" },
                        { RightControllerButton.Trigger, "選択" }
                    }},
                    // 右利き用（左コントローラーのみ）
                    { false, new Dictionary<object, string> {
                        { LeftControllerButton.Primary, "" },
                        { LeftControllerButton.Secondary, "描画終了" },
                        { LeftControllerButton.Menu, "" },
                        { LeftControllerButton.Grip, "" },
                        { LeftControllerButton.Trigger, "選択" }
                    }}
                }
            }
        };

    public static string GetButtonLabel(Mode mode, bool isLeftHanded, object button)
    {
        if (buttonLabels.ContainsKey(mode) &&
            buttonLabels[mode].ContainsKey(isLeftHanded) &&
            buttonLabels[mode][isLeftHanded].ContainsKey(button))
        {
            return buttonLabels[mode][isLeftHanded][button];
        }
        return "";
    }

    public static List<object> GetVisibleButtons(Mode mode, bool isLeftHanded)
    {
        if (buttonVisibility.ContainsKey(mode) &&
            buttonVisibility[mode].ContainsKey(isLeftHanded))
        {
            return buttonVisibility[mode][isLeftHanded];
        }
        return new List<object>();
    }
}
