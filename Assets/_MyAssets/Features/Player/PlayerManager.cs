using UnityEngine;
/// <summary>
/// プレイヤーを管理するクラス
/// </summary>
public class PlayerManager : MonoBehaviour
{
    public static PlayerManager Instance { get; private set; }  // シングルトンインスタンス
    public static event System.Action<bool> OnHandednessChanged;    // 利き手変更イベント
    [Header("Player Objects")]
    [SerializeField] private GameObject playerRightHanded;
    [SerializeField] private GameObject playerLeftHanded;
    [SerializeField] private bool isLeftHanded = false; // デフォルトは右利き
    public bool IsLeftHanded => isLeftHanded;   // 現在の利き手状態を取得できるようにする
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        isLeftHanded = GameSettings.LoadHandedness(isLeftHanded);   // 起動時に利き手設定を読み込む
    }
    private void Start()
    {
        UpdateHandedness(true);
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.H)) // 仮のキー (設定で変更可能)
        {
            ToggleChangeHandedness();
        }
    }
    /// <summary>
    /// 利き手の切り替え
    /// </summary>
    public void ToggleChangeHandedness()
    {
        isLeftHanded = !isLeftHanded;
        UpdateHandedness();
        GameSettings.SaveHandedness(isLeftHanded);  // 利き手設定を保存
    }
    /// <summary>
    /// 利き手の更新
    /// </summary>
    private void UpdateHandedness(bool isInitializing = false)
    {
        playerRightHanded.SetActive(!isLeftHanded);
        playerLeftHanded.SetActive(isLeftHanded);
        // 初期化時も含めて必ずイベントを発火
        OnHandednessChanged?.Invoke(isLeftHanded);
    }
}
