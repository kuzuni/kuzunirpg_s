using System;
using UnityEngine;
using Sirenix.OdinInspector;

// 제네릭 인벤토리 슬롯
[Serializable]
public class InventorySlot<T> where T : class
{
    [HorizontalGroup("Slot", 0.15f)]
    [VerticalGroup("Slot/Icon")]
    [PreviewField(50), HideLabel]
    [ShowIf("@GetIcon() != null")]
    [ShowInInspector]
    public Sprite Icon => GetIcon();

    [VerticalGroup("Slot/Info")]
    [LabelText("아이템")]
    public T item;

    [VerticalGroup("Slot/Info")]
    [LabelText("수량")]
    [MinValue(1)]
    public int quantity = 1;

    // 기본 생성자 (제네릭 제약 조건을 위해 필요)
    public InventorySlot()
    {
        this.item = null;
        this.quantity = 1;
    }

    public InventorySlot(T item, int quantity = 1)
    {
        this.item = item;
        this.quantity = quantity;
    }

    private Sprite GetIcon()
    {
        if (item == null) return null;

        // IGachaItem 인터페이스를 구현한 경우
        if (item is IGachaItem gachaItem)
        {
            return gachaItem.Icon;
        }

        // RelicInstance인 경우
        if (item is RelicInstance relicInstance)
        {
            return relicInstance.Icon;
        }

        return null;
    }
}