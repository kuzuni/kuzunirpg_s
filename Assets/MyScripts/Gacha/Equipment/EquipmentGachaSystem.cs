using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class EquipmentGachaSystem : BaseGachaSystem<EquipmentData, EquipmentRarity>
{
    [Title("��� �̱� Ư�� ����")]
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

    [Title("���")]
    [SerializeField]
    private Dictionary<EquipmentRarity, int> pullStatistics = new Dictionary<EquipmentRarity, int>();

    [Title("�κ��丮 ����")]
    [SerializeField]
    private EquipmentInventorySystem inventorySystem;

    [SerializeField]
    private bool autoAddToInventory = true;

    protected override void Start()
    {
        base.Start();
        InitializeStatistics();

        // �κ��丮 �ý��� ã��
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
            Debug.LogError("Resources/Equipment �������� ��� ã�� �� �����ϴ�!");
            return;
        }

        foreach (var equipment in allEquipments)
        {
            if (equipment != null && itemCache.ContainsKey(equipment.rarity))
            {
                itemCache[equipment.rarity].Add(equipment);
            }
        }

        // �ε� ��� ���
        Debug.Log("========== ��� �ε� �Ϸ� ==========");
        foreach (var kvp in itemCache)
        {
            Debug.Log($"{kvp.Key}: {kvp.Value.Count}��");
        }
        Debug.Log("====================================");
    }

    protected override EquipmentData GetRandomItem(EquipmentRarity rarity)
    {
        if (!itemCache.ContainsKey(rarity) || itemCache[rarity].Count == 0)
        {
            Debug.LogError($"{rarity} ����� ��� �����ϴ�!");

            // ��ü ��� ã��
            foreach (EquipmentRarity fallbackRarity in System.Enum.GetValues(typeof(EquipmentRarity)))
            {
                if (itemCache.ContainsKey(fallbackRarity) && itemCache[fallbackRarity].Count > 0)
                {
                    Debug.LogWarning($"{rarity} ��� {fallbackRarity} ��� ���� ��ü�մϴ�.");
                    rarity = fallbackRarity;
                    break;
                }
            }
        }

        var equipmentList = itemCache[rarity];
        var original = equipmentList[Random.Range(0, equipmentList.Count)];

        // ���纻 ����
        var copy = Instantiate(original);
        copy.name = original.name;

        return copy;
    }

    protected override EquipmentRarity DetermineRarity()
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
            if (IsRarityGreaterOrEqual(rate.rarity, guaranteedRarity))
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

    // �������̵�� �޼����
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

        // 10ȸ �Ϲ� �̱�
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

        // 11��° �̱� (��� �̻� ����)
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

        ShowMultipleResults(results, "11ȸ �̱�");
        return results;
    }

    // 55ȸ �̱�
    public List<EquipmentData> Pull55()
    {
        List<EquipmentData> results = new List<EquipmentData>();
        bool hasEpicOrBetter = false;

        // 54ȸ �̱�
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

        // 55��° �̱� (���� �̻� ����)
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

        ShowMultipleResults(results, "55ȸ �̱�");
        ShowPullStatistics(results);
        return results;
    }

    // Ÿ�Ժ� �̱� �޼���
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
            Debug.LogError($"{rarity} ����� ��� �����ϴ�!");
            return null;
        }

        var typedEquipments = itemCache[rarity].Where(e => e.equipmentType == targetType).ToList();

        if (typedEquipments.Count == 0)
        {
            // ��ü ��� ã��
            foreach (EquipmentRarity fallbackRarity in System.Enum.GetValues(typeof(EquipmentRarity)))
            {
                if (itemCache.ContainsKey(fallbackRarity))
                {
                    var fallbackTyped = itemCache[fallbackRarity].Where(e => e.equipmentType == targetType).ToList();
                    if (fallbackTyped.Count > 0)
                    {
                        Debug.LogWarning($"{rarity} {targetType} ��� {fallbackRarity} {targetType}�� ��ü�մϴ�.");
                        typedEquipments = fallbackTyped;
                        break;
                    }
                }
            }
        }

        if (typedEquipments.Count == 0)
        {
            Debug.LogError($"��� ������ {targetType} Ÿ�� ��� �ϳ��� �����ϴ�!");
            return null;
        }

        var original = typedEquipments[Random.Range(0, typedEquipments.Count)];
        var copy = Instantiate(original);
        copy.name = original.name;

        return copy;
    }

    // ��� ������Ʈ
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

    // ��� ǥ��
    private void ShowSingleResult(EquipmentData equipment)
    {
        var color = ColorUtility.ToHtmlStringRGB(equipment.GetRarityColor());
        Debug.Log($"<color=#{color}>�� ȹ��: {equipment.GetFullRarityName()} - {equipment.equipmentName} ��</color>");

        // �κ��丮�� �ڵ� �߰�
        if (autoAddToInventory && inventorySystem != null)
        {
            inventorySystem.AddItem(equipment);
        }
    }

    protected override void ShowMultipleResults(List<EquipmentData> results, string title)
    {
        base.ShowMultipleResults(results, title);

        // �κ��丮�� �ڵ� �߰�
        if (autoAddToInventory && inventorySystem != null)
        {
            int addedCount = inventorySystem.AddItems(results);
            if (addedCount < results.Count)
            {
                Debug.LogWarning($"�κ��丮 ���� ����! {results.Count}�� �� {addedCount}���� �߰���");
            }
        }
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

    [Title("�̱� ���")]
    [ButtonGroup("Gacha")]
    [Button("1ȸ �̱�", ButtonSizes.Large), GUIColor(0.3f, 0.8f, 0.3f)]
    private void Pull1Button()
    {
        if (!Application.isPlaying)
        {
            Debug.LogWarning("Play Mode������ �̱Ⱑ �����մϴ�!");
            return;
        }
        PullSingle();
    }

    [ButtonGroup("Gacha")]
    [Button("11ȸ �̱�", ButtonSizes.Large), GUIColor(0.5f, 0.5f, 1f)]
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
    private void Pull55Button()
    {
        if (!Application.isPlaying)
        {
            Debug.LogWarning("Play Mode������ �̱Ⱑ �����մϴ�!");
            return;
        }
        Pull55();
    }

    [Title("�����")]
    [Button("��� ������ ��ε�", ButtonSizes.Large)]
    [GUIColor(0.3f, 0.8f, 0.8f)]
    private void ReloadEquipmentData()
    {
        InitializeCache();
        LoadItemData();
        Debug.Log("��� �����͸� �ٽ� �ε��߽��ϴ�.");
    }

    [Button("��ü ��� �ʱ�ȭ", ButtonSizes.Medium)]
    [GUIColor(0.8f, 0.3f, 0.3f)]
    private void ResetStatistics()
    {
        InitializeStatistics();
        recentPullHistory.Clear();
        Debug.Log("��谡 �ʱ�ȭ�Ǿ����ϴ�.");
    }

    protected override void ShowRateInfo()
    {
        base.ShowRateInfo();
        Debug.Log($"����Ʈ õ��: {softPityStart}ȸ���� ���� (ȸ�� +{softPityIncrement}%)");
        Debug.Log("\n11ȸ �̱�: ��� �̻� 1�� ����");
        Debug.Log("55ȸ �̱�: ���� �̻� 1�� ����");
    }
}