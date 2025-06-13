
// ===== 분리된 시스템들 =====

namespace RPG.Combat.Interfaces
{
    // 데미지 처리 시스템
    public interface IDamageable
    {
        void TakeDamage(int damage);
        bool IsDead { get; }
    }

}
