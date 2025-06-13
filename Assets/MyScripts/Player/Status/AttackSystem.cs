using UnityEngine;
using System;
using RPG.Combat.Interfaces;

namespace RPG.Player
{
    public class AttackSystem : MonoBehaviour, IAttacker
    {
        [SerializeField] private PlayerStatus playerStatus;

        public event Action<bool, int> OnAttack; // bool: isCritical, int: damage

        public int CalculateDamage()
        {
            bool isCritical = UnityEngine.Random.Range(0f, 1f) < playerStatus.CritChance;
            int damage = isCritical
                ? Mathf.RoundToInt(playerStatus.AttackPower * playerStatus.CritDamage)
                : playerStatus.AttackPower;

            OnAttack?.Invoke(isCritical, damage);
            return damage;
        }

        public float GetAttackCooldown()
        {
            return 1f / playerStatus.AttackSpeed;
        }
    }

}
