using UnityEngine;

/// <summary>
/// カラースロット管理
/// </summary>
public class ColorSlotsManager : MonoBehaviour
{
    [SerializeField] private ColorPicker colorPicker;
    [SerializeField] private ColorSaveButton colorSaveButton;
    private ColorSlot[] colorSlots;
    private ColorSlot selectedSlot;

    private void Awake()
    {
        colorSlots = GetComponentsInChildren<ColorSlot>();
        colorSaveButton.SetInteractable(false);
    }

    /// <summary>
    /// カラースロットのインタラクション
    /// </summary>
    public class ColorSlotInteractable : MonoBehaviour
    {
        [SerializeField] private ColorSlot colorSlot;
        [SerializeField] private ColorSlotsManager manager;

        public void OnSlotSelected()
        {
            manager.OnSlotSelected(colorSlot);
        }
    }

    /// <summary>
    /// スロットが選択された時、選択されたスロットを保持
    /// </summary>
    /// <param name="slot"></param>
    public void OnSlotSelected(ColorSlot slot)
    {
        if (selectedSlot == slot)
        {
            // 同じスロットを選択した場合は選択解除
            selectedSlot.SetSelected(false);
            selectedSlot = null;
            colorSaveButton.SetInteractable(false);
        }
        else
        {
            // 通常の選択処理
            if (selectedSlot != null)
            {
                selectedSlot.SetSelected(false);
                colorSaveButton.SetInteractable(false);
            }
            selectedSlot = slot;
            selectedSlot.SetSelected(true);
            colorPicker.SetCurrentColor(slot.GetColor());
            colorSaveButton.SetInteractable(true);
        }
    }

    /// <summary>
    /// 保存ボタンが押された時、選択されたスロットに現在の色を保存
    /// </summary>
    public void OnSaveButtonPressed()
    {
        if (selectedSlot != null && System.Array.Exists(colorSlots, s => s == selectedSlot))
        {
            selectedSlot.SetColor(colorPicker.GetCurrentColor());
            // オプション：保存後に選択を解除
            // selectedSlot.SetSelected(false);
            // selectedSlot = null;
        }
    }

    // ColorPickerでの色変更時のコールバック
    public void OnColorChanged(Color newColor)
    {
        // 何かあれば追加する
    }
}
