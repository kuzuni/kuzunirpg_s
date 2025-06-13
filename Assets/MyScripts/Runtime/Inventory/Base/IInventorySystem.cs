using System.Collections.Generic;
using System;
namespace RPG.Inventory.Base
{

    // 인벤토리 시스템 공통 인터페이스
    public interface IInventorySystem<T> where T : class
    {
        // 아이템 추가/제거
        bool AddItem(T item, int quantity = 1);
        bool RemoveItem(T item, int quantity = 1);
        int AddItems(List<T> items);

        // 아이템 조회
        List<T> GetAllItems();
        int GetItemCount(T item);
        int GetTotalItemCount();
        int GetUniqueItemCount();

        // 인벤토리 관리
        void SortInventory();
        void ClearInventory();

        // 이벤트
        event Action<T, int> OnItemAdded;
        event Action<T, int> OnItemRemoved;
    }
}