using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Sirenix.OdinInspector;
using System;

public class RelicEffectSystem : MonoBehaviour
{
    [Title("시스템 참조")]
    [SerializeField, Required]
    private PlayerStatus playerStatus;
    
    [SerializeField, Required]
    private RelicInventorySystem relicInventory;
    
    [Title("유물 효과 상태")]
    [ShowInInspector, ReadOnly]
    [DictionaryDrawerSettings(KeyLabel = "스탯", ValueLabel = "총 보너스")]
    private Dictionary<StatType, float> currentRelicBonuses = new Dictionary<StatType, float>();
    
    [ShowInInspector, ReadOnly]
    private bool isInitialized = false;
    
    // 원본 스탯 저장
    [ShowInInspector, ReadOnly]
    [DictionaryDrawerSettings(KeyLabel = "스탯", ValueLabel = "원본 값")]
    private Dictionary<StatType, float> baseStats = new Dictionary<StatType, float>();
    
    // 이벤트
    public event Action<StatType, float> OnRelicBonusChanged;
    
    void Start()
    {
        Initialize();
    }
    
    [Button("시스템 초기화", ButtonSizes.Large), GUIColor(0.4f, 0.4f, 0.8f)]
    public void Initialize()
    {
        if (playerStatus == null)
        {
            playerStatus = GetComponent<PlayerStatus>();
            if (playerStatus == null)
            {
                playerStatus = FindObjectOfType<PlayerController>()?.Status;
            }
        }
        
        if (relicInventory == null)
        {
            relicInventory = GetComponent<RelicInventorySystem>();
            if (relicInventory == null)
            {
                relicInventory = FindObjectOfType<RelicInventorySystem>();
            }
        }
        
        if (playerStatus == null || relicInventory == null)
        {
            Debug.LogError("필요한 시스템을 찾을 수 없습니다!");
            return;
        }
        
        SaveBaseStats();
        SubscribeToEvents();
        RecalculateAllBonuses();
        
        isInitialized = true;
        Debug.Log("RelicEffectSystem이 초기화되었습니다.");
    }
    
    private void SaveBaseStats()
    {
        baseStats.Clear();
        baseStats[StatType.MaxHp] = playerStatus.MaxHp;
        baseStats[StatType.AttackPower] = playerStatus.AttackPower;
        baseStats[StatType.CritChance] = playerStatus.CritChance;
        baseStats[StatType.CritDamage] = playerStatus.CritDamage;
        baseStats[StatType.AttackSpeed] = playerStatus.AttackSpeed;
        baseStats[StatType.HpRegen] = playerStatus.HpRegen;
    }
    
    private void SubscribeToEvents()
    {
        if (relicInventory != null)
        {
            relicInventory.OnRelicAdded += OnRelicAdded;
            relicInventory.OnRelicRemoved += OnRelicRemoved;
            relicInventory.OnFusionAttempt += OnFusionAttempt;
        }
    }
    
    private void OnDestroy()
    {
        if (relicInventory != null)
        {
            relicInventory.OnRelicAdded -= OnRelicAdded;
            relicInventory.OnRelicRemoved -= OnRelicRemoved;
            relicInventory.OnFusionAttempt -= OnFusionAttempt;
        }
    }
    
    private void OnRelicAdded(RelicInstance relic)
    {
        RecalculateAllBonuses();
    }
    
    private void OnRelicRemoved(RelicInstance relic)
    {
        RecalculateAllBonuses();
    }
    
    private void OnFusionAttempt(RelicInstance relic, bool success)
    {
        if (success)
        {
            RecalculateAllBonuses();
        }
    }
    
    [Button("유물 효과 재계산", ButtonSizes.Large), GUIColor(0.3f, 0.8f, 0.3f)]
    public void RecalculateAllBonuses()
    {
        if (!isInitialized) return;
        
        // 보너스 초기화
        currentRelicBonuses.Clear();
        foreach (StatType statType in Enum.GetValues(typeof(StatType)))
        {
            currentRelicBonuses[statType] = 0f;
        }
        
        // 모든 유물의 보너스 계산
        var allRelics = relicInventory.GetAllRelics();
        foreach (var relic in allRelics)
        {
            var bonuses = relic.GetAllStatBonuses();
            foreach (var bonus in bonuses)
            {
                currentRelicBonuses[bonus.Key] += bonus.Value;
            }
        }
        
        // 플레이어 스탯에 적용
        ApplyBonusesToPlayer();
        
        Debug.Log("유물 효과가 재계산되었습니다.");
    }
    
    private void ApplyBonusesToPlayer()
    {
        // 원본 값으로 초기화
        playerStatus.MaxHp = (int)baseStats[StatType.MaxHp];
        playerStatus.AttackPower = (int)baseStats[StatType.AttackPower];
        playerStatus.CritChance = baseStats[StatType.CritChance];
        playerStatus.CritDamage = baseStats[StatType.CritDamage];
        playerStatus.AttackSpeed = baseStats[StatType.AttackSpeed];
        playerStatus.HpRegen = baseStats[StatType.HpRegen];
        
        // 유물 보너스 적용
        foreach (var bonus in currentRelicBonuses)
        {
            if (bonus.Value <= 0) continue;
            
            bool isPercentage = false;
            
            switch (bonus.Key)
            {
                case StatType.MaxHp:
                    // 퍼센트로 적용
                    isPercentage = true;
                    playerStatus.MaxHp = (int)(playerStatus.MaxHp * (1 + bonus.Value / 100f));
                    break;
                    
                case StatType.AttackPower:
                    // 퍼센트로 적용
                    isPercentage = true;
                    playerStatus.AttackPower = (int)(playerStatus.AttackPower * (1 + bonus.Value / 100f));
                    break;
                    
                case StatType.CritChance:
                    // 퍼센트 포인트로 적용 (5% → 0.05 추가)
                    playerStatus.CritChance += bonus.Value / 100f;
                    break;
                    
                case StatType.CritDamage:
                    // 퍼센트 포인트로 적용 (10% → 0.1 추가)
                    playerStatus.CritDamage += bonus.Value / 100f;
                    break;
                    
                case StatType.AttackSpeed:
                    // 퍼센트로 적용
                    isPercentage = true;
                    playerStatus.AttackSpeed *= (1 + bonus.Value / 100f);
                    break;
                    
                case StatType.HpRegen:
                    // 고정값으로 적용
                    playerStatus.HpRegen += bonus.Value;
                    break;
            }
            
            OnRelicBonusChanged?.Invoke(bonus.Key, bonus.Value);
        }
    }
    
    [Title("유물 효과 요약")]
    [Button("효과 요약 출력", ButtonSizes.Large)]
    public void ShowRelicEffectSummary()
    {
        Debug.Log("========== 유물 효과 요약 ==========");
        
        foreach (var bonus in currentRelicBonuses)
        {
            if (bonus.Value > 0)
            {
                string statName = GetStatTypeName(bonus.Key);
                bool isPercentage = bonus.Key == StatType.MaxHp || 
                                  bonus.Key == StatType.AttackPower || 
                                  bonus.Key == StatType.AttackSpeed;
                
                string value = isPercentage ? $"+{bonus.Value:F1}%" : $"+{bonus.Value:F1}";
                
                Debug.Log($"<color=cyan>{statName}: {value}</color>");
            }
        }
        
        Debug.Log("\n--- 최종 스탯 ---");
        Debug.Log($"최대 체력: {baseStats[StatType.MaxHp]} → {playerStatus.MaxHp}");
        Debug.Log($"공격력: {baseStats[StatType.AttackPower]} → {playerStatus.AttackPower}");
        Debug.Log($"치명타 확률: {baseStats[StatType.CritChance]:P0} → {playerStatus.CritChance:P0}");
        Debug.Log($"치명타 데미지: {baseStats[StatType.CritDamage]:F1}x → {playerStatus.CritDamage:F1}x");
        Debug.Log($"공격 속도: {baseStats[StatType.AttackSpeed]:F2} → {playerStatus.AttackSpeed:F2}");
        Debug.Log($"체력 재생: {baseStats[StatType.HpRegen]:F1} → {playerStatus.HpRegen:F1}");
        
        Debug.Log("====================================");
    }
    
    private string GetStatTypeName(StatType statType)
    {
        switch (statType)
        {
            case StatType.MaxHp: return "최대 체력";
            case StatType.AttackPower: return "공격력";
            case StatType.CritChance: return "치명타 확률";
            case StatType.CritDamage: return "치명타 데미지";
            case StatType.AttackSpeed: return "공격 속도";
            case StatType.HpRegen: return "체력 재생";
            default: return statType.ToString();
        }
    }
    
    [Title("유물별 기여도")]
    [Button("유물별 기여도 분석", ButtonSizes.Medium)]
    private void AnalyzeRelicContributions()
    {
        Debug.Log("========== 유물별 기여도 ==========");
        
        var allRelics = relicInventory.GetAllRelics()
            .OrderByDescending(r => r.relicData.rarity)
            .ThenByDescending(r => r.level);
        
        foreach (var relic in allRelics.Take(10)) // 상위 10개만 표시
        {
            var color = ColorUtility.ToHtmlStringRGB(relic.relicData.GetRarityColor());
            Debug.Log($"\n<color=#{color}>[{relic.relicData.GetRarityName()}] {relic.relicData.relicName} Lv.{relic.level}</color>");
            
            var bonuses = relic.GetAllStatBonuses();
            foreach (var bonus in bonuses)
            {
                string statName = GetStatTypeName(bonus.Key);
                bool isPercentage = relic.relicData.statBonuses.First(b => b.statType == bonus.Key).isPercentage;
                string value = isPercentage ? $"+{bonus.Value:F1}%" : $"+{bonus.Value:F0}";
                Debug.Log($"  {statName}: {value}");
            }
        }
        
        Debug.Log("====================================");
    }
}