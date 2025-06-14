using System;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using RPG.Common;

namespace RPG.Enhancement.Base
{
    /// <summary>
    /// 강화 시스템의 기본 인터페이스
    /// </summary>
    public interface IEnhanceable
    {
        void ApplyEnhancement(EnhancementData enhancement);
        void ResetEnhancements();
        float GetTotalBonus(StatType statType);
    }

    /// <summary>
    /// 강화 시스템 베이스 클래스
    /// </summary>
    public abstract class BaseEnhancementSystem<T> : MonoBehaviour, IEnhanceable where T : class
    {
        [Title("Enhancement Levels")]
        [SerializeField, TableList(ShowIndexLabels = true, DrawScrollView = true, MaxScrollViewHeight = 300)]
        protected List<StatEnhancementLevel> enhancementLevels = new List<StatEnhancementLevel>();

        [ShowInInspector, ReadOnly]
        protected bool isInitialized = false;

        // 강화 대상
        protected T targetObject;

        // 이벤트
        public event Action<StatType, int> OnStatEnhanced;
        public event Action<StatType> OnStatMaxed;
        public event Action OnEnhancementChanged;

        /// <summary>
        /// 초기화 (하위 클래스에서 구현)
        /// </summary>
        public abstract void Initialize(T target);

        /// <summary>
        /// 원본 스탯 저장 (하위 클래스에서 구현)
        /// </summary>
        protected abstract void SaveOriginalStats();

        /// <summary>
        /// 스탯 재계산 (하위 클래스에서 구현)
        /// </summary>
        protected abstract void RecalculateStats();

        /// <summary>
        /// 강화 레벨 초기화
        /// </summary>
        protected virtual void InitializeEnhancementLevels()
        {
            // 기본 구현 - 하위 클래스에서 오버라이드 가능
        }

        /// <summary>
        /// 특정 스탯 강화
        /// </summary>
        public virtual bool EnhanceStat(StatType statType, int levels = 1)
        {
            if (!isInitialized)
            {
                Debug.LogError($"{GetType().Name}이 초기화되지 않았습니다!");
                return false;
            }

            var enhancementLevel = GetEnhancementLevel(statType);
            if (enhancementLevel == null) return false;

            int actualLevels = Mathf.Min(levels, enhancementLevel.maxLevel - enhancementLevel.currentLevel);
            if (actualLevels <= 0)
            {
                Debug.LogWarning($"{statType}은(는) 이미 최대 레벨입니다!");
                return false;
            }

            enhancementLevel.currentLevel += actualLevels;
            RecalculateStats();

            // 이벤트 발생
            OnStatEnhanced?.Invoke(statType, enhancementLevel.currentLevel);

            if (enhancementLevel.currentLevel >= enhancementLevel.maxLevel)
            {
                OnStatMaxed?.Invoke(statType);
            }

            OnEnhancementChanged?.Invoke();

            Debug.Log($"{statType} 강화 완료! (Lv.{enhancementLevel.currentLevel})");
            return true;
        }

        /// <summary>
        /// 강화 적용 (기본 구현)
        /// </summary>
        public virtual void ApplyEnhancement(EnhancementData enhancement)
        {
            // 하위 클래스에서 구체적인 구현
        }

        /// <summary>
        /// 강화 초기화
        /// </summary>
        public virtual void ResetEnhancements()
        {
            if (!isInitialized)
            {
                Debug.LogError($"{GetType().Name}이 초기화되지 않았습니다!");
                return;
            }

            foreach (var enhancement in enhancementLevels)
            {
                enhancement.currentLevel = 0;
            }

            RecalculateStats();
            OnEnhancementChanged?.Invoke();
        }

        /// <summary>
        /// 강화 정보 가져오기
        /// </summary>
        public StatEnhancementLevel GetEnhancementLevel(StatType statType)
        {
            return enhancementLevels.Find(e => e.statType == statType);
        }

        /// <summary>
        /// 전체 보너스 계산
        /// </summary>
        public virtual float GetTotalBonus(StatType statType)
        {
            var enhancement = GetEnhancementLevel(statType);
            return enhancement?.GetEnhancementValue() ?? 0f;
        }

        /// <summary>
        /// 강화 비용 계산
        /// </summary>
        public virtual long CalculateEnhanceCost(StatType statType, int currentLevel, float baseCost, float multiplier)
        {
            return (long)(baseCost * Mathf.Pow(multiplier, currentLevel));
        }

        /// <summary>
        /// 다중 강화 비용 계산
        /// </summary>
        public virtual long CalculateMultiEnhanceCost(StatType statType, int currentLevel, int count, float baseCost, float multiplier)
        {
            long totalCost = 0;
            for (int i = 0; i < count; i++)
            {
                totalCost += CalculateEnhanceCost(statType, currentLevel + i, baseCost, multiplier);
            }
            return totalCost;
        }

        /// <summary>
        /// 디버그 정보 출력
        /// </summary>
        [Button("Show Enhancement Info", ButtonSizes.Large)]
        public virtual void DebugEnhancementInfo()
        {
            if (!isInitialized)
            {
                Debug.LogWarning($"{GetType().Name}이 아직 초기화되지 않았습니다!");
                return;
            }

            Debug.Log($"========== {GetType().Name} Info ==========");
            foreach (var enhancement in enhancementLevels)
            {
                string color = enhancement.currentLevel >= enhancement.maxLevel ? "yellow" : "white";
                Debug.Log($"<color={color}>{enhancement.statType}: Lv.{enhancement.currentLevel}/{enhancement.maxLevel} " +
                         $"(+{enhancement.GetEnhancementValue()}{(enhancement.isPercentage ? "%" : "")})</color>");
            }
            Debug.Log("=====================================");
        }

        /// <summary>
        /// 전체 강화 진행도
        /// </summary>
        [ShowInInspector, ReadOnly, ProgressBar(0, 1, 0.5f, 0.5f, 1f)]
        protected float TotalEnhancementProgress
        {
            get
            {
                if (enhancementLevels == null || enhancementLevels.Count == 0) return 0;

                int totalCurrent = 0;
                int totalMax = 0;

                foreach (var enhancement in enhancementLevels)
                {
                    totalCurrent += enhancement.currentLevel;
                    totalMax += enhancement.maxLevel;
                }

                return totalMax > 0 ? (float)totalCurrent / totalMax : 0;
            }
        }
    }
}