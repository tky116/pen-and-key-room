using UnityEngine;
/// <summary>
/// MetaQuest3の入力管理クラス
/// </summary>
public class MetaQuest3Input : XRInputManager
{
    public MetaQuestHMD QuestHMD => _hmd as MetaQuestHMD;
    public MetaQuest3Controller LeftQuestController => _leftController as MetaQuest3Controller;
    public MetaQuest3Controller RightQuestController => _rightController as MetaQuest3Controller;
    public enum HandPreference { Right, Left }
    public HandPreference CurrentHandPreference { get; private set; } = HandPreference.Right;
    protected override void OnEnable()
    {
        _hmd = new MetaQuestHMD();
        _leftController = new MetaQuest3Controller();
        _rightController = new MetaQuest3Controller();
        base.OnEnable();
    }
    /// <summary>
    /// 利き手の設定を変更する
    /// </summary>
    public void SetHandPreference(HandPreference hand)
    {
        CurrentHandPreference = hand;
        PlayerPrefs.SetInt("HandPreference", (int)hand);
        PlayerPrefs.Save();
    }
    /// <summary>
    /// 利き手の設定をロード
    /// </summary>
    private void LoadHandPreference()
    {
        if (PlayerPrefs.HasKey("HandPreference"))
        {
            CurrentHandPreference = (HandPreference)PlayerPrefs.GetInt("HandPreference");
        }
    }
}
