// ===== EquipmentType.cs =====

namespace RPG.Items.Equipment
{
    public enum EquipmentType
    {
        Weapon,     // 무기
        Armor,      // 갑옷
        Ring        // 반지
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