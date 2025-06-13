// 플레이어 매니저 (통합 관리)
using RPG.Enhancement ;
using RPG.Items.Equipment;
using RPG.Items.Relic;
using UnityEngine;
namespace RPG.Player
{

    public class PlayerController : MonoBehaviour
    {
        [SerializeField] private PlayerStatus playerStatus = new PlayerStatus();

        private HealthSystem healthSystem;
        private HealthRegenerationSystem regenSystem;
        private AttackSystem attackSystem;
        private EnhancementSystem enhancementSystem;
        private EquipmentSystem equipmentSystem;
        private RelicEffectSystem relicEffectSystem;

        public PlayerStatus Status => playerStatus;
        public HealthSystem Health => healthSystem;
        public AttackSystem Attack => attackSystem;
        public EnhancementSystem Enhancement => enhancementSystem;
        public EquipmentSystem Equipment => equipmentSystem;
        public RelicEffectSystem RelicEffect => relicEffectSystem;

        void Awake()
        {
            // 컴포넌트 초기화
            healthSystem = gameObject.AddComponent<HealthSystem>();
            regenSystem = gameObject.AddComponent<HealthRegenerationSystem>();
            attackSystem = gameObject.AddComponent<AttackSystem>();
            enhancementSystem = gameObject.AddComponent<EnhancementSystem>();
            equipmentSystem = gameObject.AddComponent<EquipmentSystem>();
            relicEffectSystem = gameObject.AddComponent<RelicEffectSystem>();

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

            // EquipmentSystem은 Initialize 메서드로 초기화
            equipmentSystem.Initialize(playerStatus);

            // RelicEffectSystem은 필드 주입 후 Initialize
            statusField = typeof(RelicEffectSystem).GetField("playerStatus",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            statusField.SetValue(relicEffectSystem, playerStatus);
        }

        void Start()
        {
            // RelicEffectSystem은 RelicInventorySystem이 준비된 후 초기화
            if (relicEffectSystem != null)
            {
                relicEffectSystem.Initialize();
            }
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

        [ContextMenu("Show Equipment Status")]
        void ShowEquipmentStatus()
        {
            if (equipmentSystem != null)
                equipmentSystem.DebugEquipmentStatus();
        }

        [ContextMenu("Show Relic Effects")]
        void ShowRelicEffects()
        {
            if (relicEffectSystem != null)
                relicEffectSystem.ShowRelicEffectSummary();
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

            // F3 키를 누르면 장비 정보 표시
            if (Input.GetKeyDown(KeyCode.F3))
            {
                if (equipmentSystem != null)
                    equipmentSystem.DebugEquipmentStatus();
            }

            // F4 키를 누르면 유물 효과 표시
            if (Input.GetKeyDown(KeyCode.F4))
            {
                if (relicEffectSystem != null)
                    relicEffectSystem.ShowRelicEffectSummary();
            }
        }
    }
}