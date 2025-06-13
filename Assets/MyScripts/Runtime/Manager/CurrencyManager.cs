using UnityEngine;
using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using RPG.Common;
using RPG.Core.Events;
namespace RPG.Managers
{

    // 화폐 관리자 (Single Responsibility)
    public class CurrencyManager : MonoBehaviour
    {
        [Title("화폐 현황")]
        [ShowInInspector, ReadOnly]
        [DictionaryDrawerSettings(KeyLabel = "화폐", ValueLabel = "보유량")]
        private Dictionary<CurrencyType, long> currencies = new Dictionary<CurrencyType, long>
    {
        { CurrencyType.Gold, 0 },
        { CurrencyType.Diamond, 0 },
        { CurrencyType.Energy, 100 },
        { CurrencyType.SoulStone, 0 }
    };

        public long Gold => currencies[CurrencyType.Gold];
        public long Diamond => currencies[CurrencyType.Diamond];
        public long Energy => currencies[CurrencyType.Energy];
        public long SoulStone => currencies[CurrencyType.SoulStone];

      
        private void Start()
        {
            // 초기 화폐 설정
            AddCurrency(CurrencyType.Gold, 1000);
            AddCurrency(CurrencyType.Diamond, 100);
        }

        public bool CanAfford(CurrencyType type, long amount)
        {
            return currencies.ContainsKey(type) && currencies[type] >= amount;
        }

        public void AddCurrency(CurrencyType type, long amount)
        {
            if (!currencies.ContainsKey(type)) return;

            currencies[type] += amount;

            // 중앙 이벤트 시스템으로 전파
            GameEventManager.TriggerCurrencyChanged(type, currencies[type]);
        }

        public bool TrySpend(CurrencyType type, long amount)
        {
            if (!CanAfford(type, amount)) return false;

            currencies[type] -= amount;
            GameEventManager.TriggerCurrencyChanged(type, currencies[type]);
            return true;
        }
        [Title("디버그")]
        [Button("골드 추가 (+10,000)", ButtonSizes.Medium)]
        [GUIColor(1f, 0.8f, 0.3f)]
        private void DebugAddGold()
        {
            AddCurrency(CurrencyType.Gold, 10000);
        }

        [Button("다이아몬드 추가 (+100)", ButtonSizes.Medium)]
        [GUIColor(0.3f, 0.8f, 1f)]
        private void DebugAddDiamond()
        {
            AddCurrency(CurrencyType.Diamond, 100);
        }
    }

}