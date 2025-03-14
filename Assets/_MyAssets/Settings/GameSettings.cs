using UnityEngine;

/// <summary>
/// ゲーム設定を管理するクラス
/// </summary>
public static class GameSettings
{
    public enum Handedness
    {
        RightHanded = 0,
        LeftHanded = 1
    }

    private static class PrefsKeys
    {
        public const string HANDEDNESS = "PlayerHandedness";
        /*
         * 将来的に他のデータを保存する場合はここにキーを追加
         */
    }

    /// <summary>
    /// 利き手設定の保存
    /// </summary>
    public static void SaveHandedness(bool isLeftHanded)
    {
        PlayerPrefs.SetInt(PrefsKeys.HANDEDNESS,
            isLeftHanded ? (int)Handedness.LeftHanded : (int)Handedness.RightHanded);
        PlayerPrefs.Save();
    }

    /// <summary>
    /// 利き手設定の読み込み
    /// </summary>
    public static bool LoadHandedness(bool defaultValue = false)
    {
        if (!PlayerPrefs.HasKey(PrefsKeys.HANDEDNESS))
        {
            return defaultValue;
        }
        return PlayerPrefs.GetInt(PrefsKeys.HANDEDNESS) == (int)Handedness.LeftHanded;
    }

    /// <summary>
    /// 全設定の削除
    /// </summary>
    public static void ClearAll()
    {
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();
    }

    /// <summary>
    /// 指定した設定の削除
    /// </summary>
    public static void ClearHandedness()
    {
        PlayerPrefs.DeleteKey(PrefsKeys.HANDEDNESS);
        PlayerPrefs.Save();
    }
}
