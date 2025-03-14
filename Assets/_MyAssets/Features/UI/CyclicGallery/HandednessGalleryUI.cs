using UnityEngine;

/// <summary>
/// 利き手設定に応じたギャラリーUIを管理するコンポーネント
/// </summary>
public class HandednessGalleryUI : MonoBehaviour
{
    [SerializeField] private CyclicGalleryUI galleryUI;
    [SerializeField] private HandednessGalleryData galleryData;
    [SerializeField] private CyclicOptionUI handednessOptionUI;

    private void Start()
    {
        if (galleryUI == null || galleryData == null) return;

        bool isLeftHanded = GameSettings.LoadHandedness();
        UpdateGalleryItems(isLeftHanded);
    }

    private void OnEnable()
    {
        if (handednessOptionUI != null)
        {
            handednessOptionUI.OnOptionChanged += HandleHandednessChanged;
        }

        bool isLeftHanded = GameSettings.LoadHandedness();
        UpdateGalleryItems(isLeftHanded);
    }

    private void OnDisable()
    {
        if (handednessOptionUI != null)
        {
            handednessOptionUI.OnOptionChanged -= HandleHandednessChanged;
        }
    }

    /// <summary>
    /// 利き手設定が変更された際の処理
    /// </summary>
    /// <param name="identifier"></param>
    private void HandleHandednessChanged(string identifier)
    {
        bool isLeftHanded = identifier == "left";
        UpdateGalleryItems(isLeftHanded);
    }

    /// <summary>
    /// ギャラリーのアイテムを更新する
    /// </summary>
    /// <param name="isLeftHanded"></param>
    private void UpdateGalleryItems(bool isLeftHanded)
    {
        if (galleryUI == null || galleryData == null) return;

        var newItems = galleryData.GetGalleryData(isLeftHanded);
        if (newItems != null && newItems.Length > 0)
        {
            galleryUI.SetGalleryItems(newItems);
            galleryUI.SetItem(newItems[0].identifier);
        }
    }
}
