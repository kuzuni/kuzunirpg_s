using UnityEngine;
using System;
using Sirenix.OdinInspector;
// 확장된 플레이어 스테이터스 (기존 PlayerStatus 부분 확장)
namespace RPG.Player
{
    public partial class PlayerStatus
    {
        [Title("레벨 & 경험치", Bold = true)]
        [HorizontalGroup("Level", 0.5f)]
        [VerticalGroup("Level/Current")]
        [LabelText("현재 레벨")]
        [PropertyRange(1, 999)]
        [OnValueChanged("OnLevelChanged")]
        [SerializeField] private int level = 1;

        [VerticalGroup("Level/Current")]
        [LabelText("현재 경험치")]
        [ProgressBar(0, "@GetMaxExp()", ColorGetter = "GetExpBarColor")]
        [SerializeField] private int currentExp = 0;

        [VerticalGroup("Level/Info")]
        [ShowInInspector, ReadOnly]
        [LabelText("필요 경험치")]
        private int RequiredExp => GetMaxExp() - currentExp;

        [VerticalGroup("Level/Info")]
        [ShowInInspector, ReadOnly]
        [LabelText("총 경험치")]
        private int totalExp = 0;

        // 프로퍼티
        public int Level
        {
            get => level;
            private set => level = Mathf.Max(1, value);
        }

        public int CurrentExp
        {
            get => currentExp;
            private set => currentExp = value;
        }

        // 이벤트
        public event Action<int> OnLevelUp;
        public event Action<int, int> OnExpChanged;

        // 경험치 관련 메서드
        public void AddExperience(int amount)
        {
            if (amount <= 0) return;

            totalExp += amount;
            currentExp += amount;

            // 레벨업 체크
            while (currentExp >= GetMaxExp() && level < 999)
            {
                currentExp -= GetMaxExp();
                LevelUp();
            }

            // 최대 레벨인 경우 경험치 초과분 제거
            if (level >= 999)
            {
                currentExp = 0;
            }

            OnExpChanged?.Invoke(currentExp, GetMaxExp());
        }

        private void LevelUp()
        {
            level++;

            // 레벨업 시 스탯 증가
            maxHp += 10 + level;
            currentHp = maxHp; // 체력 완전 회복
            attackPower += 2 + (level / 10);

            OnLevelUp?.Invoke(level);
            Debug.Log($"<color=yellow>레벨 업! Lv.{level}</color>");
        }

        public int GetMaxExp()
        {
            // 레벨에 따른 필요 경험치 공식
            return 100 * level + (level * level * 10);
        }

        public float GetExpProgress()
        {
            int maxExp = GetMaxExp();
            return maxExp > 0 ? (float)currentExp / maxExp : 0f;
        }

        private void OnLevelChanged()
        {
            OnLevelUp?.Invoke(level);
        }

        private Color GetExpBarColor(float value)
        {
            return new Color(0.2f, 0.8f, 1f); // 하늘색
        }

        // 전투력 계산
        [Title("전투력", Bold = true)]
        [ShowInInspector, ReadOnly]
        [ProgressBar(0, 999999, 0.8f, 0.2f, 0.2f)]
        [LabelText("총 전투력")]
        public int CombatPower
        {
            get
            {
                int basePower = 0;
                basePower += MaxHp * 1;
                basePower += AttackPower * 10;
                basePower += (int)(CritChance * 1000);
                basePower += (int)(CritDamage * 100);
                basePower += (int)(AttackSpeed * 50);
                basePower += (int)(HpRegen * 20);
                basePower += Level * 100;

                return basePower;
            }
        }
    }
}