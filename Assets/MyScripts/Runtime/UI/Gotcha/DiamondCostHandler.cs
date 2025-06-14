// ===== 1. 인터페이스 정의 (Interface Segregation Principle) =====

using UnityEngine;
using RPG.Common;
using RPG.Gacha.Interfaces;
using RPG.Managers;




// ===== 3. 가챠 비용 처리자 (Single Responsibility) =====
namespace RPG.Gacha.Core
{
    /// <summary>
    /// 다이아몬드 비용 처리자
    /// </summary>
    public class DiamondCostHandler : MonoBehaviour, IGachaCostHandler
    {
        private CurrencyManager currencyManager;

        private void Start()
        {
            currencyManager = FindObjectOfType<CurrencyManager>();
        }

        public bool CanAfford(int cost)
        {
            return currencyManager != null && currencyManager.CanAfford(CurrencyType.Diamond, cost);
        }

        public bool TryConsume(int cost)
        {
            return currencyManager != null && currencyManager.TrySpend(CurrencyType.Diamond, cost);
        }

        public int GetCurrentAmount()
        {
            return currencyManager != null ? (int)currencyManager.Diamond : 0;
        }
    }
}

// ===== 4. 가챠 선택 팝업 (UI만 담당) =====



// ===== 7. 사용 예시 =====

/*
1. PopupManager의 PopupType에 추가:
   - Gacha (가챠 선택 팝업)
   - GachaResult (가챠 결과 팝업)

2. 씬 구성:
   - GachaFlowController (GameObject)
     ├── EquipmentGachaExecutor (Component)
     ├── DiamondCostHandler (Component)
     └── PopupManager 참조

3. 프리팹 구성:
   - GachaSelectionPopup (선택 UI만)
   - GachaResultPopup (결과 표시만)

4. 실행:
   gachaFlowController.OpenGachaSelection();
*/