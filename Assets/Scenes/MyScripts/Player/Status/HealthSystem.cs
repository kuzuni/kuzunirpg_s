using UnityEngine;
using System;

public class HealthSystem : MonoBehaviour, IDamageable
{
    [SerializeField] private PlayerStatus playerStatus;

    public event Action OnDeath;
    public event Action<int> OnDamaged;
    public event Action<int> OnHealed;

    public bool IsDead => playerStatus.CurrentHp <= 0;

    public void TakeDamage(int damage)
    {
        playerStatus.CurrentHp -= damage;
        OnDamaged?.Invoke(damage);

        if (IsDead)
        {
            OnDeath?.Invoke();
        }
    }

    public void Heal(int amount)
    {
        int previousHp = playerStatus.CurrentHp;
        playerStatus.CurrentHp += amount;
        int actualHealed = playerStatus.CurrentHp - previousHp;

        if (actualHealed > 0)
        {
            OnHealed?.Invoke(actualHealed);
        }
    }
}
