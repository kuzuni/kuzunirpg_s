using UnityEngine;
using System;
using Sirenix.OdinInspector;
using RPG.Core.Events;


namespace RPG.Player
{
    [Serializable]
    [InlineProperty]
    public partial class PlayerStatus
    {
        [Title("체력 (Health)", Bold = true)]
        [HorizontalGroup("Health", 0.5f)]
        [VerticalGroup("Health/Stats")]
        [LabelText("최대 체력")]
        [PropertyRange(1, 9999)]
        [SuffixLabel("HP", true)]
        [OnValueChanged("OnMaxHpChanged")]
        [SerializeField] private int maxHp = 100;

        [VerticalGroup("Health/Stats")]
        [LabelText("현재 체력")]
        [ProgressBar(0, "@maxHp", ColorGetter = "GetHealthBarColor", Height = 25)]
        [SuffixLabel("HP", true)]
        [SerializeField] private int currentHp = 100;

        [VerticalGroup("Health/Visual")]
        [ShowInInspector, HideLabel]
        [DisplayAsString]
        private string HealthText => $"{currentHp} / {maxHp} ({HealthPercentage:P0})";

        [Title("공격 스탯 (Attack Stats)", Bold = true)]
        [TabGroup("Combat", "Attack")]
        [LabelText("공격력")]
        [PropertyRange(0, 999)]
        [SuffixLabel("DMG", true)]
        [GUIColor(1f, 0.7f, 0.7f)]
        [SerializeField] private int attackPower = 10;

        [TabGroup("Combat", "Attack")]
        [LabelText("치명타 확률")]
        [PropertyRange(0f, 1f)]
        [CustomValueDrawer("DrawCritChanceBar")]
        [SerializeField] private float critChance = 0.1f;

        [TabGroup("Combat", "Attack")]
        [LabelText("치명타 배율")]
        [PropertyRange(1f, 5f)]
        [SuffixLabel("x", true)]
        [GUIColor(1f, 1f, 0.7f)]
        [SerializeField] private float critDamage = 1.5f;

        [TabGroup("Combat", "Attack")]
        [LabelText("공격 속도")]
        [PropertyRange(0.1f, 5f)]
        [SuffixLabel("회/초", true)]
        [GUIColor(0.7f, 1f, 0.7f)]
        [SerializeField] private float attackSpeed = 1.0f;

        [TabGroup("Combat", "Attack")]
        [ShowInInspector, ReadOnly]
        [LabelText("예상 DPS")]
        [GUIColor(1f, 0.5f, 0.5f)]
        private float EstimatedDPS => attackPower * attackSpeed * (1 + (critChance * (critDamage - 1)));

        [Title("회복 스탯 (Recovery)", Bold = true)]
        [TabGroup("Combat", "Recovery")]
        [LabelText("체력 재생")]
        [PropertyRange(0f, 50f)]
        [SuffixLabel("HP/초", true)]
        [GUIColor(0.7f, 1f, 0.9f)]
        [SerializeField] private float hpRegen = 1.0f;

        [TabGroup("Combat", "Recovery")]
        [ShowInInspector, ReadOnly]
        [LabelText("완전 회복 시간")]
        [SuffixLabel("초", true)]
        private float FullRecoveryTime => hpRegen > 0 ? (maxHp - currentHp) / hpRegen : float.PositiveInfinity;

        [Title("스탯 요약 (Summary)", Bold = true)]
        [PropertySpace(10)]
        [InfoBox("플레이어의 전체 스탯 요약입니다.", InfoMessageType.None)]
        [ShowInInspector, ReadOnly]
        [ListDrawerSettings(ShowFoldout = false, ShowPaging = false, Expanded = true)]
        private string[] StatSummary => new string[]
        {
        $"체력: {currentHp}/{maxHp} ({HealthPercentage:P0})",
        $"공격력: {attackPower} (DPS: {EstimatedDPS:F1})",
        $"치명타: {critChance:P0} 확률로 {critDamage:F1}배 데미지",
        $"공격속도: {attackSpeed:F1}회/초",
        $"체력재생: {hpRegen:F1} HP/초"
        };

        // Properties
        public int MaxHp
        {
            get => maxHp;
            set => maxHp = Mathf.Max(1, value);
        }

        public int CurrentHp
        {
            get => currentHp;
            set
            {
                currentHp = Mathf.Clamp(value, 0, maxHp);
                GameEventManager.TriggerPlayerHealthChanged(currentHp, maxHp);

                if (currentHp <= 0)
                {
                    GameEventManager.TriggerPlayerDeath();
                }
            }
        }


        public int AttackPower
        {
            get => attackPower;
            set => attackPower = Mathf.Max(0, value);
        }

        public float CritChance
        {
            get => critChance;
            set => critChance = Mathf.Clamp01(value);
        }

        public float CritDamage
        {
            get => critDamage;
            set => critDamage = Mathf.Max(1f, value);
        }

        public float AttackSpeed
        {
            get => attackSpeed;
            set => attackSpeed = Mathf.Max(0.1f, value);
        }

        public float HpRegen
        {
            get => hpRegen;
            set => hpRegen = Mathf.Max(0f, value);
        }

        // 생성자
        public PlayerStatus()
        {
            currentHp = maxHp;
        }

        public PlayerStatus(int maxHp, int attackPower, float critChance,
                           float critDamage, float attackSpeed, float hpRegen)
        {
            this.maxHp = maxHp;
            this.currentHp = maxHp;
            this.attackPower = attackPower;
            this.critChance = critChance;
            this.critDamage = critDamage;
            this.attackSpeed = attackSpeed;
            this.hpRegen = hpRegen;
        }

        // Odin Inspector Helper Methods
        private float HealthPercentage => maxHp > 0 ? (float)currentHp / maxHp : 0f;

        private Color GetHealthBarColor()
        {
            float healthPercent = HealthPercentage;
            if (healthPercent > 0.6f) return Color.green;
            if (healthPercent > 0.3f) return Color.yellow;
            return Color.red;
        }

        private float DrawCritChanceBar()
        {
            return critChance;
        }

        private void OnMaxHpChanged()
        {
            if (currentHp > maxHp)
                currentHp = maxHp;
        }

        // Test Buttons
        [Title("테스트 기능", Bold = true)]
        [ButtonGroup("TestButtons")]
        [Button("체력 50% 감소", ButtonSizes.Medium), GUIColor(1f, 0.5f, 0.5f)]
        private void TestDamage()
        {
            CurrentHp = Mathf.RoundToInt(maxHp * 0.5f);
            Debug.Log($"테스트: 체력이 {currentHp}로 감소했습니다.");
        }

        [ButtonGroup("TestButtons")]
        [Button("체력 완전 회복", ButtonSizes.Medium), GUIColor(0.5f, 1f, 0.5f)]
        private void TestFullHeal()
        {
            CurrentHp = MaxHp;
            Debug.Log("테스트: 체력이 완전 회복되었습니다.");
        }

        [ButtonGroup("TestButtons")]
        [Button("랜덤 스탯 설정", ButtonSizes.Medium), GUIColor(0.5f, 0.5f, 1f)]
        private void TestRandomStats()
        {
            MaxHp = UnityEngine.Random.Range(50, 200);
            CurrentHp = UnityEngine.Random.Range(1, MaxHp);
            AttackPower = UnityEngine.Random.Range(5, 30);
            CritChance = UnityEngine.Random.Range(0.05f, 0.5f);
            CritDamage = UnityEngine.Random.Range(1.5f, 3f);
            AttackSpeed = UnityEngine.Random.Range(0.5f, 3f);
            HpRegen = UnityEngine.Random.Range(0.5f, 5f);
            Debug.Log("테스트: 랜덤 스탯이 설정되었습니다.");
        }
    }
}