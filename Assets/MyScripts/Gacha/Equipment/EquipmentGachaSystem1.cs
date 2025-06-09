//// ===== EquipmentGachaSystem.cs ���� ���� =====
//// ���� �ڵ忡 �߰��� �κ�

//using Sirenix.OdinInspector;
//using System.Collections.Generic;
//using System.Linq;
//using UnityEngine;

//public partial class EquipmentGachaSystem : MonoBehaviour
//{
//    [Title("Ÿ�Ժ� �̱�")]
//    [TabGroup("TypedGacha", "���� �̱�")]
//    [ButtonGroup("TypedGacha/���� �̱�/Buttons")]
//    [Button("���� 1ȸ", ButtonSizes.Large), GUIColor(1f, 0.5f, 0.5f)]
//    private void PullWeapon1()
//    {
//        if (!Application.isPlaying)
//        {
//            Debug.LogWarning("Play Mode������ �̱Ⱑ �����մϴ�!");
//            return;
//        }
//        var result = PerformTypedSinglePull(EquipmentType.Weapon);
//        if (result != null)
//        {
//            ShowSingleResult(result);
//        }
//    }

//    [ButtonGroup("TypedGacha/���� �̱�/Buttons")]
//    [Button("���� 11ȸ", ButtonSizes.Large), GUIColor(1f, 0.5f, 0.5f)]
//    private void PullWeapon11()
//    {
//        if (!Application.isPlaying)
//        {
//            Debug.LogWarning("Play Mode������ �̱Ⱑ �����մϴ�!");
//            return;
//        }
//        var results = PerformTyped11Pull(EquipmentType.Weapon);
//        ShowMultipleResults(results, "���� 11ȸ �̱�");
//    }

//    [ButtonGroup("TypedGacha/���� �̱�/Buttons")]
//    [Button("���� 55ȸ", ButtonSizes.Large), GUIColor(1f, 0.5f, 0.5f)]
//    private void PullWeapon55()
//    {
//        if (!Application.isPlaying)
//        {
//            Debug.LogWarning("Play Mode������ �̱Ⱑ �����մϴ�!");
//            return;
//        }
//        var results = PerformTyped55Pull(EquipmentType.Weapon);
//        ShowMultipleResults(results, "���� 55ȸ �̱�");
//        ShowPullStatistics(results);
//    }

//    [TabGroup("TypedGacha", "���� �̱�")]
//    [ButtonGroup("TypedGacha/���� �̱�/Buttons")]
//    [Button("���� 1ȸ", ButtonSizes.Large), GUIColor(0.5f, 1f, 0.5f)]
//    private void PullArmor1()
//    {
//        if (!Application.isPlaying)
//        {
//            Debug.LogWarning("Play Mode������ �̱Ⱑ �����մϴ�!");
//            return;
//        }
//        var result = PerformTypedSinglePull(EquipmentType.Armor);
//        if (result != null)
//        {
//            ShowSingleResult(result);
//        }
//    }

//    [ButtonGroup("TypedGacha/���� �̱�/Buttons")]
//    [Button("���� 11ȸ", ButtonSizes.Large), GUIColor(0.5f, 1f, 0.5f)]
//    private void PullArmor11()
//    {
//        if (!Application.isPlaying)
//        {
//            Debug.LogWarning("Play Mode������ �̱Ⱑ �����մϴ�!");
//            return;
//        }
//        var results = PerformTyped11Pull(EquipmentType.Armor);
//        ShowMultipleResults(results, "���� 11ȸ �̱�");
//    }

//    [ButtonGroup("TypedGacha/���� �̱�/Buttons")]
//    [Button("���� 55ȸ", ButtonSizes.Large), GUIColor(0.5f, 1f, 0.5f)]
//    private void PullArmor55()
//    {
//        if (!Application.isPlaying)
//        {
//            Debug.LogWarning("Play Mode������ �̱Ⱑ �����մϴ�!");
//            return;
//        }
//        var results = PerformTyped55Pull(EquipmentType.Armor);
//        ShowMultipleResults(results, "���� 55ȸ �̱�");
//        ShowPullStatistics(results);
//    }

//    [TabGroup("TypedGacha", "���� �̱�")]
//    [ButtonGroup("TypedGacha/���� �̱�/Buttons")]
//    [Button("���� 1ȸ", ButtonSizes.Large), GUIColor(0.5f, 0.5f, 1f)]
//    private void PullRing1()
//    {
//        if (!Application.isPlaying)
//        {
//            Debug.LogWarning("Play Mode������ �̱Ⱑ �����մϴ�!");
//            return;
//        }
//        var result = PerformTypedSinglePull(EquipmentType.Ring);
//        if (result != null)
//        {
//            ShowSingleResult(result);
//        }
//    }

//    [ButtonGroup("TypedGacha/���� �̱�/Buttons")]
//    [Button("���� 11ȸ", ButtonSizes.Large), GUIColor(0.5f, 0.5f, 1f)]
//    private void PullRing11()
//    {
//        if (!Application.isPlaying)
//        {
//            Debug.LogWarning("Play Mode������ �̱Ⱑ �����մϴ�!");
//            return;
//        }
//        var results = PerformTyped11Pull(EquipmentType.Ring);
//        ShowMultipleResults(results, "���� 11ȸ �̱�");
//    }

//    [ButtonGroup("TypedGacha/���� �̱�/Buttons")]
//    [Button("���� 55ȸ", ButtonSizes.Large), GUIColor(0.5f, 0.5f, 1f)]
//    private void PullRing55()
//    {
//        if (!Application.isPlaying)
//        {
//            Debug.LogWarning("Play Mode������ �̱Ⱑ �����մϴ�!");
//            return;
//        }
//        var results = PerformTyped55Pull(EquipmentType.Ring);
//        ShowMultipleResults(results, "���� 55ȸ �̱�");
//        ShowPullStatistics(results);
//    }

//    // Ÿ�Ժ� �̱� �ٽ� �޼����
//    private EquipmentData PerformTypedSinglePull(EquipmentType targetType)
//    {
//        currentPityCount++;

//        // õ�� ����
//        if (currentPityCount >= pityCount)
//        {
//            currentPityCount = 0;
//            return PerformTypedGuaranteedPull(guaranteedRarity, targetType);
//        }

//        // ��� ����
//        EquipmentRarity selectedRarity = DetermineRarity();

//        // ���� ����̸� õ�� ����
//        if (selectedRarity >= guaranteedRarity)
//        {
//            currentPityCount = 0;
//        }

//        // Ư�� Ÿ���� ��� ����
//        var equipment = GetRandomTypedEquipment(selectedRarity, targetType);
//        if (equipment != null)
//        {
//            UpdateStatistics(equipment);
//            AddToHistory(equipment);
//        }

//        return equipment;
//    }

//    private EquipmentData PerformTypedGuaranteedPull(EquipmentRarity minimumRarity, EquipmentType targetType)
//    {
//        var rarity = GetRandomRarityWithMinimum(minimumRarity);
//        var equipment = GetRandomTypedEquipment(rarity, targetType);

//        if (equipment != null)
//        {
//            UpdateStatistics(equipment);
//            AddToHistory(equipment);
//            Debug.Log($"<color=yellow>�ں���� {equipment.GetFullRarityName()} {equipment.equipmentName} ȹ��!</color>");
//        }

//        return equipment;
//    }

//    private List<EquipmentData> PerformTyped11Pull(EquipmentType targetType)
//    {
//        List<EquipmentData> results = new List<EquipmentData>();
//        bool hasRareOrBetter = false;

//        // 10ȸ �Ϲ� �̱�
//        for (int i = 0; i < 10; i++)
//        {
//            var equipment = PerformTypedSinglePull(targetType);
//            if (equipment != null)
//            {
//                results.Add(equipment);
//                if (equipment.rarity >= EquipmentRarity.Rare)
//                {
//                    hasRareOrBetter = true;
//                }
//            }
//        }

//        // 11��° �̱� (��� �̻� ����)
//        if (enable11PullGuarantee && !hasRareOrBetter)
//        {
//            var guaranteed = PerformTypedGuaranteedPull(EquipmentRarity.Rare, targetType);
//            results.Add(guaranteed);
//        }
//        else
//        {
//            results.Add(PerformTypedSinglePull(targetType));
//        }

//        return results;
//    }

//    private List<EquipmentData> PerformTyped55Pull(EquipmentType targetType)
//    {
//        List<EquipmentData> results = new List<EquipmentData>();
//        bool hasEpicOrBetter = false;

//        // 54ȸ �̱�
//        for (int i = 0; i < 54; i++)
//        {
//            var equipment = PerformTypedSinglePull(targetType);
//            if (equipment != null)
//            {
//                results.Add(equipment);
//                if (equipment.rarity >= EquipmentRarity.Epic)
//                {
//                    hasEpicOrBetter = true;
//                }
//            }
//        }

//        // 55��° �̱� (���� �̻� ����)
//        if (enable55PullGuarantee && !hasEpicOrBetter)
//        {
//            var guaranteed = PerformTypedGuaranteedPull(EquipmentRarity.Epic, targetType);
//            results.Add(guaranteed);
//        }
//        else
//        {
//            results.Add(PerformTypedSinglePull(targetType));
//        }

//        return results;
//    }

//    private EquipmentData GetRandomTypedEquipment(EquipmentRarity rarity, EquipmentType targetType)
//    {
//        if (equipmentCache == null)
//        {
//            Debug.LogError("Equipment cache is null! ��ε尡 �ʿ��մϴ�.");
//            LoadEquipmentData();
//        }

//        if (!equipmentCache.ContainsKey(rarity) || equipmentCache[rarity] == null || equipmentCache[rarity].Count == 0)
//        {
//            Debug.LogError($"{rarity} ����� ��� �����ϴ�!");
//            return null;
//        }

//        // �ش� ��޿��� Ư�� Ÿ���� ��� ���͸�
//        var typedEquipments = equipmentCache[rarity].Where(e => e.equipmentType == targetType).ToList();

//        if (typedEquipments.Count == 0)
//        {
//            Debug.LogError($"{rarity} ����� {targetType} Ÿ�� ��� �����ϴ�!");

//            // ��ü ����: �ش� Ÿ���� �ִ� ���� ���� ��� ã��
//            foreach (EquipmentRarity fallbackRarity in System.Enum.GetValues(typeof(EquipmentRarity)))
//            {
//                if (equipmentCache.ContainsKey(fallbackRarity))
//                {
//                    var fallbackTyped = equipmentCache[fallbackRarity].Where(e => e.equipmentType == targetType).ToList();
//                    if (fallbackTyped.Count > 0)
//                    {
//                        Debug.LogWarning($"{rarity} {targetType} ��� {fallbackRarity} {targetType}�� ��ü�մϴ�.");
//                        typedEquipments = fallbackTyped;
//                        break;
//                    }
//                }
//            }

//            if (typedEquipments.Count == 0)
//            {
//                Debug.LogError($"��� ������ {targetType} Ÿ�� ��� �ϳ��� �����ϴ�!");
//                return null;
//            }
//        }

//        var original = typedEquipments[Random.Range(0, typedEquipments.Count)];
//        var copy = Instantiate(original);
//        copy.name = original.name;

//        return copy;
//    }

//    private void UpdateStatistics(EquipmentData equipment)
//    {
//        if (equipment == null) return;

//        if (pullStatistics == null)
//        {
//            InitializeStatistics();
//        }

//        if (pullStatistics.ContainsKey(equipment.rarity))
//        {
//            pullStatistics[equipment.rarity]++;
//        }
//        else
//        {
//            pullStatistics[equipment.rarity] = 1;
//        }
//    }

//    [Title("Ÿ�Ժ� ��� ����")]
//    [ShowInInspector, ReadOnly]
//    [DictionaryDrawerSettings(KeyLabel = "Ÿ��", ValueLabel = "����")]
//    private Dictionary<EquipmentType, int> TypedEquipmentCount
//    {
//        get
//        {
//            var count = new Dictionary<EquipmentType, int>();
//            foreach (EquipmentType type in System.Enum.GetValues(typeof(EquipmentType)))
//            {
//                count[type] = 0;
//            }

//            if (equipmentCache != null)
//            {
//                foreach (var kvp in equipmentCache)
//                {
//                    foreach (var equipment in kvp.Value)
//                    {
//                        count[equipment.equipmentType]++;
//                    }
//                }
//            }

//            return count;
//        }
//    }
//}