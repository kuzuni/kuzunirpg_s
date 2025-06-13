using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Sirenix.OdinInspector;
using System;
using RPG.Player;
using RPG.Inventory.Base;
using RPG.Items.Relic;
using RPG.Common;



namespace RPG.Inventory
{

    // RelicInstance를 직접 슬롯으로 사용
    public class RelicInventorySystem : BaseInventorySystem<RelicInstance, RelicInstance>
    {
        [Title("유물 인벤토리 통계")]
        [ShowInInspector, ReadOnly]
        public override int TotalItems => inventory.Count;

        [ShowInInspector, ReadOnly]
        [DictionaryDrawerSettings(KeyLabel = "등급", ValueLabel = "개수")]
        private Dictionary<RelicRarity, int> RelicsByRarity
        {
            get
            {
                var result = new Dictionary<RelicRarity, int>();
                foreach (RelicRarity rarity in Enum.GetValues(typeof(RelicRarity)))
                {
                    result[rarity] = 0;
                }

                foreach (var relic in inventory)
                {
                    if (relic?.relicData != null)
                    {
                        result[relic.relicData.rarity]++;
                    }
                }

                return result;
            }
        }

        [ShowInInspector, ReadOnly]
        [DictionaryDrawerSettings(KeyLabel = "타입", ValueLabel = "개수")]
        private Dictionary<RelicType, int> RelicsByType
        {
            get
            {
                var result = new Dictionary<RelicType, int>();
                foreach (RelicType type in Enum.GetValues(typeof(RelicType)))
                {
                    result[type] = 0;
                }

                foreach (var relic in inventory)
                {
                    if (relic?.relicData != null)
                    {
                        result[relic.relicData.relicType]++;
                    }
                }

                return result;
            }
        }

        [Title("합성 설정")]
        [SerializeField]
        private bool showFusionAnimation = true;

        // 유물 전용 이벤트 (BaseInventorySystem의 이벤트와 별개)
        public event Action<RelicInstance> OnRelicAdded;
        public event Action<RelicInstance> OnRelicRemoved;
        public event Action<RelicInstance, bool> OnFusionAttempt;

        // BaseInventorySystem 추상 메서드 구현
        protected override RelicInstance GetItemFromSlot(RelicInstance slot)
        {
            return slot; // RelicInstance를 직접 슬롯으로 사용
        }

        protected override void SetItemToSlot(RelicInstance slot, RelicInstance item)
        {
            // RelicInstance는 참조 타입이므로 직접 수정 불가
            // 이 경우는 사용하지 않음
        }

        protected override int GetSlotQuantity(RelicInstance slot)
        {
            return 1; // 유물은 스택 불가, 항상 1개
        }

        protected override void SetSlotQuantity(RelicInstance slot, int quantity)
        {
            // 유물은 스택 불가
        }

        protected override RelicInstance CreateNewSlot(RelicInstance item, int quantity)
        {
            return item; // RelicInstance를 그대로 반환
        }

        protected override bool IsSameItem(RelicInstance item1, RelicInstance item2)
        {
            // 유물은 각각 고유하므로 항상 false
            return false;
        }

        // BaseInventorySystem의 AddItem 오버라이드
        public override bool AddItem(RelicInstance item, int quantity = 1)
        {
            bool result = base.AddItem(item, quantity);
            if (result && item != null)
            {
                OnRelicAdded?.Invoke(item);
            }
            return result;
        }

        // BaseInventorySystem의 RemoveItem 오버라이드
        public override bool RemoveItem(RelicInstance item, int quantity = 1)
        {
            bool result = base.RemoveItem(item, quantity);
            if (result && item != null)
            {
                OnRelicRemoved?.Invoke(item);
            }
            return result;
        }

        public override int GetTotalItemCount()
        {
            return TotalItems;
        }

        public override void SortInventory()
        {
            inventory = inventory
                .Where(r => r != null && r.relicData != null)
                .OrderByDescending(r => r.relicData.rarity)
                .ThenByDescending(r => r.level)
                .ThenBy(r => r.relicData.relicType)
                .ThenBy(r => r.relicData.relicName)
                .ToList();

            Debug.Log("유물 인벤토리를 정렬했습니다.");
        }

        protected override void LogItemAdded(RelicInstance item, int quantity)
        {
            if (item?.relicData != null)
            {
                var color = ColorUtility.ToHtmlStringRGB(item.relicData.GetRarityColor());
                Debug.Log($"<color=#{color}>[{item.relicData.GetRarityName()}] {item.relicData.relicName} Lv.{item.level}을(를) 획득했습니다!</color>");
            }
        }

        public override void ShowInventoryStatus()
        {
            Debug.Log($"========== 유물 인벤토리 상태 ==========");
            Debug.Log($"보유 중: {TotalItems}개");

            foreach (var kvp in RelicsByRarity)
            {
                if (kvp.Value > 0)
                {
                    var color = ColorUtility.ToHtmlStringRGB(RelicRarityColors.GetRarityColor(kvp.Key));
                    Debug.Log($"<color=#{color}>{RelicRarityColors.GetRarityName(kvp.Key)}: {kvp.Value}개</color>");
                }
            }

            // 고레벨 유물 표시
            var highLevelRelics = inventory.Where(r => r != null && r.level >= 50).OrderByDescending(r => r.level);
            if (highLevelRelics.Any())
            {
                Debug.Log("\n--- 고레벨 유물 (50+) ---");
                foreach (var relic in highLevelRelics.Take(5))
                {
                    var color = ColorUtility.ToHtmlStringRGB(relic.relicData.GetRarityColor());
                    Debug.Log($"<color=#{color}>{relic.relicData.relicName} Lv.{relic.level}</color>");
                }
            }

            Debug.Log("====================================");
        }

        // 모든 유물 가져오기
        public List<RelicInstance> GetAllRelics()
        {
            return GetAllItems();
        }

        // 유물 전용 메서드들
        [Title("합성 기능")]
        [Button("유물 합성", ButtonSizes.Large), GUIColor(0.8f, 0.5f, 0.8f)]
        public bool TryFuseRelics(RelicInstance targetRelic, RelicInstance materialRelic)
        {
            if (targetRelic == null || materialRelic == null)
            {
                Debug.LogError("합성할 유물을 선택해주세요!");
                return false;
            }

            if (targetRelic == materialRelic)
            {
                Debug.LogError("같은 유물끼리는 합성할 수 없습니다!");
                return false;
            }

            if (targetRelic.level >= 100)
            {
                Debug.LogWarning($"{targetRelic.relicData.relicName}은(는) 이미 최대 레벨입니다!");
                return false;
            }

            // 합성 확률 계산
            float successRate = targetRelic.GetFusionSuccessRate();

            // 재료 유물 제거
            RemoveItem(materialRelic);

            // 확률 판정
            bool success = UnityEngine.Random.Range(0f, 1f) <= successRate;

            if (success)
            {
                targetRelic.level++;
                var color = ColorUtility.ToHtmlStringRGB(targetRelic.relicData.GetRarityColor());
                Debug.Log($"<color=#{color}>★ 합성 성공! {targetRelic.relicData.relicName} Lv.{targetRelic.level} ★</color>");

                if (targetRelic.level == 100)
                {
                    Debug.Log($"<color=yellow>★★★ 축하합니다! {targetRelic.relicData.relicName}이(가) 최대 레벨에 도달했습니다! ★★★</color>");
                }
            }
            else
            {
                Debug.Log($"<color=red>합성 실패... (성공률: {successRate:P0})</color>");
            }

            OnFusionAttempt?.Invoke(targetRelic, success);
            return success;
        }

        // 같은 유물 찾기 (합성용)
        public List<RelicInstance> GetSameRelics(RelicData relicData)
        {
            return inventory.Where(r => r?.relicData == relicData).ToList();
        }

        // 특정 타입의 유물 찾기
        public List<RelicInstance> GetRelicsByType(RelicType type)
        {
            return inventory.Where(r => r?.relicData != null && r.relicData.relicType == type).ToList();
        }

        // 특정 등급의 유물 찾기
        public List<RelicInstance> GetRelicsByRarity(RelicRarity rarity)
        {
            return inventory.Where(r => r?.relicData != null && r.relicData.rarity == rarity).ToList();
        }

        [Button("낮은 등급 일괄 합성", ButtonSizes.Large)]
        [ButtonGroup("Management")]
        [GUIColor(0.8f, 0.8f, 0.3f)]
        private void AutoFuseCommonRelics()
        {
            var commonRelics = GetRelicsByRarity(RelicRarity.Common)
                .GroupBy(r => r.relicData)
                .Where(g => g.Count() >= 2)
                .ToList();

            int fusionCount = 0;
            foreach (var group in commonRelics)
            {
                var relicList = group.ToList();
                var targetRelic = relicList.OrderByDescending(r => r.level).First();

                for (int i = 1; i < relicList.Count && targetRelic.level < 100; i++)
                {
                    if (TryFuseRelics(targetRelic, relicList[i]))
                    {
                        fusionCount++;
                    }
                }
            }

            Debug.Log($"<color=yellow>일반 등급 유물 {fusionCount}회 합성 시도 완료!</color>");
        }

        [Title("합성 테스트")]
        [InfoBox("테스트용 합성 기능입니다. 인덱스를 선택하여 합성을 시도합니다.")]
        [HorizontalGroup("FusionTest", 0.5f)]
        [VerticalGroup("FusionTest/Target")]
        [LabelText("대상 유물 인덱스")]
        [PropertyRange(0, "@inventory.Count - 1")]
        public int targetIndex = 0;

        [VerticalGroup("FusionTest/Material")]
        [LabelText("재료 유물 인덱스")]
        [PropertyRange(0, "@inventory.Count - 1")]
        public int materialIndex = 1;

        [Button("선택한 유물 합성", ButtonSizes.Large), GUIColor(0.5f, 0.8f, 0.5f)]
        private void TestFusion()
        {
            if (targetIndex < 0 || targetIndex >= inventory.Count ||
                materialIndex < 0 || materialIndex >= inventory.Count)
            {
                Debug.LogError("유효한 인덱스를 선택해주세요!");
                return;
            }

            TryFuseRelics(inventory[targetIndex], inventory[materialIndex]);
        }

        [Title("유물 효과 총합")]
        [ShowInInspector, ReadOnly]
        [DictionaryDrawerSettings(KeyLabel = "스탯", ValueLabel = "총 보너스")]
        private Dictionary<StatType, string> TotalRelicBonuses
        {
            get
            {
                var totals = new Dictionary<StatType, float>();
                var result = new Dictionary<StatType, string>();

                // 모든 스탯 타입 초기화
                foreach (StatType statType in Enum.GetValues(typeof(StatType)))
                {
                    totals[statType] = 0f;
                }

                // 모든 유물의 보너스 합산
                foreach (var relic in inventory)
                {
                    if (relic != null)
                    {
                        var bonuses = relic.GetAllStatBonuses();
                        foreach (var bonus in bonuses)
                        {
                            totals[bonus.Key] += bonus.Value;
                        }
                    }
                }

                // 결과 포맷팅
                foreach (var kvp in totals)
                {
                    if (kvp.Value > 0)
                    {
                        // 스탯에 따라 퍼센트 또는 고정값으로 표시
                        bool isPercentage = kvp.Key == StatType.CritChance ||
                                          kvp.Key == StatType.CritDamage ||
                                          kvp.Key == StatType.AttackSpeed ||
                                          kvp.Key == StatType.MaxHp ||
                                          kvp.Key == StatType.AttackPower;

                        result[kvp.Key] = isPercentage ? $"+{kvp.Value:F1}%" : $"+{kvp.Value:F0}";
                    }
                }

                return result;
            }
        }
    }
}