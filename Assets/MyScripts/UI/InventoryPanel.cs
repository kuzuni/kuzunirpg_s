using UnityEngine;
using Sirenix.OdinInspector;
// 인벤토리 패널
public class InventoryPanel : BaseUIPanel
{
    [Title("인벤토리")]
    [TabGroup("Items", "장비")]
    [SerializeField] private EquipmentInventorySystem equipmentInventory;
    
    [TabGroup("Items", "유물")]
    [SerializeField] private RelicInventorySystem relicInventory;
    
    [ShowInInspector, ReadOnly]
    [TabGroup("Items", "장비")]
    private int equipmentCount;
    
    [ShowInInspector, ReadOnly]
    [TabGroup("Items", "유물")]
    private int relicCount;
    
    public override void UpdatePanel()
    {
        if (equipmentInventory != null)
        {
            equipmentCount = equipmentInventory.GetTotalItemCount();
        }
        
        if (relicInventory != null)
        {
            relicCount = relicInventory.GetTotalItemCount();
        }
        
        RefreshInventoryUI();
    }
    
    private void RefreshInventoryUI()
    {
        Debug.Log($"인벤토리 새로고침 - 장비: {equipmentCount}개, 유물: {relicCount}개");
    }
    
    [Button("정렬", ButtonSizes.Medium)]
    [ButtonGroup("Actions")]
    private void SortInventory()
    {
        equipmentInventory?.SortInventory();
        relicInventory?.SortInventory();
        UpdatePanel();
    }
    
    [Button("일괄 판매", ButtonSizes.Medium)]
    [ButtonGroup("Actions")]
    [GUIColor(0.8f, 0.3f, 0.3f)]
    private void SellBulk()
    {
        Debug.Log("낮은 등급 아이템 일괄 판매");
    }
}
