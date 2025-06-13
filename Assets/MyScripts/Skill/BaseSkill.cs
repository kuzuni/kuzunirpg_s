using UnityEngine;
using Sirenix.OdinInspector;
using RPG.Player;

namespace RPG.Skills
{
    // 스킬 시스템 기본 구조
    public abstract class BaseSkill : ScriptableObject
    {
        [Title("스킬 정보")]
        public string skillName;
        public Sprite skillIcon;
        public float cooldown;
        public int unlockLevel;

        [TextArea(3, 5)]
        public string description;

        public abstract void Execute(PlayerController player);
        public abstract float GetDamageMultiplier(int skillLevel);
    }

}
