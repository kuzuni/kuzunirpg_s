using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
using Sirenix.OdinInspector;
using RPG.Core.Events;
using RPG.Items.Equipment;
using RPG.Items.Relic;

namespace RPG.Inventory.Base
{
    // 인벤토리 시스템 베이스 클래스
    public abstract class BaseInventorySystem<T, TSlot> : MonoBehaviour, IInventorySystem<T>
        where T : class
        where TSlot : class
    {
        [Title("인벤토리 내용")]
        [SerializeField]
        [ListDrawerSettings(ShowFoldout = true, ShowPaging = true, NumberOfItemsPerPage = 10)]
        protected List<TSlot> inventory = new List<TSlot>();

        [Title("인벤토리 통계")]
        [ShowInInspector, ReadOnly]
        public abstract int TotalItems { get; }

        [ShowInInspector, ReadOnly]
        public int UniqueItems => inventory.Count;

        // 로컬 이벤트 (UI 업데이트용으로 유지)
        public event Action<T, int> OnItemAdded;
        public event Action<T, int> OnItemRemoved;

        // 추상 메서드 (하위 클래스에서 구현)
        protected abstract T GetItemFromSlot(TSlot slot);
        protected abstract void SetItemToSlot(TSlot slot, T item);
        protected abstract int GetSlotQuantity(TSlot slot);
        protected abstract void SetSlotQuantity(TSlot slot, int quantity);
        protected abstract TSlot CreateNewSlot(T item, int quantity);
        protected abstract bool IsSameItem(T item1, T item2);

        // IInventorySystem 구현
        public virtual bool AddItem(T item, int quantity = 1)
        {
            if (item == null || quantity <= 0) return false;

            // 같은 아이템이 있는지 확인
            var existingSlot = inventory.FirstOrDefault(slot =>
                GetItemFromSlot(slot) != null && IsSameItem(GetItemFromSlot(slot), item));

            if (existingSlot != null)
            {
                // 기존 슬롯에 수량 추가
                SetSlotQuantity(existingSlot, GetSlotQuantity(existingSlot) + quantity);
            }
            else
            {
                // 새 슬롯 생성
                inventory.Add(CreateNewSlot(item, quantity));
            }

            // 로컬 이벤트 발생 (인벤토리 UI 업데이트용)
            OnItemAdded?.Invoke(item, quantity);

            // GameEventManager로 전파 (아이템 타입에 따라)
            TriggerGlobalItemEvent(item, true);

            LogItemAdded(item, quantity);
            return true;
        }

        public virtual bool RemoveItem(T item, int quantity = 1)
        {
            if (item == null || quantity <= 0) return false;

            var slot = inventory.FirstOrDefault(s =>
                GetItemFromSlot(s) != null && IsSameItem(GetItemFromSlot(s), item));

            if (slot == null) return false;

            int currentQuantity = GetSlotQuantity(slot);

            if (currentQuantity > quantity)
            {
                SetSlotQuantity(slot, currentQuantity - quantity);
            }
            else
            {
                inventory.Remove(slot);
                quantity = currentQuantity; // 실제 제거된 수량
            }

            // 로컬 이벤트 발생
            OnItemRemoved?.Invoke(item, quantity);

            // GameEventManager로 전파 (필요한 경우)
            TriggerGlobalItemEvent(item, false);

            return true;
        }

        // 전역 이벤트 발생 메서드
        protected virtual void TriggerGlobalItemEvent(T item, bool isAdded)
        {
            // 기본 구현: 아이템 타입 체크
            if (item is EquipmentData equipment && isAdded)
            {
                GameEventManager.TriggerEquipmentObtained(equipment);
            }
            else if (item is RelicInstance relic && isAdded)
            {
                GameEventManager.TriggerRelicObtained(relic);
            }
            // 다른 아이템 타입들은 하위 클래스에서 오버라이드하여 처리
        }

        public virtual int AddItems(List<T> items)
        {
            int addedCount = 0;
            foreach (var item in items)
            {
                if (AddItem(item))
                {
                    addedCount++;
                }
            }
            return addedCount;
        }

        public virtual List<T> GetAllItems()
        {
            return inventory
                .Where(slot => GetItemFromSlot(slot) != null)
                .Select(slot => GetItemFromSlot(slot))
                .ToList();
        }

        public virtual int GetItemCount(T item)
        {
            if (item == null) return 0;

            return inventory
                .Where(slot => GetItemFromSlot(slot) != null && IsSameItem(GetItemFromSlot(slot), item))
                .Sum(slot => GetSlotQuantity(slot));
        }

        public abstract int GetTotalItemCount();

        public int GetUniqueItemCount()
        {
            return UniqueItems;
        }

        public abstract void SortInventory();

        public virtual void ClearInventory()
        {
            // 모든 아이템 제거 이벤트 발생
            foreach (var slot in inventory.ToList())
            {
                var item = GetItemFromSlot(slot);
                if (item != null)
                {
                    OnItemRemoved?.Invoke(item, GetSlotQuantity(slot));
                }
            }

            inventory.Clear();
            Debug.Log("인벤토리를 초기화했습니다.");
        }

        // 보조 메서드
        protected abstract void LogItemAdded(T item, int quantity);

        [Title("인벤토리 관리")]
        [Button("인벤토리 상태", ButtonSizes.Medium)]
        [ButtonGroup("Debug")]
        public abstract void ShowInventoryStatus();
    }
}