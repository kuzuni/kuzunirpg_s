using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Sirenix.OdinInspector;
using RPG.Inventory.Base;
using RPG.Items.Equipment;


namespace RPG.Inventory
{


    public class EquipmentInventorySystem : BaseInventorySystem<EquipmentData, InventorySlot<EquipmentData>>
    {
        [Title("장비 인벤토리 통계")]
        [ShowInInspector, ReadOnly]
        public override int TotalItems => inventory.Sum(slot => slot.quantity);

        [ShowInInspector, ReadOnly]
        [DictionaryDrawerSettings(KeyLabel = "등급", ValueLabel = "개수")]
        private Dictionary<EquipmentRarity, int> InventoryByRarity
        {
            get
            {
                var result = new Dictionary<EquipmentRarity, int>();
                foreach (EquipmentRarity rarity in System.Enum.GetValues(typeof(EquipmentRarity)))
                {
                    result[rarity] = 0;
                }

                foreach (var slot in inventory)
                {
                    if (slot.item != null)
                    {
                        result[slot.item.rarity] += slot.quantity;
                    }
                }

                return result;
            }
        }

        [ShowInInspector, ReadOnly]
        [DictionaryDrawerSettings(KeyLabel = "타입", ValueLabel = "개수")]
        private Dictionary<EquipmentType, int> InventoryByType
        {
            get
            {
                var result = new Dictionary<EquipmentType, int>();
                foreach (EquipmentType type in System.Enum.GetValues(typeof(EquipmentType)))
                {
                    result[type] = 0;
                }

                foreach (var slot in inventory)
                {
                    if (slot.item != null)
                    {
                        result[slot.item.equipmentType] += slot.quantity;
                    }
                }

                return result;
            }
        }
        public override bool AddItem(EquipmentData item, int quantity = 1)
        {
            if (item == null || quantity <= 0) return false;

            Debug.Log($"[EquipmentInventorySystem.AddItem] 시작");
            Debug.Log($"  - 아이템: {item.GetFullRarityName()} {item.equipmentName}");
            Debug.Log($"  - 인스턴스 ID: {item.GetInstanceID()}");
            Debug.Log($"  - 추가할 수량: {quantity}");

            // 같은 아이템이 있는지 확인
            var existingSlot = inventory.FirstOrDefault(slot =>
                GetItemFromSlot(slot) != null && IsSameItem(GetItemFromSlot(slot), item));

            if (existingSlot != null)
            {
                var existingItem = GetItemFromSlot(existingSlot);
                Debug.Log($"[기존 슬롯 발견]");
                Debug.Log($"  - 기존 아이템: {existingItem.GetFullRarityName()} {existingItem.equipmentName}");
                Debug.Log($"  - 기존 인스턴스 ID: {existingItem.GetInstanceID()}");
                Debug.Log($"  - 현재 수량: {GetSlotQuantity(existingSlot)}");

                // 기존 슬롯에 수량 추가
                SetSlotQuantity(existingSlot, GetSlotQuantity(existingSlot) + quantity);

                Debug.Log($"  - 업데이트된 수량: {GetSlotQuantity(existingSlot)}");
            }
            else
            {
                Debug.Log($"[새 슬롯 생성]");
                // 새 슬롯 생성
                inventory.Add(CreateNewSlot(item, quantity));
            }

            // 로컬 이벤트 발생 (BaseInventorySystem의 이벤트 발생 코드를 그대로 사용)
            // OnItemAdded?.Invoke(item, quantity); // 이 부분은 protected 이벤트라 직접 호출 불가

            // GameEventManager로 전파
            TriggerGlobalItemEvent(item, true);

            // 로그
            LogItemAdded(item, quantity);

            return true;
        }
        // BaseInventorySystem 추상 메서드 구현
        protected override EquipmentData GetItemFromSlot(InventorySlot<EquipmentData> slot)
        {
            return slot?.item;
        }

        protected override void SetItemToSlot(InventorySlot<EquipmentData> slot, EquipmentData item)
        {
            if (slot != null)
                slot.item = item;
        }

        protected override int GetSlotQuantity(InventorySlot<EquipmentData> slot)
        {
            return slot?.quantity ?? 0;
        }

        protected override void SetSlotQuantity(InventorySlot<EquipmentData> slot, int quantity)
        {
            if (slot != null)
                slot.quantity = quantity;
        }

        protected override InventorySlot<EquipmentData> CreateNewSlot(EquipmentData item, int quantity)
        {
            return new InventorySlot<EquipmentData>(item, quantity);
        }

        protected override bool IsSameItem(EquipmentData item1, EquipmentData item2)
        {
            // 장비 이름과 세부등급이 모두 같을 때만 같은 아이템으로 처리
            return item1.equipmentName == item2.equipmentName &&
                   item1.subGrade == item2.subGrade &&
                   item1.rarity == item2.rarity;  // 등급도 추가로 확인
        }

        public override int GetTotalItemCount()
        {
            return TotalItems;
        }

        public override void SortInventory()
        {
            inventory = inventory
                .OrderByDescending(s => s.item.rarity)
                .ThenByDescending(s => s.item.subGrade)
                .ThenBy(s => s.item.equipmentType)
                .ThenBy(s => s.item.equipmentName)
                .ToList();

            Debug.Log("장비 인벤토리를 정렬했습니다.");
        }

        protected override void LogItemAdded(EquipmentData item, int quantity)
        {
            var color = ColorUtility.ToHtmlStringRGB(item.GetRarityColor());
            Debug.Log($"<color=#{color}>{item.GetFullRarityName()} {item.equipmentName} x{quantity}을(를) 획득했습니다!</color>");
        }

        public override void ShowInventoryStatus()
        {
            Debug.Log($"========== 장비 인벤토리 상태 ==========");
            Debug.Log($"보유 중: {UniqueItems}종류, 총 {TotalItems}개");

            foreach (var kvp in InventoryByRarity)
            {
                if (kvp.Value > 0)
                {
                    var color = ColorUtility.ToHtmlStringRGB(RarityColors.GetRarityColor(kvp.Key));
                    Debug.Log($"<color=#{color}>{RarityColors.GetRarityName(kvp.Key)}: {kvp.Value}개</color>");
                }
            }
            Debug.Log("====================================");
        }

        // 장비 전용 메서드들
        public List<EquipmentData> GetEquipmentsByType(EquipmentType type)
        {
            return inventory
                .Where(slot => slot.item != null && slot.item.equipmentType == type)
                .Select(slot => slot.item)
                .ToList();
        }

        public List<EquipmentData> GetEquipmentsByRarity(EquipmentRarity rarity)
        {
            return inventory
                .Where(slot => slot.item != null && slot.item.rarity == rarity)
                .Select(slot => slot.item)
                .ToList();
        }

        [Button("낮은 등급 일괄 판매", ButtonSizes.Large)]
        [ButtonGroup("Management")]
        [GUIColor(0.8f, 0.8f, 0.3f)]
        private void SellLowRarityItems()
        {
            var itemsToSell = inventory
                .Where(slot => slot.item != null && slot.item.rarity <= EquipmentRarity.Uncommon)
                .ToList();

            int totalGold = 0;
            foreach (var slot in itemsToSell)
            {
                totalGold += slot.item.sellPrice * slot.quantity;
                inventory.Remove(slot);
            }

            Debug.Log($"<color=yellow>{itemsToSell.Count}개 아이템을 판매하여 {totalGold} 골드를 획득했습니다!</color>");
        }
    }
}