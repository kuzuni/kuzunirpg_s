// ===== EquipmentType.cs =====
using Sirenix.OdinInspector;
using UnityEngine;
// ===== EquipmentData.cs =====


[CreateAssetMenu(fileName = "New Equipment", menuName = "RPG/Equipment")]
public class EquipmentData : ScriptableObject
{
    [Title("기본 정보")]
    [LabelText("장비 이름")]
    public string equipmentName;

    [LabelText("장비 타입")]
    public EquipmentType equipmentType;

    [LabelText("장비 아이콘")]
    [PreviewField(60)]
    public Sprite icon;

    [Title("장비 스탯")]
    [ShowIf("equipmentType", EquipmentType.Weapon)]
    [LabelText("공격력 증가")]
    [PropertyRange(0, 100)]
    public int attackPowerBonus;

    [ShowIf("equipmentType", EquipmentType.Armor)]
    [LabelText("최대 체력 증가")]
    [PropertyRange(0, 500)]
    public int maxHpBonus;

    [ShowIf("equipmentType", EquipmentType.Ring)]
    [LabelText("체력 회복력 증가")]
    [PropertyRange(0f, 10f)]
    public float hpRegenBonus;

    [Title("추가 정보")]
    [TextArea(3, 5)]
    [LabelText("설명")]
    public string description;

    [LabelText("구매 가격")]
    [PropertyRange(0, 10000)]
    public int buyPrice;

    [LabelText("판매 가격")]
    [PropertyRange(0, 5000)]
    public int sellPrice;

    [Button("스탯 미리보기", ButtonSizes.Large)]
    private void PreviewStats()
    {
        switch (equipmentType)
        {
            case EquipmentType.Weapon:
                Debug.Log($"<color=red>{equipmentName}: 공격력 +{attackPowerBonus}</color>");
                break;
            case EquipmentType.Armor:
                Debug.Log($"<color=green>{equipmentName}: 최대 체력 +{maxHpBonus}</color>");
                break;
            case EquipmentType.Ring:
                Debug.Log($"<color=cyan>{equipmentName}: 체력 회복력 +{hpRegenBonus}/초</color>");
                break;
        }
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