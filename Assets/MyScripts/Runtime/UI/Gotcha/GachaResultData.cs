using System;
using System.Collections.Generic;
using UnityEngine;
using RPG.Gacha.Base;
using RPG.Common;

namespace RPG.Gacha.Interfaces
{
    /// <summary>
    /// 가챠 결과 데이터 - 런타임 전용
    /// </summary>
    public class GachaResultData
    {
        // 모든 필드를 프로퍼티로만 선언 (Unity가 직렬화하지 않음)
        public List<IGachaItem> Items { get; private set; }
        public int PullCount { get; private set; }
        public CurrencyType UsedCurrency { get; private set; }
        public int UsedAmount { get; private set; }

        public GachaResultData(List<IGachaItem> items, int pullCount, CurrencyType currency, int amount)
        {
            this.Items = items;
            this.PullCount = pullCount;
            this.UsedCurrency = currency;
            this.UsedAmount = amount;
        }
    }

    /// <summary>
    /// 가챠 실행 인터페이스
    /// </summary>
    public interface IGachaExecutor
    {
        GachaResultData Execute(int pullCount);
        bool CanExecute(int pullCount);
        int GetCost(int pullCount);
    }

    /// <summary>
    /// 가챠 결과 표시 인터페이스
    /// </summary>
    public interface IGachaResultDisplay
    {
        void DisplayResults(GachaResultData results);
        void ClearDisplay();
        event Action OnDisplayComplete;
    }


    /// <summary>
    /// 가챠 비용 처리 인터페이스
    /// </summary>
    public interface IGachaCostHandler
    {
        bool CanAfford(int cost);
        bool TryConsume(int cost);
        int GetCurrentAmount();
    }
}