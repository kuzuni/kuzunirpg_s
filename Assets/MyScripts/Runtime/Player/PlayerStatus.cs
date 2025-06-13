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
        [Title("ü�� (Health)", Bold = true)]
        [HorizontalGroup("Health", 0.5f)]
        [VerticalGroup("Health/Stats")]
        [LabelText("�ִ� ü��")]
        [PropertyRange(1, 9999)]
        [SuffixLabel("HP", true)]
        [OnValueChanged("OnMaxHpChanged")]
        [SerializeField] private int maxHp = 100;

        [VerticalGroup("Health/Stats")]
        [LabelText("���� ü��")]
        [ProgressBar(0, "@maxHp", ColorGetter = "GetHealthBarColor", Height = 25)]
        [SuffixLabel("HP", true)]
        [SerializeField] private int currentHp = 100;

        [VerticalGroup("Health/Visual")]
        [ShowInInspector, HideLabel]
        [DisplayAsString]
        private string HealthText => $"{currentHp} / {maxHp} ({HealthPercentage:P0})";

        [Title("���� ���� (Attack Stats)", Bold = true)]
        [TabGroup("Combat", "Attack")]
        [LabelText("���ݷ�")]
        [PropertyRange(0, 999)]
        [SuffixLabel("DMG", true)]
        [GUIColor(1f, 0.7f, 0.7f)]
        [SerializeField] private int attackPower = 10;

        [TabGroup("Combat", "Attack")]
        [LabelText("ġ��Ÿ Ȯ��")]
        [PropertyRange(0f, 1f)]
        [CustomValueDrawer("DrawCritChanceBar")]
        [SerializeField] private float critChance = 0.1f;

        [TabGroup("Combat", "Attack")]
        [LabelText("ġ��Ÿ ����")]
        [PropertyRange(1f, 5f)]
        [SuffixLabel("x", true)]
        [GUIColor(1f, 1f, 0.7f)]
        [SerializeField] private float critDamage = 1.5f;

        [TabGroup("Combat", "Attack")]
        [LabelText("���� �ӵ�")]
        [PropertyRange(0.1f, 5f)]
        [SuffixLabel("ȸ/��", true)]
        [GUIColor(0.7f, 1f, 0.7f)]
        [SerializeField] private float attackSpeed = 1.0f;

        [TabGroup("Combat", "Attack")]
        [ShowInInspector, ReadOnly]
        [LabelText("���� DPS")]
        [GUIColor(1f, 0.5f, 0.5f)]
        private float EstimatedDPS => attackPower * attackSpeed * (1 + (critChance * (critDamage - 1)));

        [Title("ȸ�� ���� (Recovery)", Bold = true)]
        [TabGroup("Combat", "Recovery")]
        [LabelText("ü�� ���")]
        [PropertyRange(0f, 50f)]
        [SuffixLabel("HP/��", true)]
        [GUIColor(0.7f, 1f, 0.9f)]
        [SerializeField] private float hpRegen = 1.0f;

        [TabGroup("Combat", "Recovery")]
        [ShowInInspector, ReadOnly]
        [LabelText("���� ȸ�� �ð�")]
        [SuffixLabel("��", true)]
        private float FullRecoveryTime => hpRegen > 0 ? (maxHp - currentHp) / hpRegen : float.PositiveInfinity;

        [Title("���� ��� (Summary)", Bold = true)]
        [PropertySpace(10)]
        [InfoBox("�÷��̾��� ��ü ���� ����Դϴ�.", InfoMessageType.None)]
        [ShowInInspector, ReadOnly]
        [ListDrawerSettings(ShowFoldout = false, ShowPaging = false, Expanded = true)]
        private string[] StatSummary => new string[]
        {
        $"ü��: {currentHp}/{maxHp} ({HealthPercentage:P0})",
        $"���ݷ�: {attackPower} (DPS: {EstimatedDPS:F1})",
        $"ġ��Ÿ: {critChance:P0} Ȯ���� {critDamage:F1}�� ������",
        $"���ݼӵ�: {attackSpeed:F1}ȸ/��",
        $"ü�����: {hpRegen:F1} HP/��"
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

        // ������
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
        [Title("�׽�Ʈ ���", Bold = true)]
        [ButtonGroup("TestButtons")]
        [Button("ü�� 50% ����", ButtonSizes.Medium), GUIColor(1f, 0.5f, 0.5f)]
        private void TestDamage()
        {
            CurrentHp = Mathf.RoundToInt(maxHp * 0.5f);
            Debug.Log($"�׽�Ʈ: ü���� {currentHp}�� �����߽��ϴ�.");
        }

        [ButtonGroup("TestButtons")]
        [Button("ü�� ���� ȸ��", ButtonSizes.Medium), GUIColor(0.5f, 1f, 0.5f)]
        private void TestFullHeal()
        {
            CurrentHp = MaxHp;
            Debug.Log("�׽�Ʈ: ü���� ���� ȸ���Ǿ����ϴ�.");
        }

        [ButtonGroup("TestButtons")]
        [Button("���� ���� ����", ButtonSizes.Medium), GUIColor(0.5f, 0.5f, 1f)]
        private void TestRandomStats()
        {
            MaxHp = UnityEngine.Random.Range(50, 200);
            CurrentHp = UnityEngine.Random.Range(1, MaxHp);
            AttackPower = UnityEngine.Random.Range(5, 30);
            CritChance = UnityEngine.Random.Range(0.05f, 0.5f);
            CritDamage = UnityEngine.Random.Range(1.5f, 3f);
            AttackSpeed = UnityEngine.Random.Range(0.5f, 3f);
            HpRegen = UnityEngine.Random.Range(0.5f, 5f);
            Debug.Log("�׽�Ʈ: ���� ������ �����Ǿ����ϴ�.");
        }
    }
}