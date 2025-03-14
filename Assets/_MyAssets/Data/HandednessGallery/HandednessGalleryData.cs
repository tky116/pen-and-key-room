using UnityEngine;

/// <summary>
/// 利き手設定に応じたギャラリーデータを管理するクラス
/// </summary>
[CreateAssetMenu(fileName = "HandednessGalleryData", menuName = "_MyAssets/Data/HandednessGallery")]
public class HandednessGalleryData : ScriptableObject
{
    [System.Serializable]
    public class HandednessGalleryItem
    {
        public string label;
        public string identifier;
        public Sprite image;
    }

    [Header("Right Handed Gallery Items")]
    [SerializeField] private HandednessGalleryItem[] rightHandedItems;

    [Header("Left Handed Gallery Items")]
    [SerializeField] private HandednessGalleryItem[] leftHandedItems;

    /// <summary>
    /// 現在の利き手設定に応じたギャラリーアイテムを取得
    /// </summary>
    public CyclicGalleryUI.GalleryData[] GetGalleryData(bool isLeftHanded)
    {
        var sourceItems = isLeftHanded ? leftHandedItems : rightHandedItems;
        var galleryData = new CyclicGalleryUI.GalleryData[sourceItems.Length];

        for (int i = 0; i < sourceItems.Length; i++)
        {
            galleryData[i] = new CyclicGalleryUI.GalleryData
            {
                label = sourceItems[i].label,
                identifier = sourceItems[i].identifier,
                image = sourceItems[i].image
            };
        }

        return galleryData;
    }
}
