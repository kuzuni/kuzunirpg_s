// ���� �̱� �޼���� (public���� �ܺο��� ȣ�� ����)
using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;


using System.Linq;

public partial class EquipmentGachaSystem : MonoBehaviour
{
    [Title("�̱� ����")]
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
    [LabelText("�� Ȯ��")]
    private float TotalProbability => gachaRates.Sum(r => r.probability);

    [Title("õ�� �ý���")]
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
    [LabelText("����Ʈ õ�� ����")]
    private int softPityStart = 70;

    [FoldoutGroup("Pity System")]
    [SerializeField]
    [LabelText("����Ʈ õ�� Ȯ�� ����")]
    [SuffixLabel("%/ȸ", true)]
    private float softPityIncrement = 5f;

    [Title("�̱� ����")]
    [InfoBox("11ȸ: ��� �̻� 1�� ����\n55ȸ: ���� �̻� 1�� ����")]
    [SerializeField]
    private bool enable11PullGuarantee = true;
    [SerializeField]
    private bool enable55PullGuarantee = true;

    [Title("��� ������")]
    private Dictionary<EquipmentRarity, List<EquipmentData>> equipmentCache;

    [Title("�̱� ���")]
    [SerializeField]
    [ListDrawerSettings(ShowFoldout = false, NumberOfItemsPerPage = 10)]
    private List<string> recentPullHistory = new List<string>();

    [Title("���")]
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

        // Resources���� ��� ��� �ε�
        var allEquipments = Resources.LoadAll<EquipmentData>("Equipment");

        if (allEquipments == null || allEquipments.Length == 0)
        {
            Debug.LogError("Resources/Equipment �������� ��� ã�� �� �����ϴ�!");
            return;
        }

        foreach (var equipment in allEquipments)
        {
            if (equipment != null && equipmentCache.ContainsKey(equipment.rarity))
            {
                equipmentCache[equipment.rarity].Add(equipment);
            }
        }

        // �ε� ��� ���
        Debug.Log("========== ��� �ε� �Ϸ� ==========");
        foreach (var kvp in equipmentCache)
        {
            Debug.Log($"{kvp.Key}: {kvp.Value.Count}��");
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

    [Title("�̱� ���")]
    [ButtonGroup("Gacha")]
    [Button("1ȸ �̱�", ButtonSizes.Large), GUIColor(0.3f, 0.8f, 0.3f)]
    [PropertyOrder(1)]
    private void Pull1Button()
    {
        if (!Application.isPlaying)
        {
            Debug.LogWarning("Play Mode������ �̱Ⱑ �����մϴ�!");
            return;
        }
        Pull1();
    }

    [ButtonGroup("Gacha")]
    [Button("11ȸ �̱�", ButtonSizes.Large), GUIColor(0.5f, 0.5f, 1f)]
    [PropertyOrder(2)]
    private void Pull11Button()
    {
        if (!Application.isPlaying)
        {
            Debug.LogWarning("Play Mode������ �̱Ⱑ �����մϴ�!");
            return;
        }
        Pull11();
    }

    [ButtonGroup("Gacha")]
    [Button("55ȸ �̱�", ButtonSizes.Large), GUIColor(1f, 0.8f, 0.3f)]
    [PropertyOrder(3)]
    private void Pull55Button()
    {
        if (!Application.isPlaying)
        {
            Debug.LogWarning("Play Mode������ �̱Ⱑ �����մϴ�!");
            return;
        }
        Pull55();
    }

    private EquipmentData PerformSinglePull()
    {
        currentPityCount++;

        // õ�� ����
        if (currentPityCount >= pityCount)
        {
            currentPityCount = 0;
            return PerformGuaranteedPull(guaranteedRarity);
        }

        // ��� ���� (����Ʈ õ�� ����)
        EquipmentRarity selectedRarity = DetermineRarity();

        // ���� ����̸� õ�� ����
        if (selectedRarity >= guaranteedRarity)
        {
            currentPityCount = 0;
        }

        // ��� ����
        var equipment = GetRandomEquipment(selectedRarity);
        if (equipment != null)
        {
            // ��� ������Ʈ - null üũ �߰�
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
            // ��� ������Ʈ - null üũ �߰�
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
            Debug.Log($"<color=yellow>�ں���� {equipment.GetFullRarityName()} {equipment.equipmentName} ȹ��!</color>");
        }

        return equipment;
    }

    private EquipmentRarity DetermineRarity()
    {
        float bonusChance = 0f;

        // ����Ʈ õ�� ���
        if (currentPityCount >= softPityStart)
        {
            int overSoftPity = currentPityCount - softPityStart;
            bonusChance = overSoftPity * softPityIncrement;
        }

        float random = Random.Range(0f, 100f);
        float cumulative = 0f;

        // ���� ��޺��� Ȯ�� (����Ʈ õ�� ���ʽ� ����)
        for (int i = gachaRates.Count - 1; i >= 0; i--)
        {
            var rate = gachaRates[i];
            float actualRate = rate.probability;

            // ���� ��� �̻󿡸� ���ʽ� ����
            if (rate.rarity >= guaranteedRarity)
            {
                actualRate += bonusChance;
            }

            if (random < actualRate)
            {
                return rate.rarity;
            }
        }

        // �Ϲ� Ȯ�� ���
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
            Debug.LogError("Equipment cache is null! ��ε尡 �ʿ��մϴ�.");
            LoadEquipmentData();
        }

        if (!equipmentCache.ContainsKey(rarity) || equipmentCache[rarity] == null || equipmentCache[rarity].Count == 0)
        {
            Debug.LogError($"{rarity} ����� ��� �����ϴ�! �ٸ� ������� ��ü�մϴ�.");

            // ��ü ��� ã�� (���� ��޺���)
            foreach (EquipmentRarity fallbackRarity in System.Enum.GetValues(typeof(EquipmentRarity)))
            {
                if (equipmentCache.ContainsKey(fallbackRarity) && equipmentCache[fallbackRarity].Count > 0)
                {
                    Debug.LogWarning($"{rarity} ��� {fallbackRarity} ��� ���� ��ü�մϴ�.");
                    rarity = fallbackRarity;
                    break;
                }
            }

            // �׷��� ������ ����
            if (equipmentCache[rarity].Count == 0)
            {
                Debug.LogError("��� ������ ��� �ϳ��� �����ϴ�!");
                return null;
            }
        }

        var equipmentList = equipmentCache[rarity];
        var original = equipmentList[Random.Range(0, equipmentList.Count)];

        // ���纻 ����
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
        Debug.Log($"<color=#{color}>�� ȹ��: {equipment.GetFullRarityName()} - {equipment.equipmentName} ��</color>");
    }

    private void ShowMultipleResults(List<EquipmentData> results, string title)
    {
        Debug.Log($"========== {title} ��� ==========");

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
        Debug.Log("========== �̱� ��� ==========");

        var stats = results.GroupBy(e => e.rarity)
                          .OrderByDescending(g => g.Key)
                          .Select(g => new { Rarity = g.Key, Count = g.Count() });

        foreach (var stat in stats)
        {
            var color = ColorUtility.ToHtmlStringRGB(RarityColors.GetRarityColor(stat.Rarity));
            Debug.Log($"<color=#{color}>{RarityColors.GetRarityName(stat.Rarity)}: {stat.Count}��</color>");
        }

        Debug.Log("================================");
    }

    [Title("�����")]
    [Button("��� ������ ��ε�", ButtonSizes.Large)]
    [GUIColor(0.3f, 0.8f, 0.8f)]
    private void ReloadEquipmentData()
    {
        InitializeCache();
        LoadEquipmentData();
        Debug.Log("��� �����͸� �ٽ� �ε��߽��ϴ�.");
    }

    [Button("õ�� ī��Ʈ ����", ButtonSizes.Medium)]
    private void ResetPity()
    {
        currentPityCount = 0;
        Debug.Log("õ�� ī��Ʈ�� ���µǾ����ϴ�.");
    }

    [Button("�̱� Ȯ�� ����", ButtonSizes.Medium)]
    private void ShowRateInfo()
    {
        Debug.Log("========== �̱� Ȯ�� ���� ==========");
        foreach (var rate in gachaRates)
        {
            var color = ColorUtility.ToHtmlStringRGB(RarityColors.GetRarityColor(rate.rarity));
            Debug.Log($"<color=#{color}>{RarityColors.GetRarityName(rate.rarity)}: {rate.probability}%</color>");
        }
        Debug.Log($"\nõ��: {pityCount}ȸ ({RarityColors.GetRarityName(guaranteedRarity)} �̻� Ȯ��)");
        Debug.Log($"����Ʈ õ��: {softPityStart}ȸ���� ���� (ȸ�� +{softPityIncrement}%)");
        Debug.Log("\n11ȸ �̱�: ��� �̻� 1�� ����");
        Debug.Log("55ȸ �̱�: ���� �̻� 1�� ����");
        Debug.Log("====================================");
    }

    [Button("��ü ��� �ʱ�ȭ", ButtonSizes.Medium)]
    [GUIColor(0.8f, 0.3f, 0.3f)]
    private void ResetStatistics()
    {
        InitializeStatistics();
        recentPullHistory.Clear();
        Debug.Log("��谡 �ʱ�ȭ�Ǿ����ϴ�.");
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

        // 10ȸ �Ϲ� �̱�
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

        // 11��° �̱� (��� �̻� ����)
        if (enable11PullGuarantee && !hasRareOrBetter)
        {
            var guaranteed = PerformGuaranteedPull(EquipmentRarity.Rare);
            results.Add(guaranteed);
        }
        else
        {
            results.Add(PerformSinglePull());
        }

        ShowMultipleResults(results, "11ȸ �̱�");
        return results;
    }

    public List<EquipmentData> Pull55()
    {
        List<EquipmentData> results = new List<EquipmentData>();
        bool hasEpicOrBetter = false;

        // 54ȸ �̱�
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

        // 55��° �̱� (���� �̻� ����)
        if (enable55PullGuarantee && !hasEpicOrBetter)
        {
            var guaranteed = PerformGuaranteedPull(EquipmentRarity.Epic);
            results.Add(guaranteed);
        }
        else
        {
            results.Add(PerformSinglePull());
        }

        ShowMultipleResults(results, "55ȸ �̱�");
        ShowPullStatistics(results);
        return results;

    }
}

