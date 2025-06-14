using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using RPG.Items.Equipment;
using RPG.Inventory;
using Sirenix.OdinInspector;

namespace RPG.Inventory
{
    /// <summary>
    /// 장비 합성 시스템 - 동일 장비 합성 처리
    /// </summary>
    public class EquipmentFusionSystem : MonoBehaviour
    {
        [Title("시스템 참조")]
        [SerializeField, Required]
        private EquipmentInventorySystem inventorySystem;

        [Title("합성 설정")]
        [SerializeField]
        private int fusionRequiredCount = 5; // 합성에 필요한 개수를 5개로 명시적 설정
        /// <summary>
        /// 합성에 필요한 개수를 외부에서 접근 가능하도록
        /// </summary>
        public int FusionRequiredCount => fusionRequiredCount;

        // 합성 결과 콜백
        public System.Action<EquipmentData, EquipmentData, bool> OnFusionComplete;
        public System.Action<int, int> OnAutoFusionComplete;

        private void Start()
        {
            // 인벤토리 시스템 자동 찾기
            if (inventorySystem == null)
            {
                inventorySystem = FindObjectOfType<EquipmentInventorySystem>();
            }

            // Start에서 한 번 더 확인
            Debug.Log($"[EquipmentFusionSystem] 합성 필요 개수: {fusionRequiredCount}개");
        }

        /// <summary>
        /// 장비 합성 시도
        /// </summary>
        public bool TryFusion(EquipmentData equipment, int count = -1)
        {
            // count가 -1이면 기본값 사용
            if (count == -1)
            {
                count = fusionRequiredCount;
            }

            if (equipment == null || inventorySystem == null)
            {
                Debug.LogError("장비 또는 인벤토리 시스템이 없습니다!");
                return false;
            }

            // 수량 확인
            int currentCount = inventorySystem.GetItemCount(equipment);
            if (currentCount < count)
            {
                Debug.LogError($"장비 부족! 현재: {currentCount}, 필요: {count}");
                return false;
            }

            // 합성 타입 결정
            bool isMaxSubGrade = equipment.subGrade >= 5;

            if (isMaxSubGrade && equipment.rarity < EquipmentRarity.Celestial)
            {
                // 등급 승급 합성
                return PerformRarityUpgradeFusion(equipment);
            }
            else if (!isMaxSubGrade)
            {
                // 세부등급 강화 합성
                return PerformSubGradeUpgradeFusion(equipment);
            }
            else
            {
                Debug.Log("이미 최고 등급입니다!");
                return false;
            }
        }

        /// <summary>
        /// 세부등급 강화 합성 (실패 없음)
        /// </summary>
        private bool PerformSubGradeUpgradeFusion(EquipmentData baseEquipment)
        {
            Debug.Log($"[합성 시작] {baseEquipment.GetFullRarityName()} {baseEquipment.equipmentName}");
            Debug.Log($"[합성 시작] baseEquipment 인스턴스 ID: {baseEquipment.GetInstanceID()}");

            // 합성 전 개수 확인
            int beforeCount = inventorySystem.GetItemCount(baseEquipment);
            Debug.Log($"[합성 전] 개수: {beforeCount}개");

            // 재료 소모
            bool removed = inventorySystem.RemoveItem(baseEquipment, fusionRequiredCount);
            if (!removed)
            {
                Debug.LogError($"아이템 제거 실패!");
                return false;
            }

            // 합성 후 개수 확인
            int afterCount = inventorySystem.GetItemCount(baseEquipment);
            Debug.Log($"[재료 소모 후] 개수: {afterCount}개");

            // 인벤토리의 모든 아이템 확인
            var allEquipments = inventorySystem.GetAllItems();
            Debug.Log($"[인벤토리] 총 아이템 종류: {allEquipments.Count}");

            // 업그레이드된 장비 찾기
            var upgradedEquipment = allEquipments.FirstOrDefault(e =>
                e.equipmentName == baseEquipment.equipmentName &&
                e.equipmentType == baseEquipment.equipmentType &&
                e.rarity == baseEquipment.rarity &&
                e.subGrade == baseEquipment.subGrade + 1);

            if (upgradedEquipment != null)
            {
                Debug.Log($"[기존 장비 발견] {upgradedEquipment.GetFullRarityName()} {upgradedEquipment.equipmentName}");
                Debug.Log($"[기존 장비] 인스턴스 ID: {upgradedEquipment.GetInstanceID()}");

                // 추가 전 개수
                int beforeAddCount = inventorySystem.GetItemCount(upgradedEquipment);
                Debug.Log($"[추가 전] {upgradedEquipment.GetFullRarityName()} 개수: {beforeAddCount}");

                // 기존 장비에 추가
                inventorySystem.AddItem(upgradedEquipment, 1);

                // 추가 후 개수
                int afterAddCount = inventorySystem.GetItemCount(upgradedEquipment);
                Debug.Log($"[추가 후] {upgradedEquipment.GetFullRarityName()} 개수: {afterAddCount}");
            }
            else
            {
                Debug.Log($"[새 장비 생성 필요] {baseEquipment.equipmentName} {baseEquipment.rarity} {baseEquipment.subGrade + 1}★");

                // 새로 생성
                upgradedEquipment = CreateUpgradedEquipment(baseEquipment, baseEquipment.subGrade + 1, baseEquipment.rarity);
                Debug.Log($"[새 장비 생성됨] 인스턴스 ID: {upgradedEquipment.GetInstanceID()}");

                inventorySystem.AddItem(upgradedEquipment, 1);

                // 추가 확인
                int newItemCount = inventorySystem.GetItemCount(upgradedEquipment);
                Debug.Log($"[새 장비 추가 후] 개수: {newItemCount}");
            }

            Debug.Log($"[합성 완료] {upgradedEquipment.GetFullRarityName()} {upgradedEquipment.equipmentName}");

            OnFusionComplete?.Invoke(baseEquipment, upgradedEquipment, true);
            return true;
        }
        /// <summary>
        /// 등급 승급 합성 (실패 없음)
        /// </summary>
        private bool PerformRarityUpgradeFusion(EquipmentData baseEquipment)
        {
            Debug.Log($"등급 승급 합성 시도 - 100% 성공 (필요 개수: {fusionRequiredCount}개)");

            // 합성 전 개수 확인
            int beforeCount = inventorySystem.GetItemCount(baseEquipment);
            Debug.Log($"합성 전 개수 보유: {beforeCount}개");

            // 재료 소모 - 명시적으로 fusionRequiredCount 사용
            bool removed = inventorySystem.RemoveItem(baseEquipment, fusionRequiredCount);
            if (!removed)
            {
                Debug.LogError($"아이템 제거 실패! 제거하려던 개수: {fusionRequiredCount}");
                return false;
            }

            // 합성 후 개수 확인
            int afterCount = inventorySystem.GetItemCount(baseEquipment);
            Debug.Log($"합성 후 개수 보유: {afterCount}개 (재료 소모: {beforeCount - afterCount}개)");

            // 다음 등급 가져오기
            EquipmentRarity nextRarity = GetNextRarity(baseEquipment.rarity);

            // 합성 후 인벤토리에서 업그레이드된 장비 찾기
            var allEquipments = inventorySystem.GetAllItems();
            var upgradedEquipment = allEquipments.FirstOrDefault(e =>
                e.equipmentName == baseEquipment.equipmentName &&
                e.equipmentType == baseEquipment.equipmentType &&
                e.rarity == nextRarity &&
                e.subGrade == 1);

            if (upgradedEquipment != null)
            {
                // 기존 장비 사용
                inventorySystem.AddItem(upgradedEquipment, 1);
            }
            else
            {
                // 새로 생성
                upgradedEquipment = CreateUpgradedEquipment(baseEquipment, 1, nextRarity);
                inventorySystem.AddItem(upgradedEquipment, 1);
            }

            Debug.Log($"<color=yellow>★등급 승급 성공!★ {upgradedEquipment.GetFullRarityName()} {upgradedEquipment.equipmentName} 획득!</color>");
            OnFusionComplete?.Invoke(baseEquipment, upgradedEquipment, true);
            return true;
        }

        /// <summary>
        /// 자동 합성 실행
        /// </summary>
        public void PerformAutoFusion(EquipmentType equipmentType)
        {
            if (inventorySystem == null) return;

            Debug.Log($"[자동 합성 시작] 필요 개수: {fusionRequiredCount}개");

            int totalFusionCount = 0;
            int successCount = 0;
            bool hasMoreFusions = true;

            // 더 이상 합성할 것이 없을 때까지 반복
            while (hasMoreFusions)
            {
                hasMoreFusions = false;

                // 매번 새로 장비 목록을 가져옴
                var allEquipments = inventorySystem.GetEquipmentsByType(equipmentType);

                // 등급과 세부등급이 낮은 것부터 정렬 (낮은 것부터 합성)
                var sortedEquipments = allEquipments
                    .OrderBy(e => e.rarity)
                    .ThenBy(e => e.subGrade)
                    .ThenBy(e => e.equipmentName)
                    .ToList();

                // 각 장비별로 그룹화
                var equipmentGroups = sortedEquipments
                    .GroupBy(e => new { e.equipmentName, e.subGrade, e.rarity })
                    .ToList();

                foreach (var group in equipmentGroups)
                {
                    var equipment = group.First();
                    int count = inventorySystem.GetItemCount(equipment);

                    if (count >= fusionRequiredCount)
                    {
                        Debug.Log($"[자동 합성] {equipment.GetFullRarityName()} {equipment.equipmentName}: {count}개 보유");

                        // 이 장비로 가능한 모든 합성 수행
                        while (count >= fusionRequiredCount)
                        {
                            totalFusionCount++;

                            if (TryFusion(equipment, fusionRequiredCount))
                            {
                                successCount++;
                                hasMoreFusions = true; // 합성했으므로 다시 확인 필요
                            }
                            else
                            {
                                break; // 합성 실패 시 중단
                            }

                            // 남은 개수 재확인
                            count = inventorySystem.GetItemCount(equipment);
                        }
                    }
                }
            }

            if (totalFusionCount > 0)
            {
                Debug.Log($"<color=yellow>자동 합성 완료: {totalFusionCount}회 시도, {successCount}회 성공</color>");
                OnAutoFusionComplete?.Invoke(totalFusionCount, successCount);
            }
            else
            {
                Debug.Log("합성 가능한 장비가 없습니다!");
                OnAutoFusionComplete?.Invoke(0, 0);
            }
        }
        /// <summary>
        /// 업그레이드된 장비 생성
        /// </summary>
        private EquipmentData CreateUpgradedEquipment(EquipmentData baseEquipment, int newSubGrade, EquipmentRarity newRarity)
        {
            // 새 장비 생성 (ScriptableObject 복사)
            var upgradedEquipment = ScriptableObject.CreateInstance<EquipmentData>();

            // 기본 정보 복사
            upgradedEquipment.name = baseEquipment.name;
            upgradedEquipment.equipmentName = baseEquipment.equipmentName;
            upgradedEquipment.equipmentType = baseEquipment.equipmentType;
            upgradedEquipment.icon = baseEquipment.icon;
            upgradedEquipment.description = baseEquipment.description;

            // 새로운 등급 설정
            upgradedEquipment.rarity = newRarity;
            upgradedEquipment.subGrade = newSubGrade;

            // 스탯 계산 (등급 변화 시 스탯 증가)
            float statMultiplier = GetRarityStatMultiplier(newRarity) / GetRarityStatMultiplier(baseEquipment.rarity);

            switch (baseEquipment.equipmentType)
            {
                case EquipmentType.Weapon:
                    upgradedEquipment.attackPowerBonus = Mathf.RoundToInt(baseEquipment.attackPowerBonus * statMultiplier);
                    break;
                case EquipmentType.Armor:
                    upgradedEquipment.maxHpBonus = Mathf.RoundToInt(baseEquipment.maxHpBonus * statMultiplier);
                    break;
                case EquipmentType.Ring:
                    upgradedEquipment.hpRegenBonus = baseEquipment.hpRegenBonus * statMultiplier;
                    break;
            }

            // 가격도 조정
            upgradedEquipment.buyPrice = Mathf.RoundToInt(baseEquipment.buyPrice * statMultiplier);
            upgradedEquipment.sellPrice = upgradedEquipment.buyPrice / 2;

            return upgradedEquipment;
        }

        #region 유틸리티 메서드

        private EquipmentRarity GetNextRarity(EquipmentRarity current)
        {
            switch (current)
            {
                case EquipmentRarity.Common: return EquipmentRarity.Uncommon;
                case EquipmentRarity.Uncommon: return EquipmentRarity.Rare;
                case EquipmentRarity.Rare: return EquipmentRarity.Epic;
                case EquipmentRarity.Epic: return EquipmentRarity.Legendary;
                case EquipmentRarity.Legendary: return EquipmentRarity.Mythic;
                case EquipmentRarity.Mythic: return EquipmentRarity.Celestial;
                default: return current;
            }
        }

        private float GetRarityStatMultiplier(EquipmentRarity rarity)
        {
            switch (rarity)
            {
                case EquipmentRarity.Common: return 1.0f;
                case EquipmentRarity.Uncommon: return 1.5f;
                case EquipmentRarity.Rare: return 2.5f;
                case EquipmentRarity.Epic: return 4.0f;
                case EquipmentRarity.Legendary: return 6.5f;
                case EquipmentRarity.Mythic: return 10.0f;
                case EquipmentRarity.Celestial: return 15.0f;
                default: return 1.0f;
            }
        }

        /// <summary>
        /// 합성 가능 여부 확인
        /// </summary>
        public bool CanFuse(EquipmentData equipment)
        {
            if (equipment == null || inventorySystem == null) return false;

            int count = inventorySystem.GetItemCount(equipment);
            return count >= fusionRequiredCount;
        }

        /// <summary>
        /// 합성 미리보기
        /// </summary>
        public FusionPreview GetFusionPreview(EquipmentData equipment)
        {
            var preview = new FusionPreview();

            if (equipment == null)
            {
                preview.isValid = false;
                return preview;
            }

            preview.isValid = true;
            preview.baseEquipment = equipment;
            preview.requiredCount = fusionRequiredCount;

            // 합성 타입 판단
            if (equipment.subGrade >= 5 && equipment.rarity < EquipmentRarity.Celestial)
            {
                // 등급 승급
                preview.fusionType = FusionType.RarityUpgrade;
                preview.successRate = 1.0f; // 100% 성공
                preview.resultRarity = GetNextRarity(equipment.rarity);
                preview.resultSubGrade = 1;
            }
            else if (equipment.subGrade < 5)
            {
                // 세부등급 강화
                preview.fusionType = FusionType.SubGradeUpgrade;
                preview.successRate = 1.0f; // 100% 성공
                preview.resultRarity = equipment.rarity;
                preview.resultSubGrade = equipment.subGrade + 1;
            }
            else
            {
                // 최고 등급
                preview.fusionType = FusionType.MaxLevel;
                preview.successRate = 0f;
            }

            return preview;
        }

        #endregion

        [Title("디버그")]
        [Button("합성 정보 출력")]
        private void DebugPrintFusionInfo()
        {
            Debug.Log("===== 장비 합성 시스템 =====");
            Debug.Log($"필요 개수: {fusionRequiredCount}개");
            Debug.Log("성공률: 100% (실패 없음)");
            Debug.Log("\n세부등급 강화: 1★ → 2★ → 3★ → 4★ → 5★");
            Debug.Log("등급 승급: 5★ 장비 → 다음 등급 1★");
            Debug.Log("=============================");
        }

        [Button("현재 설정 확인")]
        private void CheckCurrentSettings()
        {
            Debug.Log($"[EquipmentFusionSystem] 현재 fusionRequiredCount: {fusionRequiredCount}");

            if (inventorySystem != null)
            {
                Debug.Log("[EquipmentFusionSystem] InventorySystem 연결됨");
            }
            else
            {
                Debug.LogError("[EquipmentFusionSystem] InventorySystem이 없습니다!");
            }
        }
    }


    /// <summary>
    /// 합성 미리보기 정보
    /// </summary>
    [System.Serializable]
    public class FusionPreview
    {
        public bool isValid;
        public EquipmentData baseEquipment;
        public FusionType fusionType;
        public int requiredCount;
        public float successRate;
        public EquipmentRarity resultRarity;
        public int resultSubGrade;
    }

    public enum FusionType
    {
        SubGradeUpgrade,    // 세부등급 강화
        RarityUpgrade,      // 등급 승급
        MaxLevel           // 최고 등급
    }
}