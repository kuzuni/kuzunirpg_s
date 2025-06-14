using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Sirenix.OdinInspector;
using RPG.Core.Events;
using DG.Tweening;

namespace RPG.UI.Player
{
    public class PlayerInfoUI : MonoBehaviour
    {
        [Title("UI 참조")]
        [BoxGroup("Name & Level")]
        [SerializeField, Required]
        private TextMeshProUGUI playerNameText;

        [BoxGroup("Name & Level")]
        [SerializeField, Required]
        private TextMeshProUGUI levelText;

        [BoxGroup("Experience")]
        [SerializeField, Required]
        private Slider expSlider;

        [BoxGroup("Experience")]
        [SerializeField, Required]
        private TextMeshProUGUI expPercentText;

        [BoxGroup("Experience")]
        [SerializeField]
        private TextMeshProUGUI expAmountText; // 현재/최대 경험치 표시 (선택)

        [Title("텍스트 포맷")]
        [BoxGroup("Format")]
        [SerializeField]
        private string nameFormat = "{0}";

        [BoxGroup("Format")]
        [SerializeField]
        private string levelFormat = "Lv.{0}";

        [BoxGroup("Format")]
        [SerializeField]
        private string expPercentFormat = "{0:F1}%";

        [BoxGroup("Format")]
        [SerializeField]
        private string expAmountFormat = "{0}/{1}";

        [Title("설정")]
        [SerializeField]
        private string defaultPlayerName = "플레이어";

        [SerializeField]
        private bool animateExpBar = true;

        [SerializeField, ShowIf("animateExpBar")]
        private float expAnimationDuration = 0.5f;

        [Title("디버그")]
        [ShowInInspector, ReadOnly]
        private int currentLevel = 1;

        [ShowInInspector, ReadOnly]
        private int currentExp = 0;

        [ShowInInspector, ReadOnly]
        private int maxExp = 100;

        [ShowInInspector, ReadOnly]
        private float expProgress = 0f;

        private void Awake()
        {
            // 초기 설정
            if (expSlider != null)
            {
                expSlider.minValue = 0;
                expSlider.maxValue = 1;
                expSlider.value = 0;
            }

            // 초기 플레이어 이름 설정
            UpdateNameDisplay(defaultPlayerName);
        }

        private void Start()
        {
            // 초기값 설정 (이벤트가 발생하기 전까지 기본값 표시)
            UpdateLevelDisplay(1);
            UpdateExpDisplay(0, GetExpForLevel(1));
        }

        private void OnEnable()
        {
            // 이벤트 구독
            GameEventManager.OnPlayerLevelUp += OnPlayerLevelUp;
            GameEventManager.OnPlayerExpGained += OnPlayerExpGained;
        }

        private void OnDisable()
        {
            // 이벤트 구독 해제
            GameEventManager.OnPlayerLevelUp -= OnPlayerLevelUp;
            GameEventManager.OnPlayerExpGained -= OnPlayerExpGained;

            // 애니메이션 정리
            if (animateExpBar && expSlider != null)
            {
                expSlider.DOKill();
            }
        }

        private void OnPlayerLevelUp(int newLevel)
        {
            currentLevel = newLevel;
            currentExp = 0; // 레벨업 시 경험치는 0으로 리셋
            maxExp = GetExpForLevel(newLevel);

            UpdateLevelDisplay(newLevel);
            UpdateExpDisplay(0, maxExp);
        }

        private void OnPlayerExpGained(int amount)
        {
            // 경험치 증가 처리
            currentExp += amount;

            // 최대 경험치를 넘었을 경우 (레벨업 직전)
            if (currentExp >= maxExp)
            {
                currentExp = maxExp - 1; // 레벨업 이벤트를 기다림
            }

            UpdateExpDisplay(currentExp, maxExp);
        }

        private void UpdateNameDisplay(string playerName)
        {
            if (playerNameText == null) return;

            playerNameText.text = string.Format(nameFormat, playerName);
        }

        private void UpdateLevelDisplay(int level)
        {
            if (levelText == null) return;

            levelText.text = string.Format(levelFormat, level);
        }

        private void UpdateExpDisplay(int current, int max)
        {
            if (max <= 0) return;

            float newProgress = (float)current / max;
            expProgress = newProgress;

            // 슬라이더 업데이트
            if (expSlider != null)
            {
                if (animateExpBar && Application.isPlaying)
                {
                    expSlider.DOValue(newProgress, expAnimationDuration)
                        .SetEase(Ease.OutQuad);
                }
                else
                {
                    expSlider.value = newProgress;
                }
            }

            // 퍼센트 텍스트 업데이트
            if (expPercentText != null)
            {
                float percentage = newProgress * 100f;
                expPercentText.text = string.Format(expPercentFormat, percentage);
            }

            // 경험치 수치 텍스트 업데이트 (옵션)
            if (expAmountText != null)
            {
                expAmountText.text = string.Format(expAmountFormat, current, max);
            }
        }

        // 레벨별 필요 경험치 계산 (PlayerStatus와 동일한 공식 사용)
        private int GetExpForLevel(int level)
        {
            return 100 * level + (level * level * 10);
        }

        [Title("공개 메서드")]
        [Button("플레이어 이름 변경")]
        public void SetPlayerName(string newName)
        {
            if (string.IsNullOrEmpty(newName))
                newName = defaultPlayerName;

            UpdateNameDisplay(newName);
        }

        [Title("테스트")]
        [Button("테스트: 경험치 25% 추가")]
        private void TestAddExp()
        {
            int testExp = maxExp / 4;
            OnPlayerExpGained(testExp);
            Debug.Log($"테스트: {testExp} 경험치 추가 (UI만 업데이트)");
        }

        [Button("테스트: 레벨업")]
        private void TestLevelUp()
        {
            OnPlayerLevelUp(currentLevel + 1);
            Debug.Log($"테스트: 레벨 {currentLevel}로 상승 (UI만 업데이트)");
        }

        [Button("테스트: 초기화")]
        private void TestReset()
        {
            currentLevel = 1;
            currentExp = 0;
            maxExp = GetExpForLevel(1);

            UpdateLevelDisplay(1);
            UpdateExpDisplay(0, maxExp);
            UpdateNameDisplay(defaultPlayerName);

            Debug.Log("테스트: UI 초기화 완료");
        }
    }
}