//// ===== EquipmentGachaSystem.cs 수정 버전 =====
//// 기존 코드에 추가할 부분

//using Sirenix.OdinInspector;
//using System.Collections.Generic;
//using System.Linq;
//using UnityEngine;

//public partial class EquipmentGachaSystem : MonoBehaviour
//{
//    [Title("타입별 뽑기")]
//    [TabGroup("TypedGacha", "무기 뽑기")]
//    [ButtonGroup("TypedGacha/무기 뽑기/Buttons")]
//    [Button("무기 1회", ButtonSizes.Large), GUIColor(1f, 0.5f, 0.5f)]
//    private void PullWeapon1()
//    {
//        if (!Application.isPlaying)
//        {
//            Debug.LogWarning("Play Mode에서만 뽑기가 가능합니다!");
//            return;
//        }
//        var result = PerformTypedSinglePull(EquipmentType.Weapon);
//        if (result != null)
//        {
//            ShowSingleResult(result);
//        }
//    }

//    [ButtonGroup("TypedGacha/무기 뽑기/Buttons")]
//    [Button("무기 11회", ButtonSizes.Large), GUIColor(1f, 0.5f, 0.5f)]
//    private void PullWeapon11()
//    {
//        if (!Application.isPlaying)
//        {
//            Debug.LogWarning("Play Mode에서만 뽑기가 가능합니다!");
//            return;
//        }
//        var results = PerformTyped11Pull(EquipmentType.Weapon);
//        ShowMultipleResults(results, "무기 11회 뽑기");
//    }

//    [ButtonGroup("TypedGacha/무기 뽑기/Buttons")]
//    [Button("무기 55회", ButtonSizes.Large), GUIColor(1f, 0.5f, 0.5f)]
//    private void PullWeapon55()
//    {
//        if (!Application.isPlaying)
//        {
//            Debug.LogWarning("Play Mode에서만 뽑기가 가능합니다!");
//            return;
//        }
//        var results = PerformTyped55Pull(EquipmentType.Weapon);
//        ShowMultipleResults(results, "무기 55회 뽑기");
//        ShowPullStatistics(results);
//    }

//    [TabGroup("TypedGacha", "갑옷 뽑기")]
//    [ButtonGroup("TypedGacha/갑옷 뽑기/Buttons")]
//    [Button("갑옷 1회", ButtonSizes.Large), GUIColor(0.5f, 1f, 0.5f)]
//    private void PullArmor1()
//    {
//        if (!Application.isPlaying)
//        {
//            Debug.LogWarning("Play Mode에서만 뽑기가 가능합니다!");
//            return;
//        }
//        var result = PerformTypedSinglePull(EquipmentType.Armor);
//        if (result != null)
//        {
//            ShowSingleResult(result);
//        }
//    }

//    [ButtonGroup("TypedGacha/갑옷 뽑기/Buttons")]
//    [Button("갑옷 11회", ButtonSizes.Large), GUIColor(0.5f, 1f, 0.5f)]
//    private void PullArmor11()
//    {
//        if (!Application.isPlaying)
//        {
//            Debug.LogWarning("Play Mode에서만 뽑기가 가능합니다!");
//            return;
//        }
//        var results = PerformTyped11Pull(EquipmentType.Armor);
//        ShowMultipleResults(results, "갑옷 11회 뽑기");
//    }

//    [ButtonGroup("TypedGacha/갑옷 뽑기/Buttons")]
//    [Button("갑옷 55회", ButtonSizes.Large), GUIColor(0.5f, 1f, 0.5f)]
//    private void PullArmor55()
//    {
//        if (!Application.isPlaying)
//        {
//            Debug.LogWarning("Play Mode에서만 뽑기가 가능합니다!");
//            return;
//        }
//        var results = PerformTyped55Pull(EquipmentType.Armor);
//        ShowMultipleResults(results, "갑옷 55회 뽑기");
//        ShowPullStatistics(results);
//    }

//    [TabGroup("TypedGacha", "반지 뽑기")]
//    [ButtonGroup("TypedGacha/반지 뽑기/Buttons")]
//    [Button("반지 1회", ButtonSizes.Large), GUIColor(0.5f, 0.5f, 1f)]
//    private void PullRing1()
//    {
//        if (!Application.isPlaying)
//        {
//            Debug.LogWarning("Play Mode에서만 뽑기가 가능합니다!");
//            return;
//        }
//        var result = PerformTypedSinglePull(EquipmentType.Ring);
//        if (result != null)
//        {
//            ShowSingleResult(result);
//        }
//    }

//    [ButtonGroup("TypedGacha/반지 뽑기/Buttons")]
//    [Button("반지 11회", ButtonSizes.Large), GUIColor(0.5f, 0.5f, 1f)]
//    private void PullRing11()
//    {
//        if (!Application.isPlaying)
//        {
//            Debug.LogWarning("Play Mode에서만 뽑기가 가능합니다!");
//            return;
//        }
//        var results = PerformTyped11Pull(EquipmentType.Ring);
//        ShowMultipleResults(results, "반지 11회 뽑기");
//    }

//    [ButtonGroup("TypedGacha/반지 뽑기/Buttons")]
//    [Button("반지 55회", ButtonSizes.Large), GUIColor(0.5f, 0.5f, 1f)]
//    private void PullRing55()
//    {
//        if (!Application.isPlaying)
//        {
//            Debug.LogWarning("Play Mode에서만 뽑기가 가능합니다!");
//            return;
//        }
//        var results = PerformTyped55Pull(EquipmentType.Ring);
//        ShowMultipleResults(results, "반지 55회 뽑기");
//        ShowPullStatistics(results);
//    }

//    // 타입별 뽑기 핵심 메서드들
//    private EquipmentData PerformTypedSinglePull(EquipmentType targetType)
//    {
//        currentPityCount++;

//        // 천장 도달
//        if (currentPityCount >= pityCount)
//        {
//            currentPityCount = 0;
//            return PerformTypedGuaranteedPull(guaranteedRarity, targetType);
//        }

//        // 등급 결정
//        EquipmentRarity selectedRarity = DetermineRarity();

//        // 높은 등급이면 천장 리셋
//        if (selectedRarity >= guaranteedRarity)
//        {
//            currentPityCount = 0;
//        }

//        // 특정 타입의 장비만 선택
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
//            Debug.Log($"<color=yellow>★보장★ {equipment.GetFullRarityName()} {equipment.equipmentName} 획득!</color>");
//        }

//        return equipment;
//    }

//    private List<EquipmentData> PerformTyped11Pull(EquipmentType targetType)
//    {
//        List<EquipmentData> results = new List<EquipmentData>();
//        bool hasRareOrBetter = false;

//        // 10회 일반 뽑기
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

//        // 11번째 뽑기 (희귀 이상 보장)
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

//        // 54회 뽑기
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

//        // 55번째 뽑기 (영웅 이상 보장)
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
//            Debug.LogError("Equipment cache is null! 재로드가 필요합니다.");
//            LoadEquipmentData();
//        }

//        if (!equipmentCache.ContainsKey(rarity) || equipmentCache[rarity] == null || equipmentCache[rarity].Count == 0)
//        {
//            Debug.LogError($"{rarity} 등급의 장비가 없습니다!");
//            return null;
//        }

//        // 해당 등급에서 특정 타입의 장비만 필터링
//        var typedEquipments = equipmentCache[rarity].Where(e => e.equipmentType == targetType).ToList();

//        if (typedEquipments.Count == 0)
//        {
//            Debug.LogError($"{rarity} 등급의 {targetType} 타입 장비가 없습니다!");

//            // 대체 로직: 해당 타입이 있는 가장 낮은 등급 찾기
//            foreach (EquipmentRarity fallbackRarity in System.Enum.GetValues(typeof(EquipmentRarity)))
//            {
//                if (equipmentCache.ContainsKey(fallbackRarity))
//                {
//                    var fallbackTyped = equipmentCache[fallbackRarity].Where(e => e.equipmentType == targetType).ToList();
//                    if (fallbackTyped.Count > 0)
//                    {
//                        Debug.LogWarning($"{rarity} {targetType} 대신 {fallbackRarity} {targetType}로 대체합니다.");
//                        typedEquipments = fallbackTyped;
//                        break;
//                    }
//                }
//            }

//            if (typedEquipments.Count == 0)
//            {
//                Debug.LogError($"사용 가능한 {targetType} 타입 장비가 하나도 없습니다!");
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

//    [Title("타입별 장비 개수")]
//    [ShowInInspector, ReadOnly]
//    [DictionaryDrawerSettings(KeyLabel = "타입", ValueLabel = "개수")]
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