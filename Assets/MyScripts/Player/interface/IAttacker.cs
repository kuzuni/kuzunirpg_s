
namespace RPG.Combat.Interfaces
{
    // 공격 시스템
    public interface IAttacker
    {
        int CalculateDamage();
        float GetAttackCooldown();
    }

}
