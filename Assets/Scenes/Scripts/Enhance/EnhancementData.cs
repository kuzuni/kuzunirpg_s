using System;
using UnityEngine;
using Sirenix.OdinInspector;

// ��ȭ ������
[Serializable]
public class EnhancementData
{
    public StatType statType;
    public float enhancementValue;
    public bool isPercentage; // true: �ۼ�Ʈ ����, false: ������ ����

    public EnhancementData(StatType type, float value, bool isPercent = false)
    {
        statType = type;
        enhancementValue = value;
        isPercentage = isPercent;
    }
}

// ��ȭ ���� ������
[Serializable]
public class StatEnhancementLevel
{
    [TableColumnWidth(100)]
    [ReadOnly] public StatType statType;

    [TableColumnWidth(80)]
    [ProgressBar(0, "@maxLevel", 0.5f, 0.8f, 0.5f)]
    public int currentLevel = 0;

    [TableColumnWidth(80)]
    public int maxLevel = 10;

    [TableColumnWidth(120)]
    [LabelText("Base Value")]
    public float baseEnhancementValue = 5f;

    [TableColumnWidth(80)]
    [LabelText("Is %")]
    public bool isPercentage = false;

    [TableColumnWidth(120)]
    [ShowInInspector, ReadOnly]
    [LabelText("Total Bonus")]
    public string TotalBonus => $"+{GetEnhancementValue()}{(isPercentage ? "%" : "")}";

    public float GetEnhancementValue()
    {
        return baseEnhancementValue * currentLevel;
    }
}