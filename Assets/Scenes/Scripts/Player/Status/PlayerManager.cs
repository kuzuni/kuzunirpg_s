// 플레이어 매니저 (통합 관리)
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    [SerializeField] private PlayerStatus playerStatus = new PlayerStatus();

    private HealthSystem healthSystem;
    private HealthRegenerationSystem regenSystem;
    private AttackSystem attackSystem;
    private EnhancementSystem enhancementSystem;

    public PlayerStatus Status => playerStatus;
    public HealthSystem Health => healthSystem;
    public AttackSystem Attack => attackSystem;
    public EnhancementSystem Enhancement => enhancementSystem;

    void Awake()
    {
        // 컴포넌트 초기화
        healthSystem = gameObject.AddComponent<HealthSystem>();
        regenSystem = gameObject.AddComponent<HealthRegenerationSystem>();
        attackSystem = gameObject.AddComponent<AttackSystem>();
        enhancementSystem = gameObject.AddComponent<EnhancementSystem>();

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

        // EnhancementSystem은 Initialize 메서드로 초기화
        enhancementSystem.Initialize(playerStatus);
    }

    // 디버그 메서드
    public void DebugPlayerStatus()
    {
        Debug.Log("========== Player Status ==========");
        Debug.Log($"체력: {playerStatus.CurrentHp} / {playerStatus.MaxHp}");
        Debug.Log($"공격력: {playerStatus.AttackPower}");
        Debug.Log($"치명타 확률: {playerStatus.CritChance * 100:F1}%");
        Debug.Log($"치명타 데미지: {playerStatus.CritDamage * 100:F0}%");
        Debug.Log($"공격 속도: {playerStatus.AttackSpeed:F2}");
        Debug.Log($"체력 회복력: {playerStatus.HpRegen:F1}/초");
        Debug.Log("===================================");
    }

    // Inspector에서 테스트용 버튼
    [ContextMenu("Show Player Status")]
    void ShowStatusInInspector()
    {
        DebugPlayerStatus();
    }

    [ContextMenu("Show Enhancement Info")]
    void ShowEnhancementInfo()
    {
        if (enhancementSystem != null)
            enhancementSystem.DebugEnhancementInfo();
    }

    // 키보드 입력으로 디버그 (옵션)
    void Update()
    {
        // F1 키를 누르면 스탯 표시
        if (Input.GetKeyDown(KeyCode.F1))
        {
            DebugPlayerStatus();
        }

        // F2 키를 누르면 강화 정보 표시
        if (Input.GetKeyDown(KeyCode.F2))
        {
            if (enhancementSystem != null)
                enhancementSystem.DebugEnhancementInfo();
        }
    }
}