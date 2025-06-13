using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
using Sirenix.OdinInspector;
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

        // 이벤트
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

            OnItemAdded?.Invoke(item, quantity);
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

            OnItemRemoved?.Invoke(item, quantity);
            return true;
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