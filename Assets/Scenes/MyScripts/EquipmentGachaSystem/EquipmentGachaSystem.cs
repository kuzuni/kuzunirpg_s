// 실제 뽑기 메서드들 (public으로 외부에서 호출 가능)
using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;


using System.Linq;

public partial class EquipmentGachaSystem : MonoBehaviour
{
    [Title("뽑기 설정")]
    [SerializeField]
    [TableList(ShowIndexLabels = true, DrawScrollView = true)]
    private List<GachaRateTable> gachaRates = new List<GachaRateTable>
    {
        new GachaRateTable { rarity = EquipmentRarity.Common, probability = 45f },
        new GachaRateTable { rarity = EquipmentRarity.Uncommon, probability = 30f },
        new GachaRateTable { rarity = EquipmentRarity.Rare, probability = 17f },
        new GachaRateTable { rarity = EquipmentRarity.Epic, probability = 6f },
        new GachaRateTable { rarity = EquipmentRarity.Legendary, probability = 1.5f },
        new GachaRateTable { rarity = EquipmentRarity.Mythic, probability = 0.45f },
        new GachaRateTable { rarity = EquipmentRarity.Celestial, probability = 0.05f }
    };

    [ShowInInspector, ReadOnly]
    [ProgressBar(0, 100, 0.3f, 0.8f, 0.3f)]
    [LabelText("총 확률")]
    private float TotalProbability => gachaRates.Sum(r => r.probability);

    [Title("천장 시스템")]
    [FoldoutGroup("Pity System")]
    [SerializeField]
    private int pityCount = 90;

    [FoldoutGroup("Pity System")]
    [SerializeField]
    [ProgressBar(0, "@pityCount", 0.8f, 0.8f, 0.3f)]
    private int currentPityCount = 0;

    [FoldoutGroup("Pity System")]
    [SerializeField]
    private EquipmentRarity guaranteedRarity = EquipmentRarity.Epic;

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

    [Title("장비 데이터")]
    private Dictionary<EquipmentRarity, List<EquipmentData>> equipmentCache;

    [Title("뽑기 기록")]
    [SerializeField]
    [ListDrawerSettings(ShowFoldout = false, NumberOfItemsPerPage = 10)]
    private List<string> recentPullHistory = new List<string>();

    [Title("통계")]
    [SerializeField]
    private Dictionary<EquipmentRarity, int> pullStatistics = new Dictionary<EquipmentRarity, int>();

    void Start()
    {
        if (equipmentCache == null || equipmentCache.Count == 0)
        {
            InitializeCache();
            LoadEquipmentData();
        }

        if (pullStatistics == null || pullStatistics.Count == 0)
        {
            InitializeStatistics();
        }

        if (recentPullHistory == null)
        {
            recentPullHistory = new List<string>();
        }
    }

    private void InitializeCache()
    {
        equipmentCache = new Dictionary<EquipmentRarity, List<EquipmentData>>();

        foreach (EquipmentRarity rarity in System.Enum.GetValues(typeof(EquipmentRarity)))
        {
            equipmentCache[rarity] = new List<EquipmentData>();
        }
    }

    private void LoadEquipmentData()
    {
        if (equipmentCache == null)
        {
            InitializeCache();
        }

        // Resources에서 모든 장비 로드
        var allEquipments = Resources.LoadAll<EquipmentData>("Equipment");

        if (allEquipments == null || allEquipments.Length == 0)
        {
            Debug.LogError("Resources/Equipment 폴더에서 장비를 찾을 수 없습니다!");
            return;
        }

        foreach (var equipment in allEquipments)
        {
            if (equipment != null && equipmentCache.ContainsKey(equipment.rarity))
            {
                equipmentCache[equipment.rarity].Add(equipment);
            }
        }

        // 로드 결과 출력
        Debug.Log("========== 장비 로드 완료 ==========");
        foreach (var kvp in equipmentCache)
        {
            Debug.Log($"{kvp.Key}: {kvp.Value.Count}개");
        }
        Debug.Log("====================================");
    }

    private void InitializeStatistics()
    {
        foreach (EquipmentRarity rarity in System.Enum.GetValues(typeof(EquipmentRarity)))
        {
            pullStatistics[rarity] = 0;
        }
    }

    [Title("뽑기 기능")]
    [ButtonGroup("Gacha")]
    [Button("1회 뽑기", ButtonSizes.Large), GUIColor(0.3f, 0.8f, 0.3f)]
    [PropertyOrder(1)]
    private void Pull1Button()
    {
        if (!Application.isPlaying)
        {
            Debug.LogWarning("Play Mode에서만 뽑기가 가능합니다!");
            return;
        }
        Pull1();
    }

    [ButtonGroup("Gacha")]
    [Button("11회 뽑기", ButtonSizes.Large), GUIColor(0.5f, 0.5f, 1f)]
    [PropertyOrder(2)]
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
    [PropertyOrder(3)]
    private void Pull55Button()
    {
        if (!Application.isPlaying)
        {
            Debug.LogWarning("Play Mode에서만 뽑기가 가능합니다!");
            return;
        }
        Pull55();
    }

    private EquipmentData PerformSinglePull()
    {
        currentPityCount++;

        // 천장 도달
        if (currentPityCount >= pityCount)
        {
            currentPityCount = 0;
            return PerformGuaranteedPull(guaranteedRarity);
        }

        // 등급 결정 (소프트 천장 적용)
        EquipmentRarity selectedRarity = DetermineRarity();

        // 높은 등급이면 천장 리셋
        if (selectedRarity >= guaranteedRarity)
        {
            currentPityCount = 0;
        }

        // 장비 선택
        var equipment = GetRandomEquipment(selectedRarity);
        if (equipment != null)
        {
            // 통계 업데이트 - null 체크 추가
            if (pullStatistics == null)
            {
                InitializeStatistics();
            }

            if (pullStatistics.ContainsKey(equipment.rarity))
            {
                pullStatistics[equipment.rarity]++;
            }
            else
            {
                pullStatistics[equipment.rarity] = 1;
            }

            AddToHistory(equipment);
        }

        return equipment;
    }

    private EquipmentData PerformGuaranteedPull(EquipmentRarity minimumRarity)
    {
        var rarity = GetRandomRarityWithMinimum(minimumRarity);
        var equipment = GetRandomEquipment(rarity);

        if (equipment != null)
        {
            // 통계 업데이트 - null 체크 추가
            if (pullStatistics == null)
            {
                InitializeStatistics();
            }

            if (pullStatistics.ContainsKey(equipment.rarity))
            {
                pullStatistics[equipment.rarity]++;
            }
            else
            {
                pullStatistics[equipment.rarity] = 1;
            }

            AddToHistory(equipment);
            Debug.Log($"<color=yellow>★보장★ {equipment.GetFullRarityName()} {equipment.equipmentName} 획득!</color>");
        }

        return equipment;
    }

    private EquipmentRarity DetermineRarity()
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
            if (rate.rarity >= guaranteedRarity)
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

    private EquipmentRarity GetRandomRarityWithMinimum(EquipmentRarity minimum)
    {
        var validRates = gachaRates.Where(r => r.rarity >= minimum).ToList();
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

    private EquipmentData GetRandomEquipment(EquipmentRarity rarity)
    {
        if (equipmentCache == null)
        {
            Debug.LogError("Equipment cache is null! 재로드가 필요합니다.");
            LoadEquipmentData();
        }

        if (!equipmentCache.ContainsKey(rarity) || equipmentCache[rarity] == null || equipmentCache[rarity].Count == 0)
        {
            Debug.LogError($"{rarity} 등급의 장비가 없습니다! 다른 등급으로 대체합니다.");

            // 대체 등급 찾기 (낮은 등급부터)
            foreach (EquipmentRarity fallbackRarity in System.Enum.GetValues(typeof(EquipmentRarity)))
            {
                if (equipmentCache.ContainsKey(fallbackRarity) && equipmentCache[fallbackRarity].Count > 0)
                {
                    Debug.LogWarning($"{rarity} 대신 {fallbackRarity} 등급 장비로 대체합니다.");
                    rarity = fallbackRarity;
                    break;
                }
            }

            // 그래도 없으면 에러
            if (equipmentCache[rarity].Count == 0)
            {
                Debug.LogError("사용 가능한 장비가 하나도 없습니다!");
                return null;
            }
        }

        var equipmentList = equipmentCache[rarity];
        var original = equipmentList[Random.Range(0, equipmentList.Count)];

        // 복사본 생성
        var copy = Instantiate(original);
        copy.name = original.name;

        return copy;
    }

    private void AddToHistory(EquipmentData equipment)
    {
        if (equipment == null) return;

        var color = ColorUtility.ToHtmlStringRGB(RarityColors.GetRarityColor(equipment.rarity));
        string log = $"<color=#{color}>{equipment.GetFullRarityName()} - {equipment.equipmentName}</color>";

        if (recentPullHistory == null)
        {
            recentPullHistory = new List<string>();
        }

        recentPullHistory.Insert(0, log);
        if (recentPullHistory.Count > 50)
        {
            recentPullHistory.RemoveAt(recentPullHistory.Count - 1);
        }
    }

    private void ShowSingleResult(EquipmentData equipment)
    {
        var color = ColorUtility.ToHtmlStringRGB(RarityColors.GetRarityColor(equipment.rarity));
        Debug.Log($"<color=#{color}>★ 획득: {equipment.GetFullRarityName()} - {equipment.equipmentName} ★</color>");
    }

    private void ShowMultipleResults(List<EquipmentData> results, string title)
    {
        Debug.Log($"========== {title} 결과 ==========");

        var sortedResults = results.OrderByDescending(e => e.rarity).ThenByDescending(e => e.subGrade);

        foreach (var equipment in sortedResults)
        {
            var color = ColorUtility.ToHtmlStringRGB(RarityColors.GetRarityColor(equipment.rarity));
            Debug.Log($"<color=#{color}>{equipment.GetFullRarityName()} - {equipment.equipmentName}</color>");
        }

        Debug.Log("================================");
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

    [Title("디버그")]
    [Button("장비 데이터 재로드", ButtonSizes.Large)]
    [GUIColor(0.3f, 0.8f, 0.8f)]
    private void ReloadEquipmentData()
    {
        InitializeCache();
        LoadEquipmentData();
        Debug.Log("장비 데이터를 다시 로드했습니다.");
    }

    [Button("천장 카운트 리셋", ButtonSizes.Medium)]
    private void ResetPity()
    {
        currentPityCount = 0;
        Debug.Log("천장 카운트가 리셋되었습니다.");
    }

    [Button("뽑기 확률 정보", ButtonSizes.Medium)]
    private void ShowRateInfo()
    {
        Debug.Log("========== 뽑기 확률 정보 ==========");
        foreach (var rate in gachaRates)
        {
            var color = ColorUtility.ToHtmlStringRGB(RarityColors.GetRarityColor(rate.rarity));
            Debug.Log($"<color=#{color}>{RarityColors.GetRarityName(rate.rarity)}: {rate.probability}%</color>");
        }
        Debug.Log($"\n천장: {pityCount}회 ({RarityColors.GetRarityName(guaranteedRarity)} 이상 확정)");
        Debug.Log($"소프트 천장: {softPityStart}회부터 시작 (회당 +{softPityIncrement}%)");
        Debug.Log("\n11회 뽑기: 희귀 이상 1개 보장");
        Debug.Log("55회 뽑기: 영웅 이상 1개 보장");
        Debug.Log("====================================");
    }

    [Button("전체 통계 초기화", ButtonSizes.Medium)]
    [GUIColor(0.8f, 0.3f, 0.3f)]
    private void ResetStatistics()
    {
        InitializeStatistics();
        recentPullHistory.Clear();
        Debug.Log("통계가 초기화되었습니다.");
    }


    public EquipmentData Pull1()
    {
        var result = PerformSinglePull();

        if (result != null)
        {
            ShowSingleResult(result);
        }

        return result;
    }

    public List<EquipmentData> Pull11()
    {
        List<EquipmentData> results = new List<EquipmentData>();
        bool hasRareOrBetter = false;

        // 10회 일반 뽑기
        for (int i = 0; i < 10; i++)
        {
            var equipment = PerformSinglePull();
            if (equipment != null)
            {
                results.Add(equipment);
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
        }
        else
        {
            results.Add(PerformSinglePull());
        }

        ShowMultipleResults(results, "11회 뽑기");
        return results;
    }

    public List<EquipmentData> Pull55()
    {
        List<EquipmentData> results = new List<EquipmentData>();
        bool hasEpicOrBetter = false;

        // 54회 뽑기
        for (int i = 0; i < 54; i++)
        {
            var equipment = PerformSinglePull();
            if (equipment != null)
            {
                results.Add(equipment);
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
        }
        else
        {
            results.Add(PerformSinglePull());
        }

        ShowMultipleResults(results, "55회 뽑기");
        ShowPullStatistics(results);
        return results;

    }
}

