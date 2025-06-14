// ===== 1. 인터페이스 정의 (Interface Segregation Principle) =====

using System.Collections.Generic;
using UnityEngine;
using RPG.Gacha.Base;
using RPG.Common;
using RPG.Gacha.Interfaces;



namespace RPG.Gacha.Core
{
    /// <summary>
    /// 장비 가챠 실행자
    /// </summary>
    public class EquipmentGachaExecutor : MonoBehaviour, IGachaExecutor
    {
        [SerializeField] private EquipmentGachaSystem gachaSystem;
        [SerializeField] private int cost1Pull = 100;
        [SerializeField] private int cost11Pull = 1000;
        [SerializeField] private int cost55Pull = 4500;

        private IGachaCostHandler costHandler;

        public void Initialize(IGachaCostHandler handler)
        {
            costHandler = handler;
            if (gachaSystem == null)
                gachaSystem = GetComponent<EquipmentGachaSystem>();
        }

        public GachaResultData Execute(int pullCount)
        {
            int cost = GetCost(pullCount);

            if (!costHandler.TryConsume(cost))
                return null;

            List<IGachaItem> results = new List<IGachaItem>();

            switch (pullCount)
            {
                case 1:
                    var single = gachaSystem.PullSingle();
                    if (single != null) results.Add(single);
                    break;
                case 11:
                    results.AddRange(gachaSystem.Pull11());
                    break;
                case 55:
                    results.AddRange(gachaSystem.Pull55());
                    break;
            }

            return new GachaResultData(results, pullCount, CurrencyType.Diamond, cost);
        }

        public bool CanExecute(int pullCount)
        {
            return costHandler.CanAfford(GetCost(pullCount));
        }

        public int GetCost(int pullCount)
        {
            return pullCount switch
            {
                1 => cost1Pull,
                11 => cost11Pull,
                55 => cost55Pull,
                _ => cost1Pull * pullCount
            };
        }
    }
}

// ===== 3. 가챠 비용 처리자 (Single Responsibility) =====

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