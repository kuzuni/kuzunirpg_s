using UnityEngine;
using System;

[Serializable]
public class PlayerStatus
{
    [Header("체력 관련")]
    [SerializeField] private int maxHp = 100;
    [SerializeField] private int currentHp = 100;

    [Header("공격 관련")]
    [SerializeField] private int attackPower = 10;
    [SerializeField][Range(0f, 1f)] private float critChance = 0.1f;
    [SerializeField] private float critDamage = 1.5f;
    [SerializeField] private float attackSpeed = 1.0f;

    [Header("회복 관련")]
    [SerializeField] private float hpRegen = 1.0f;

    // Properties
    public int MaxHp
    {
        get => maxHp;
        set => maxHp = Mathf.Max(1, value);
    }

    public int CurrentHp
    {
        get => currentHp;
        set => currentHp = Mathf.Clamp(value, 0, maxHp);
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
}
