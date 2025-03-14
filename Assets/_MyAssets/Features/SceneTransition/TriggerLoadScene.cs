/// <summary>
/// シーン遷移をトリガーするクラス
/// </summary>
public class TriggerLoadScene : SceneTrigger
{
    private bool _isTriggered;

    protected override bool CheckTransitionCondition() => _isTriggered;

    public void LoadScene() => _isTriggered = true;
}
