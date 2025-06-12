using UnityEngine;
using Sirenix.OdinInspector;
// 상점 패널
public class ShopPanel : BaseUIPanel
{
    [Title("상점")]
    [TabGroup("Shop", "일반 상점")]
    [TabGroup("Shop", "프리미엄 상점")]
    [TabGroup("Shop", "이벤트 상점")]
    
    public override void UpdatePanel()
    {
        RefreshShopItems();
    }
    
    private void RefreshShopItems()
    {
        Debug.Log("상점 아이템 새로고침");
    }
}
