using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

/// <summary>
/// 画像とラベルを循環表示するギャラリーUIを管理するコンポーネント
/// </summary>
public class CyclicGalleryUI : MonoBehaviour
{
    [System.Serializable]
    public class GalleryData
    {
        public string label;         // 表示するラベル
        public string identifier;    // 選択肢の識別子
        public Sprite image;         // 表示する画像
    }

    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI titleLabel;    // タイトルのラベル
    [SerializeField] private Image displayImage;            // 表示用のImage

    [Header("Gallery")]
    [SerializeField] private GalleryData[] items;           // ギャラリーアイテムのリスト
    [SerializeField] private int defaultIndex = 0;          // デフォルトの選択インデックス

    private int currentIndex;                               // 現在選択中のインデックス
    public event Action<string> OnItemChanged;              // アイテム変更時のイベント

    private void Start()
    {
        // 初期選択を設定
        currentIndex = defaultIndex;
        UpdateDisplay();
    }

    /// <summary>
    /// 選択を切り替える
    /// </summary>
    public void OnSelect()
    {
        CycleItem();
    }

    /// <summary>
    /// アイテムを循環させる
    /// </summary>
    private void CycleItem()
    {
        currentIndex = (currentIndex + 1) % items.Length;
        UpdateDisplay();
        OnItemChanged?.Invoke(items[currentIndex].identifier);
    }

    /// <summary>
    /// 表示を更新する
    /// </summary>
    private void UpdateDisplay()
    {
        if (items != null && items.Length > 0)
        {
            titleLabel.text = items[currentIndex].label;
            displayImage.sprite = items[currentIndex].image;
        }
    }

    /// <summary>
    /// アイテムを直接設定する
    /// </summary>
    /// <param name="identifier">アイテムの識別子</param>
    public void SetItem(string identifier)
    {
        for (int i = 0; i < items.Length; i++)
        {
            if (items[i].identifier == identifier)
            {
                currentIndex = i;
                UpdateDisplay();
                break;
            }
        }
    }

    /// <summary>
    /// 現在選択中のアイテムのidentifierを取得
    /// </summary>
    public string GetCurrentIdentifier()
    {
        return items[currentIndex].identifier;
    }

    /// <summary>
    /// 現在選択中のアイテムのSpriteを取得
    /// </summary>
    public Sprite GetCurrentSprite()
    {
        return items[currentIndex].image;
    }

    /// <summary>
    /// ギャラリーアイテムを設定する
    /// </summary>
    /// <param name="newItems"></param>
    public void SetGalleryItems(GalleryData[] newItems)
    {
        if (newItems == null || newItems.Length == 0) return;
        items = newItems;
        currentIndex = 0;
        UpdateDisplay();
    }
}
