using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class EquipmentGachaSystem : BaseGachaSystem<EquipmentData, EquipmentRarity>
{
    [Title("장비 뽑기 특수 설정")]
    [FoldoutGroup("Pity System")]
    [SerializeField]
    [LabelText("소프트 천장 시작")]
    private int softPityStart = 70;

    [FoldoutGroup("Pity System")]
    [SerializeField]
    [LabelText("소프트 천장 확률 증가")]
    [SuffixLabel("%/회", true)]
    private float softPityIncrement = 5f;

    [Title("뽑기 보장")]
    [InfoBox("11회: 희귀 이상 1개 보장\n55회: 영웅 이상 1개 보장")]
    [SerializeField]
    private bool enable11PullGuarantee = true;
    [SerializeField]
    private bool enable55PullGuarantee = true;

    [Title("통계")]
    [SerializeField]
    private Dictionary<EquipmentRarity, int> pullStatistics = new Dictionary<EquipmentRarity, int>();

    [Title("인벤토리 연동")]
    [SerializeField]
    private EquipmentInventorySystem inventorySystem;

    [SerializeField]
    private bool autoAddToInventory = true;

    protected override void Start()
    {
        base.Start();
        InitializeStatistics();

        // 인벤토리 시스템 찾기
        if (inventorySystem == null)
        {
            inventorySystem = GetComponent<EquipmentInventorySystem>();
            if (inventorySystem == null)
            {
                inventorySystem = FindObjectOfType<EquipmentInventorySystem>();
            }
        }
    }

    protected override void InitializeCache()
    {
        itemCache = new Dictionary<EquipmentRarity, List<EquipmentData>>();
        foreach (EquipmentRarity rarity in System.Enum.GetValues(typeof(EquipmentRarity)))
        {
            itemCache[rarity] = new List<EquipmentData>();
        }
    }

    protected override void LoadItemData()
    {
        var allEquipments = Resources.LoadAll<EquipmentData>("Equipment");

        if (allEquipments == null || allEquipments.Length == 0)
        {
            Debug.LogError("Resources/Equipment 폴더에서 장비를 찾을 수 없습니다!");
            return;
        }

        foreach (var equipment in allEquipments)
        {
            if (equipment != null && itemCache.ContainsKey(equipment.rarity))
            {
                itemCache[equipment.rarity].Add(equipment);
            }
        }

        // 로드 결과 출력
        Debug.Log("========== 장비 로드 완료 ==========");
        foreach (var kvp in itemCache)
        {
            Debug.Log($"{kvp.Key}: {kvp.Value.Count}개");
        }
        Debug.Log("====================================");
    }

    protected override EquipmentData GetRandomItem(EquipmentRarity rarity)
    {
        if (!itemCache.ContainsKey(rarity) || itemCache[rarity].Count == 0)
        {
            Debug.LogError($"{rarity} 등급의 장비가 없습니다!");

            // 대체 등급 찾기
            foreach (EquipmentRarity fallbackRarity in System.Enum.GetValues(typeof(EquipmentRarity)))
            {
                if (itemCache.ContainsKey(fallbackRarity) && itemCache[fallbackRarity].Count > 0)
                {
                    Debug.LogWarning($"{rarity} 대신 {fallbackRarity} 등급 장비로 대체합니다.");
                    rarity = fallbackRarity;
                    break;
                }
            }
        }

        var equipmentList = itemCache[rarity];
        var original = equipmentList[Random.Range(0, equipmentList.Count)];

        // 복사본 생성
        var copy = Instantiate(original);
        copy.name = original.name;

        return copy;
    }

    protected override EquipmentRarity DetermineRarity()
    {
        float bonusChance = 0f;

        // 소프트 천장 계산
        if (currentPityCount >= softPityStart)
        {
            int overSoftPity = currentPityCount - softPityStart;
            bonusChance = overSoftPity * softPityIncrement;
        }

        float random = Random.Range(0f, 100f);
        float cumulative = 0f;

        // 높은 등급부터 확인 (소프트 천장 보너스 적용)
        for (int i = gachaRates.Count - 1; i >= 0; i--)
        {
            var rate = gachaRates[i];
            float actualRate = rate.probability;

            // 보장 등급 이상에만 보너스 적용
            if (IsRarityGreaterOrEqual(rate.rarity, guaranteedRarity))
            {
                actualRate += bonusChance;
            }

            if (random < actualRate)
            {
                return rate.rarity;
            }
        }

        // 일반 확률 계산
        foreach (var rate in gachaRates)
        {
            cumulative += rate.probability;
            if (random <= cumulative)
            {
                return rate.rarity;
            }
        }

        return EquipmentRarity.Common;
    }

    protected override EquipmentRarity GetRandomRarityWithMinimum(EquipmentRarity minimum)
    {
        var validRates = gachaRates.Where(r => IsRarityGreaterOrEqual(r.rarity, minimum)).ToList();
        float totalProb = validRates.Sum(r => r.probability);
        float random = Random.Range(0f, totalProb);
        float cumulative = 0f;

        foreach (var rate in validRates)
        {
            cumulative += rate.probability;
            if (random <= cumulative)
            {
                return rate.rarity;
            }
        }

        return minimum;
    }

    protected override bool IsRarityGreaterOrEqual(EquipmentRarity rarity1, EquipmentRarity rarity2)
    {
        return rarity1 >= rarity2;
    }

    // 오버라이드된 메서드들
    public override EquipmentData PullSingle()
    {
        var result = base.PullSingle();

        if (result != null)
        {
            UpdateStatistics(result);
            ShowSingleResult(result);
        }

        return result;
    }

    public override List<EquipmentData> Pull11()
    {
        List<EquipmentData> results = new List<EquipmentData>();
        bool hasRareOrBetter = false;

        // 10회 일반 뽑기
        for (int i = 0; i < 10; i++)
        {
            var equipment = base.PullSingle();
            if (equipment != null)
            {
                results.Add(equipment);
                UpdateStatistics(equipment);
                if (equipment.rarity >= EquipmentRarity.Rare)
                {
                    hasRareOrBetter = true;
                }
            }
        }

        // 11번째 뽑기 (희귀 이상 보장)
        if (enable11PullGuarantee && !hasRareOrBetter)
        {
            var guaranteed = PerformGuaranteedPull(EquipmentRarity.Rare);
            results.Add(guaranteed);
            UpdateStatistics(guaranteed);
        }
        else
        {
            var lastPull = base.PullSingle();
            results.Add(lastPull);
            UpdateStatistics(lastPull);
        }

        ShowMultipleResults(results, "11회 뽑기");
        return results;
    }

    // 55회 뽑기
    public List<EquipmentData> Pull55()
    {
        List<EquipmentData> results = new List<EquipmentData>();
        bool hasEpicOrBetter = false;

        // 54회 뽑기
        for (int i = 0; i < 54; i++)
        {
            var equipment = base.PullSingle();
            if (equipment != null)
            {
                results.Add(equipment);
                UpdateStatistics(equipment);
                if (equipment.rarity >= EquipmentRarity.Epic)
                {
                    hasEpicOrBetter = true;
                }
            }
        }

        // 55번째 뽑기 (영웅 이상 보장)
        if (enable55PullGuarantee && !hasEpicOrBetter)
        {
            var guaranteed = PerformGuaranteedPull(EquipmentRarity.Epic);
            results.Add(guaranteed);
            UpdateStatistics(guaranteed);
        }
        else
        {
            var lastPull = base.PullSingle();
            results.Add(lastPull);
            UpdateStatistics(lastPull);
        }

        ShowMultipleResults(results, "55회 뽑기");
        ShowPullStatistics(results);
        return results;
    }

    // 타입별 뽑기 메서드
    public EquipmentData PullSingleByType(EquipmentType targetType)
    {
        var result = PerformTypedSinglePull(targetType);
        if (result != null)
        {
            UpdateStatistics(result);
            ShowSingleResult(result);
        }
        return result;
    }

    private EquipmentData PerformTypedSinglePull(EquipmentType targetType)
    {
        currentPityCount++;

        if (currentPityCount >= pityCount)
        {
            currentPityCount = 0;
            return GetRandomTypedItem(guaranteedRarity, targetType);
        }

        EquipmentRarity selectedRarity = DetermineRarity();

        if (IsRarityGreaterOrEqual(selectedRarity, guaranteedRarity))
        {
            currentPityCount = 0;
        }

        return GetRandomTypedItem(selectedRarity, targetType);
    }

    private EquipmentData GetRandomTypedItem(EquipmentRarity rarity, EquipmentType targetType)
    {
        if (!itemCache.ContainsKey(rarity) || itemCache[rarity].Count == 0)
        {
            Debug.LogError($"{rarity} 등급의 장비가 없습니다!");
            return null;
        }

        var typedEquipments = itemCache[rarity].Where(e => e.equipmentType == targetType).ToList();

        if (typedEquipments.Count == 0)
        {
            // 대체 등급 찾기
            foreach (EquipmentRarity fallbackRarity in System.Enum.GetValues(typeof(EquipmentRarity)))
            {
                if (itemCache.ContainsKey(fallbackRarity))
                {
                    var fallbackTyped = itemCache[fallbackRarity].Where(e => e.equipmentType == targetType).ToList();
                    if (fallbackTyped.Count > 0)
                    {
                        Debug.LogWarning($"{rarity} {targetType} 대신 {fallbackRarity} {targetType}로 대체합니다.");
                        typedEquipments = fallbackTyped;
                        break;
                    }
                }
            }
        }

        if (typedEquipments.Count == 0)
        {
            Debug.LogError($"사용 가능한 {targetType} 타입 장비가 하나도 없습니다!");
            return null;
        }

        var original = typedEquipments[Random.Range(0, typedEquipments.Count)];
        var copy = Instantiate(original);
        copy.name = original.name;

        return copy;
    }

    // 통계 업데이트
    private void UpdateStatistics(EquipmentData equipment)
    {
        if (equipment == null) return;

        if (!pullStatistics.ContainsKey(equipment.rarity))
        {
            pullStatistics[equipment.rarity] = 0;
        }
        pullStatistics[equipment.rarity]++;
    }

    private void InitializeStatistics()
    {
        foreach (EquipmentRarity rarity in System.Enum.GetValues(typeof(EquipmentRarity)))
        {
            pullStatistics[rarity] = 0;
        }
    }

    // 결과 표시
    private void ShowSingleResult(EquipmentData equipment)
    {
        var color = ColorUtility.ToHtmlStringRGB(equipment.GetRarityColor());
        Debug.Log($"<color=#{color}>★ 획득: {equipment.GetFullRarityName()} - {equipment.equipmentName} ★</color>");

        // 인벤토리에 자동 추가
        if (autoAddToInventory && inventorySystem != null)
        {
            inventorySystem.AddItem(equipment);
        }
    }

    protected override void ShowMultipleResults(List<EquipmentData> results, string title)
    {
        base.ShowMultipleResults(results, title);

        // 인벤토리에 자동 추가
        if (autoAddToInventory && inventorySystem != null)
        {
            int addedCount = inventorySystem.AddItems(results);
            if (addedCount < results.Count)
            {
                Debug.LogWarning($"인벤토리 공간 부족! {results.Count}개 중 {addedCount}개만 추가됨");
            }
        }
    }

    private void ShowPullStatistics(List<EquipmentData> results)
    {
        Debug.Log("========== 뽑기 통계 ==========");

        var stats = results.GroupBy(e => e.rarity)
                          .OrderByDescending(g => g.Key)
                          .Select(g => new { Rarity = g.Key, Count = g.Count() });

        foreach (var stat in stats)
        {
            var color = ColorUtility.ToHtmlStringRGB(RarityColors.GetRarityColor(stat.Rarity));
            Debug.Log($"<color=#{color}>{RarityColors.GetRarityName(stat.Rarity)}: {stat.Count}개</color>");
        }

        Debug.Log("================================");
    }

    [Title("뽑기 기능")]
    [ButtonGroup("Gacha")]
    [Button("1회 뽑기", ButtonSizes.Large), GUIColor(0.3f, 0.8f, 0.3f)]
    private void Pull1Button()
    {
        if (!Application.isPlaying)
        {
            Debug.LogWarning("Play Mode에서만 뽑기가 가능합니다!");
            return;
        }
        PullSingle();
    }

    [ButtonGroup("Gacha")]
    [Button("11회 뽑기", ButtonSizes.Large), GUIColor(0.5f, 0.5f, 1f)]
    private void Pull11Button()
    {
        if (!Application.isPlaying)
        {
            Debug.LogWarning("Play Mode에서만 뽑기가 가능합니다!");
            return;
        }
        Pull11();
    }

    [ButtonGroup("Gacha")]
    [Button("55회 뽑기", ButtonSizes.Large), GUIColor(1f, 0.8f, 0.3f)]
    private void Pull55Button()
    {
        if (!Application.isPlaying)
        {
            Debug.LogWarning("Play Mode에서만 뽑기가 가능합니다!");
            return;
        }
        Pull55();
    }

    [Title("디버그")]
    [Button("장비 데이터 재로드", ButtonSizes.Large)]
    [GUIColor(0.3f, 0.8f, 0.8f)]
    private void ReloadEquipmentData()
    {
        InitializeCache();
        LoadItemData();
        Debug.Log("장비 데이터를 다시 로드했습니다.");
    }

    [Button("전체 통계 초기화", ButtonSizes.Medium)]
    [GUIColor(0.8f, 0.3f, 0.3f)]
    private void ResetStatistics()
    {
        InitializeStatistics();
        recentPullHistory.Clear();
        Debug.Log("통계가 초기화되었습니다.");
    }

    protected override void ShowRateInfo()
    {
        base.ShowRateInfo();
        Debug.Log($"소프트 천장: {softPityStart}회부터 시작 (회당 +{softPityIncrement}%)");
        Debug.Log("\n11회 뽑기: 희귀 이상 1개 보장");
        Debug.Log("55회 뽑기: 영웅 이상 1개 보장");
    }
}