using UnityEngine;
// 플레이어 매니저 (통합 관리)
public class PlayerManager : MonoBehaviour
{
    [SerializeField] private PlayerStatus playerStatus = new PlayerStatus();

    private HealthSystem healthSystem;
    private HealthRegenerationSystem regenSystem;
    private AttackSystem attackSystem;

    public PlayerStatus Status => playerStatus;
    public HealthSystem Health => healthSystem;
    public AttackSystem Attack => attackSystem;

    void Awake()
    {
        // 컴포넌트 초기화
        healthSystem = gameObject.AddComponent<HealthSystem>();
        regenSystem = gameObject.AddComponent<HealthRegenerationSystem>();
        attackSystem = gameObject.AddComponent<AttackSystem>();

        // PlayerStatus 주입
        var statusField = typeof(HealthSystem).GetField("playerStatus",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        statusField.SetValue(healthSystem, playerStatus);

        statusField = typeof(HealthRegenerationSystem).GetField("playerStatus",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        statusField.SetValue(regenSystem, playerStatus);

        var healthField = typeof(HealthRegenerationSystem).GetField("healthSystem",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        healthField.SetValue(regenSystem, healthSystem);

        statusField = typeof(AttackSystem).GetField("playerStatus",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        statusField.SetValue(attackSystem, playerStatus);
    }
}