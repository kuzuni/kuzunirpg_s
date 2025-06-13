// HealthSystem.cs
using UnityEngine;
using System;
using RPG.Combat.Interfaces;
using RPG.Core.Events;

namespace RPG.Player
{
    public class HealthSystem : MonoBehaviour, IDamageable
    {
        [SerializeField] private PlayerStatus playerStatus;

        // 로컬 이벤트 제거 - GameEventManager 사용

        public bool IsDead => playerStatus.CurrentHp <= 0;

        public void TakeDamage(int damage)
        {
            int previousHp = playerStatus.CurrentHp;
            playerStatus.CurrentHp -= damage;

            // 이벤트 발생 (기존 OnDamaged 대체)
            GameEventManager.TriggerDamageTaken(damage);

            // 체력 변경 이벤트
            GameEventManager.TriggerPlayerHealthChanged(playerStatus.CurrentHp, playerStatus.MaxHp);

            if (IsDead)
            {
                // 이벤트 발생 (기존 OnDeath 대체)
                GameEventManager.TriggerPlayerDeath();
            }
        }

        public void Heal(int amount)
        {
            int previousHp = playerStatus.CurrentHp;
            playerStatus.CurrentHp += amount;
            int actualHealed = playerStatus.CurrentHp - previousHp;

            if (actualHealed > 0)
            {
                // 이벤트 발생 (기존 OnHealed 대체)
                GameEventManager.TriggerPlayerHealed(actualHealed);

                // 체력 변경 이벤트
                GameEventManager.TriggerPlayerHealthChanged(playerStatus.CurrentHp, playerStatus.MaxHp);
            }
        }
    }
}