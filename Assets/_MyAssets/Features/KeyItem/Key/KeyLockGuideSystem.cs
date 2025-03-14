using UnityEngine;

public class KeyLockGuideSystem : MonoBehaviour
{
    [Header("Guide Arrows")]
    [SerializeField] private GameObject insertArrow;  // 「ここに挿してください」矢印
    [SerializeField] private GameObject rotateArrow;  // 「こう回してください」矢印

    private void Start()
    {
        // 初期状態では両方の矢印を非表示
        if (insertArrow) insertArrow.SetActive(false);
        if (rotateArrow) rotateArrow.SetActive(false);
    }

    // 鍵を掴んだとき（Keyのイベントから呼び出し）
    public void OnKeyGrabbed()
    {
        if (insertArrow) insertArrow.SetActive(true);
        if (rotateArrow) rotateArrow.SetActive(false);
    }

    // 鍵を離したとき（Keyのイベントから呼び出し）
    public void OnKeyReleased()
    {
        if (insertArrow) insertArrow.SetActive(false);
        if (rotateArrow) rotateArrow.SetActive(false);
    }

    // 鍵が挿入されたとき（KeyUnlockSystemから呼び出し）
    public void OnKeyInserted()
    {
        if (insertArrow) insertArrow.SetActive(false);
        if (rotateArrow) rotateArrow.SetActive(true);
    }

    // 鍵が解錠されたとき（KeyUnlockSystemから呼び出し）
    public void OnKeyUnlocked()
    {
        if (insertArrow) insertArrow.SetActive(false);
        if (rotateArrow) rotateArrow.SetActive(false);
    }
}
