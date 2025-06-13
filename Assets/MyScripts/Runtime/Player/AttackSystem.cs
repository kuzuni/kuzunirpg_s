// AttackSystem.cs
using UnityEngine;
using System;
using RPG.Combat.Interfaces;
using RPG.Core.Events;

namespace RPG.Player
{
    public class AttackSystem : MonoBehaviour, IAttacker
    {
        [SerializeField] private PlayerStatus playerStatus;

        // 로컬 이벤트 제거 - GameEventManager 사용

        public int CalculateDamage()
        {
            bool isCritical = UnityEngine.Random.Range(0f, 1f) < playerStatus.CritChance;
            int damage = isCritical
                ? Mathf.RoundToInt(playerStatus.AttackPower * playerStatus.CritDamage)
                : playerStatus.AttackPower;

            // 이벤트 발생 (기존 OnAttack 대체)
            GameEventManager.TriggerDamageDealt(damage, isCritical);

            return damage;
        }

        public float GetAttackCooldown()
        {
            return 1f / playerStatus.AttackSpeed;
        }
    }
}