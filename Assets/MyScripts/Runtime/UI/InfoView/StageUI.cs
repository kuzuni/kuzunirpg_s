using DG.Tweening;
using RPG.Core.Events;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;

using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Sirenix.OdinInspector;
using RPG.Core.Events;
using DG.Tweening;

namespace RPG.Stage.UI
{
    public class StageUI : MonoBehaviour
    {
        [Title("UI 참조")]
        [SerializeField, Required]
        private TextMeshProUGUI stageText;

        [SerializeField, Required]
        private Slider progressSlider;

        [SerializeField]
        private TextMeshProUGUI progressPercentText;

        [Title("텍스트 포맷")]
        [SerializeField]
        private string stageFormat = "Stage {0} <{1}>";

        [SerializeField]
        private string progressFormat = "{0:F0}%";

        [SerializeField]
        private bool showDecimalProgress = false;

        [Title("퍼센트 텍스트 설정")]
        [SerializeField]
        private bool animatePercentText = true;

        [SerializeField, ShowIf("animatePercentText")]
        private float percentCountDuration = 0.5f;

        [SerializeField]
        private Color progressCompleteColor = Color.green;

        [Title("애니메이션 설정")]
        [SerializeField]
        private bool useAnimation = true;

        [SerializeField, ShowIf("useAnimation")]
        private float progressAnimDuration = 0.5f;

        [SerializeField, ShowIf("useAnimation")]
        private Ease progressEaseType = Ease.OutQuad;

        [SerializeField, ShowIf("useAnimation")]
        private bool showFillEffect = true;

        [Title("완료 효과")]
        [SerializeField]
        private bool pulseOnComplete = true;

        [SerializeField, ShowIf("pulseOnComplete")]
        private float pulseScale = 1.1f;

        [SerializeField, ShowIf("pulseOnComplete")]
        private float pulseDuration = 0.3f;

        [Title("디버그")]
        [ShowInInspector, ReadOnly]
        private StageData currentStageData;

        [ShowInInspector, ReadOnly]
        private float currentProgress;

        [ShowInInspector, ReadOnly]
        private bool isAnimating = false;

        private Tween currentProgressTween;
        private Tween currentPercentTween;
        private Sequence completeSequence;
        private float displayedPercent = 0f;

        private void Awake()
        {
            // 초기 설정
            if (progressSlider != null)
            {
                progressSlider.value = 0;
            }

            // 퍼센트 텍스트 초기화
            UpdatePercentText(0);
        }

        private void OnEnable()
        {
            // 이벤트 구독
            GameEventManager.OnStageStarted += OnStageStarted;
            GameEventManager.OnStageProgress += OnStageProgress;
        }

        private void OnDisable()
        {
            // 이벤트 구독 해제
            GameEventManager.OnStageStarted -= OnStageStarted;
            GameEventManager.OnStageProgress -= OnStageProgress;

            // 진행 중인 애니메이션 정리
            KillAllTweens();
        }

        private void OnDestroy()
        {
            KillAllTweens();
        }

        private void KillAllTweens()
        {
            currentProgressTween?.Kill();
            currentPercentTween?.Kill();
            completeSequence?.Kill();

            if (progressSlider != null)
            {
                progressSlider.transform.DOKill();
            }

            if (progressPercentText != null)
            {
                progressPercentText.transform.DOKill();
            }
        }

        private void OnStageStarted(StageData stageData)
        {
            currentStageData = stageData;
            UpdateStageDisplay();

            // 새 스테이지 시작 시 프로그레스 바 리셋
            ResetProgress();
        }

        private void OnStageProgress(float progress)
        {
            UpdateProgress(progress);
        }
        private void UpdatePercentText(float percent)
        {
            if (progressPercentText == null) return;

            string format = showDecimalProgress ? "{0:F1}%" : progressFormat;
            progressPercentText.text = string.Format(format, percent);

            // 특정 구간별 색상 변화
            if (percent >= 100)
            {
                progressPercentText.color = progressCompleteColor;
            }
            else if (percent >= 75)
            {
                progressPercentText.color = Color.yellow;
            }
            else if (percent >= 50)
            {
                progressPercentText.color = new Color(1f, 0.65f, 0f); // 주황색
            }
            else
            {
                progressPercentText.color = Color.white;
            }
        }
        private void UpdateStageDisplay()
        {
            if (stageText == null || currentStageData == null) return;

            // Stage 1 <숲의 입구 1> 형식으로 표시
            stageText.text = string.Format(stageFormat,
                currentStageData.stageNumber,
                currentStageData.stageName);

            // 스테이지 텍스트 등장 애니메이션
            if (useAnimation && stageText != null)
            {
                stageText.transform.localScale = Vector3.one * 0.8f;
                stageText.transform.DOScale(1f, 0.3f).SetEase(Ease.OutBack);

                // 페이드 인 효과
                var canvasGroup = stageText.GetComponent<CanvasGroup>();
                if (canvasGroup == null)
                {
                    canvasGroup = stageText.gameObject.AddComponent<CanvasGroup>();
                }
                canvasGroup.alpha = 0;
                canvasGroup.DOFade(1f, 0.3f);
            }
        }

        private void ResetProgress()
        {
            // 진행 중인 애니메이션 중지
            currentProgressTween?.Kill();
            currentPercentTween?.Kill();

            currentProgress = 0;
            displayedPercent = 0;

            if (progressSlider == null) return;

            if (useAnimation)
            {
                // 리셋 애니메이션
                progressSlider.DOValue(0, 0.3f).SetEase(Ease.OutQuad);

                // 퍼센트 텍스트도 애니메이션
                if (animatePercentText && progressPercentText != null)
                {
                    DOTween.To(() => displayedPercent, x =>
                    {
                        displayedPercent = x;
                        UpdatePercentText(x);
                    }, 0, 0.3f);
                }
            }
            else
            {
                progressSlider.value = 0;
                UpdatePercentText(0);
            }

            // 색상 초기화
            if (progressPercentText != null)
            {
                progressPercentText.color = Color.white;
            }
        }

        private void UpdateProgress(float progress)
        {
            float previousProgress = currentProgress;
            currentProgress = Mathf.Clamp01(progress);
            float targetPercent = currentProgress * 100f;

            if (progressSlider == null) return;

            if (useAnimation && Application.isPlaying)
            {
                isAnimating = true;

                // 기존 애니메이션 중지
                currentProgressTween?.Kill();
                currentPercentTween?.Kill();

                // 프로그레스 바 애니메이션
                currentProgressTween = progressSlider.DOValue(currentProgress, progressAnimDuration)
                    .SetEase(progressEaseType)
                    .OnComplete(() =>
                    {
                        isAnimating = false;

                        // 100% 완료 시 효과
                        if (currentProgress >= 1f && previousProgress < 1f)
                        {
                            OnStageComplete();
                        }
                    });

                // 퍼센트 텍스트 애니메이션
                if (animatePercentText && progressPercentText != null)
                {
                    currentPercentTween = DOTween.To(() => displayedPercent, x =>
                    {
                        displayedPercent = x;
                        UpdatePercentText(x);
                    }, targetPercent, percentCountDuration)
                    .SetEase(Ease.Linear);
                }
                else
                {
                    UpdatePercentText(targetPercent);
                }

                // 진행도가 증가할 때 추가 효과
                if (showFillEffect && progress > previousProgress)
                {
                    ShowProgressEffect();
                }
            }
            else
            {
                progressSlider.value = currentProgress;
                UpdatePercentText(targetPercent);
            }
        }

        private void ShowProgressEffect()
        {
            if (progressSlider == null) return;

            // Fill 이미지에 간단한 플래시 효과
            var fillImage = progressSlider.fillRect?.GetComponent<Image>();
            if (fillImage != null)
            {
                var originalColor = fillImage.color;
                var flashColor = new Color(
                    Mathf.Min(originalColor.r + 0.3f, 1f),
                    Mathf.Min(originalColor.g + 0.3f, 1f),
                    Mathf.Min(originalColor.b + 0.3f, 1f),
                    originalColor.a
                );

                fillImage.DOColor(flashColor, 0.1f)
                    .OnComplete(() => fillImage.DOColor(originalColor, 0.2f));
            }
        }

        private void OnStageComplete()
        {
            if (!pulseOnComplete || progressSlider == null) return;

            // 완료 시퀀스
            completeSequence?.Kill();
            completeSequence = DOTween.Sequence();

            // 슬라이더 전체에 펄스 효과
            completeSequence.Append(
                progressSlider.transform.DOScale(pulseScale, pulseDuration * 0.5f)
                    .SetEase(Ease.OutQuad)
            );
            completeSequence.Append(
                progressSlider.transform.DOScale(1f, pulseDuration * 0.5f)
                    .SetEase(Ease.InQuad)
            );

            // 퍼센트 텍스트 펄스 효과
            if (progressPercentText != null)
            {
                completeSequence.Join(
                    progressPercentText.transform.DOScale(pulseScale * 1.2f, pulseDuration * 0.5f)
                        .SetEase(Ease.OutQuad)
                );
                completeSequence.Append(
                    progressPercentText.transform.DOScale(1f, pulseDuration * 0.5f)
                        .SetEase(Ease.InQuad)
                );

                // 완료 텍스트 효과
                completeSequence.Join(
                    progressPercentText.DOColor(progressCompleteColor, pulseDuration)
                );
            }

            // Fill 색상 변화
            var fillImage = progressSlider.fillRect?.GetComponent<Image>();
            if (fillImage != null)
            {
                var originalColor = fillImage.color;
                var completeColor = Color.green;

                completeSequence.Join(
                    fillImage.DOColor(completeColor, pulseDuration)
                        .OnComplete(() => fillImage.color = originalColor)
                );
            }

            Debug.Log("<color=green>스테이지 진행도 100% 달성!</color>");
        }

        [Title("테스트")]
        [Button("테스트 스테이지 표시")]
        private void TestDisplay()
        {
            if (stageText == null)
            {
                Debug.LogError("Stage Text가 할당되지 않았습니다!");
                return;
            }

            var testStage = new StageData
            {
                stageNumber = 1,
                stageName = "숲의 입구 1"
            };

            currentStageData = testStage;
            UpdateStageDisplay();
            UpdateProgress(0.5f);
        }

        [Button("프로그레스 애니메이션 테스트")]
        [ButtonGroup("AnimTest")]
        private void TestProgressAnimation()
        {
            if (!Application.isPlaying)
            {
                Debug.LogWarning("Play Mode에서만 테스트 가능합니다!");
                return;
            }

            // 0에서 100까지 단계적으로 증가
            DOTween.Sequence()
                .AppendCallback(() => UpdateProgress(0.25f))
                .AppendInterval(1f)
                .AppendCallback(() => UpdateProgress(0.5f))
                .AppendInterval(1f)
                .AppendCallback(() => UpdateProgress(0.75f))
                .AppendInterval(1f)
                .AppendCallback(() => UpdateProgress(1f));
        }

        [Button("퍼센트 텍스트 테스트")]
        [ButtonGroup("AnimTest")]
        private void TestPercentText()
        {
            if (progressPercentText == null)
            {
                Debug.LogError("Progress Percent Text가 할당되지 않았습니다!");
                return;
            }

            // 0%에서 100%까지 빠르게 카운트
            if (Application.isPlaying)
            {
                DOTween.To(() => 0f, x => UpdatePercentText(x), 100f, 2f)
                    .SetEase(Ease.Linear);
            }
            else
            {
                UpdatePercentText(75f);
            }
        }

        [Button("즉시 완료")]
        [ButtonGroup("AnimTest")]
        private void TestInstantComplete()
        {
            UpdateProgress(1f);
        }

        [Button("리셋")]
        [ButtonGroup("AnimTest")]
        private void TestReset()
        {
            ResetProgress();
        }

        [Title("애니메이션 프리셋")]
        [Button("부드러운 애니메이션")]
        [ButtonGroup("Presets")]
        private void SetSmoothAnimation()
        {
            progressAnimDuration = 0.8f;
            progressEaseType = Ease.InOutQuad;
            Debug.Log("부드러운 애니메이션 설정 적용");
        }

        [Button("빠른 애니메이션")]
        [ButtonGroup("Presets")]
        private void SetFastAnimation()
        {
            progressAnimDuration = 0.3f;
            progressEaseType = Ease.OutExpo;
            Debug.Log("빠른 애니메이션 설정 적용");
        }

        [Button("탄성 애니메이션")]
        [ButtonGroup("Presets")]
        private void SetBouncyAnimation()
        {
            progressAnimDuration = 0.6f;
            progressEaseType = Ease.OutElastic;
            Debug.Log("탄성 애니메이션 설정 적용");
        }
    }
}