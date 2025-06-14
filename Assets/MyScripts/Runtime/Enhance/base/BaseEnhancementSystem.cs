using System;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using RPG.Common;

namespace RPG.Enhancement.Base
{
    /// <summary>
    /// ��ȭ �ý����� �⺻ �������̽�
    /// </summary>
    public interface IEnhanceable
    {
        void ApplyEnhancement(EnhancementData enhancement);
        void ResetEnhancements();
        float GetTotalBonus(StatType statType);
    }

    /// <summary>
    /// ��ȭ �ý��� ���̽� Ŭ����
    /// </summary>
    public abstract class BaseEnhancementSystem<T> : MonoBehaviour, IEnhanceable where T : class
    {
        [Title("Enhancement Levels")]
        [SerializeField, TableList(ShowIndexLabels = true, DrawScrollView = true, MaxScrollViewHeight = 300)]
        protected List<StatEnhancementLevel> enhancementLevels = new List<StatEnhancementLevel>();

        [ShowInInspector, ReadOnly]
        protected bool isInitialized = false;

        // ��ȭ ���
        protected T targetObject;

        // �̺�Ʈ
        public event Action<StatType, int> OnStatEnhanced;
        public event Action<StatType> OnStatMaxed;
        public event Action OnEnhancementChanged;

        /// <summary>
        /// �ʱ�ȭ (���� Ŭ�������� ����)
        /// </summary>
        public abstract void Initialize(T target);

        /// <summary>
        /// ���� ���� ���� (���� Ŭ�������� ����)
        /// </summary>
        protected abstract void SaveOriginalStats();

        /// <summary>
        /// ���� ���� (���� Ŭ�������� ����)
        /// </summary>
        protected abstract void RecalculateStats();

        /// <summary>
        /// ��ȭ ���� �ʱ�ȭ
        /// </summary>
        protected virtual void InitializeEnhancementLevels()
        {
            // �⺻ ���� - ���� Ŭ�������� �������̵� ����
        }

        /// <summary>
        /// Ư�� ���� ��ȭ
        /// </summary>
        public virtual bool EnhanceStat(StatType statType, int levels = 1)
        {
            if (!isInitialized)
            {
                Debug.LogError($"{GetType().Name}�� �ʱ�ȭ���� �ʾҽ��ϴ�!");
                return false;
            }

            var enhancementLevel = GetEnhancementLevel(statType);
            if (enhancementLevel == null) return false;

            int actualLevels = Mathf.Min(levels, enhancementLevel.maxLevel - enhancementLevel.currentLevel);
            if (actualLevels <= 0)
            {
                Debug.LogWarning($"{statType}��(��) �̹� �ִ� �����Դϴ�!");
                return false;
            }

            enhancementLevel.currentLevel += actualLevels;
            RecalculateStats();

            // �̺�Ʈ �߻�
            OnStatEnhanced?.Invoke(statType, enhancementLevel.currentLevel);

            if (enhancementLevel.currentLevel >= enhancementLevel.maxLevel)
            {
                OnStatMaxed?.Invoke(statType);
            }

            OnEnhancementChanged?.Invoke();

            Debug.Log($"{statType} ��ȭ �Ϸ�! (Lv.{enhancementLevel.currentLevel})");
            return true;
        }

        /// <summary>
        /// ��ȭ ���� (�⺻ ����)
        /// </summary>
        public virtual void ApplyEnhancement(EnhancementData enhancement)
        {
            // ���� Ŭ�������� ��ü���� ����
        }

        /// <summary>
        /// ��ȭ �ʱ�ȭ
        /// </summary>
        public virtual void ResetEnhancements()
        {
            if (!isInitialized)
            {
                Debug.LogError($"{GetType().Name}�� �ʱ�ȭ���� �ʾҽ��ϴ�!");
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
        /// ��ȭ ���� ��������
        /// </summary>
        public StatEnhancementLevel GetEnhancementLevel(StatType statType)
        {
            return enhancementLevels.Find(e => e.statType == statType);
        }

        /// <summary>
        /// ��ü ���ʽ� ���
        /// </summary>
        public virtual float GetTotalBonus(StatType statType)
        {
            var enhancement = GetEnhancementLevel(statType);
            return enhancement?.GetEnhancementValue() ?? 0f;
        }

        /// <summary>
        /// ��ȭ ��� ���
        /// </summary>
        public virtual long CalculateEnhanceCost(StatType statType, int currentLevel, float baseCost, float multiplier)
        {
            return (long)(baseCost * Mathf.Pow(multiplier, currentLevel));
        }

        /// <summary>
        /// ���� ��ȭ ��� ���
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
        /// ����� ���� ���
        /// </summary>
        [Button("Show Enhancement Info", ButtonSizes.Large)]
        public virtual void DebugEnhancementInfo()
        {
            if (!isInitialized)
            {
                Debug.LogWarning($"{GetType().Name}�� ���� �ʱ�ȭ���� �ʾҽ��ϴ�!");
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
        /// ��ü ��ȭ ���൵
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