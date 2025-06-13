// ===== EquipmentType.cs =====
// ===== IEquipmentSlot.cs =====
namespace RPG.Items.Equipment
{
    public interface IEquipmentSlot
    {
        EquipmentData CurrentEquipment { get; }
        bool Equip(EquipmentData equipment);
        EquipmentData Unequip();
        bool IsEmpty { get; }
    }
}


// ===== PlayerManager.cs 수정 =====
// PlayerManager의 Awake 메서드에 추가:
/*
private EquipmentSystem equipmentSystem;
public EquipmentSystem Equipment => equipmentSystem;

void Awake()
{
    // 기존 코드...
    
    // 장비 시스템 추가
    equipmentSystem = gameObject.AddComponent<EquipmentSystem>();
    equipmentSystem.Initialize(playerStatus);
}
*/