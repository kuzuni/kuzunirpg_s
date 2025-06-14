using UnityEngine;
using UnityEngine.UI;
using System;
using DG.Tweening;
using Sirenix.OdinInspector;

public class PopupUI : MonoBehaviour
{
    [Title("Dim 설정")]
    [EnumToggleButtons]
    public DimType dimType = DimType.Required;

    [ShowIf("@dimType != DimType.None")]
    [Range(0f, 1f)]
    public float dimAlpha = 0.7f;

    [ShowIf("@dimType != DimType.None")]
    [ColorUsage(true, false)]
    public Color dimColor = Color.black;

    [Title("애니메이션 설정")]
    [EnumToggleButtons]
    public AnimationType animationType = AnimationType.ScaleElastic;

    [Range(0.1f, 1f)]
    public float animationDuration = 0.3f;

    [ShowIf("@animationType == AnimationType.Custom")]
    public Ease customEaseIn = Ease.OutBack;

    [ShowIf("@animationType == AnimationType.Custom")]
    public Ease customEaseOut = Ease.InBack;

    private Action onCloseCallback;
    private CanvasGroup canvasGroup;
    private Vector3 originalScale;
    private Vector3 originalPosition;
    private GameObject dimObject;
    private Image dimImage;

    public enum DimType
    {
        None,           // Dim 없음
        Required,       // Dim 필수 (클릭 시 팝업 닫힘)
        Optional        // Dim 있지만 클릭해도 안 닫힘
    }

    public enum AnimationType
    {
        ScaleElastic,   // 탄성 있는 스케일
        ScaleBounce,    // 바운스 스케일
        FadeScale,      // 페이드 + 스케일
        SlideUp,        // 아래에서 위로
        SlideDown,      // 위에서 아래로
        SlideLeft,      // 오른쪽에서 왼쪽으로
        SlideRight,     // 왼쪽에서 오른쪽으로
        Rotate,         // 회전하며 나타남
        Custom          // 커스텀 Ease 설정
    }

    private void Awake()
    {
        originalScale = transform.localScale;
        originalPosition = transform.localPosition;

        // CanvasGroup 자동 추가
        canvasGroup = GetComponent<CanvasGroup>();
        if (!canvasGroup)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
    }

    /// <summary>
    /// 팝업 열기
    /// </summary>
    public void Open(Action onOpen = null, Action onClose = null)
    {
        gameObject.SetActive(true);
        onCloseCallback = onClose;

        // Dim 생성
        if (dimType != DimType.None)
        {
            CreateDim();
        }

        // 애니메이션 시작 전 초기화
        transform.DOKill();
        canvasGroup.DOKill();

        PlayOpenAnimation(() => {
            onOpen?.Invoke();
        });
    }

    /// <summary>
    /// Dim 생성
    /// </summary>
    private void CreateDim()
    {
        // Dim 오브젝트 생성
        dimObject = new GameObject("Dim");
        dimObject.transform.SetParent(transform.parent);
        dimObject.transform.SetSiblingIndex(transform.GetSiblingIndex());

        // RectTransform 설정
        RectTransform dimRect = dimObject.AddComponent<RectTransform>();
        dimRect.anchorMin = Vector2.zero;
        dimRect.anchorMax = Vector2.one;
        dimRect.sizeDelta = Vector2.zero;
        dimRect.anchoredPosition = Vector2.zero;

        // Image 컴포넌트 추가
        dimImage = dimObject.AddComponent<Image>();
        dimImage.color = new Color(dimColor.r, dimColor.g, dimColor.b, 0);
        dimImage.raycastTarget = true;

        // Dim 클릭 이벤트
        if (dimType == DimType.Required)
        {
            Button dimButton = dimObject.AddComponent<Button>();
            dimButton.onClick.AddListener(() => Close());
        }

        // Dim 페이드 인
        dimImage.DOFade(dimAlpha, animationDuration * 0.8f);

        // 팝업을 Dim 위로 이동
        transform.SetAsLastSibling();
    }

    /// <summary>
    /// 팝업 닫기
    /// </summary>
    public void Close()
    {
        // 중복 호출 방지
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;

        // Dim 페이드 아웃
        if (dimObject && dimImage)
        {
            dimImage.DOFade(0, animationDuration * 0.6f);
        }

        PlayCloseAnimation(() => {
            // Dim 제거
            if (dimObject)
            {
                Destroy(dimObject);
            }

            onCloseCallback?.Invoke();
            Destroy(gameObject);
        });
    }

    private void PlayOpenAnimation(Action onComplete)
    {
        switch (animationType)
        {
            case AnimationType.ScaleElastic:
                transform.localScale = Vector3.zero;
                transform.DOScale(originalScale, animationDuration)
                    .SetEase(Ease.OutElastic)
                    .OnComplete(() => onComplete?.Invoke());
                break;

            case AnimationType.ScaleBounce:
                transform.localScale = Vector3.zero;
                transform.DOScale(originalScale, animationDuration)
                    .SetEase(Ease.OutBounce)
                    .OnComplete(() => onComplete?.Invoke());
                break;

            case AnimationType.FadeScale:
                transform.localScale = originalScale * 0.8f;
                canvasGroup.alpha = 0;

                transform.DOScale(originalScale, animationDuration).SetEase(Ease.OutQuart);
                canvasGroup.DOFade(1, animationDuration)
                    .SetEase(Ease.OutQuart)
                    .OnComplete(() => onComplete?.Invoke());
                break;

            case AnimationType.SlideUp:
                transform.localPosition = originalPosition - new Vector3(0, Screen.height, 0);
                transform.DOLocalMove(originalPosition, animationDuration)
                    .SetEase(Ease.OutCubic)
                    .OnComplete(() => onComplete?.Invoke());
                break;

            case AnimationType.SlideDown:
                transform.localPosition = originalPosition + new Vector3(0, Screen.height, 0);
                transform.DOLocalMove(originalPosition, animationDuration)
                    .SetEase(Ease.OutCubic)
                    .OnComplete(() => onComplete?.Invoke());
                break;

            case AnimationType.SlideLeft:
                transform.localPosition = originalPosition + new Vector3(Screen.width, 0, 0);
                transform.DOLocalMove(originalPosition, animationDuration)
                    .SetEase(Ease.OutCubic)
                    .OnComplete(() => onComplete?.Invoke());
                break;

            case AnimationType.SlideRight:
                transform.localPosition = originalPosition - new Vector3(Screen.width, 0, 0);
                transform.DOLocalMove(originalPosition, animationDuration)
                    .SetEase(Ease.OutCubic)
                    .OnComplete(() => onComplete?.Invoke());
                break;

            case AnimationType.Rotate:
                transform.localScale = Vector3.zero;
                transform.localRotation = Quaternion.Euler(0, 0, 180);

                transform.DOScale(originalScale, animationDuration).SetEase(Ease.OutBack);
                transform.DORotate(Vector3.zero, animationDuration)
                    .SetEase(Ease.OutBack)
                    .OnComplete(() => onComplete?.Invoke());
                break;

            case AnimationType.Custom:
                transform.localScale = Vector3.zero;
                transform.DOScale(originalScale, animationDuration)
                    .SetEase(customEaseIn)
                    .OnComplete(() => onComplete?.Invoke());
                break;
        }
    }

    private void PlayCloseAnimation(Action onComplete)
    {
        switch (animationType)
        {
            case AnimationType.ScaleElastic:
            case AnimationType.ScaleBounce:
                transform.DOScale(Vector3.zero, animationDuration * 0.7f)
                    .SetEase(Ease.InBack)
                    .OnComplete(() => onComplete?.Invoke());
                break;

            case AnimationType.FadeScale:
                transform.DOScale(originalScale * 0.8f, animationDuration * 0.7f).SetEase(Ease.InQuart);
                canvasGroup.DOFade(0, animationDuration * 0.7f)
                    .SetEase(Ease.InQuart)
                    .OnComplete(() => onComplete?.Invoke());
                break;

            case AnimationType.SlideUp:
                transform.DOLocalMove(originalPosition + new Vector3(0, Screen.height, 0), animationDuration * 0.7f)
                    .SetEase(Ease.InCubic)
                    .OnComplete(() => onComplete?.Invoke());
                break;

            case AnimationType.SlideDown:
                transform.DOLocalMove(originalPosition - new Vector3(0, Screen.height, 0), animationDuration * 0.7f)
                    .SetEase(Ease.InCubic)
                    .OnComplete(() => onComplete?.Invoke());
                break;

            case AnimationType.SlideLeft:
                transform.DOLocalMove(originalPosition - new Vector3(Screen.width, 0, 0), animationDuration * 0.7f)
                    .SetEase(Ease.InCubic)
                    .OnComplete(() => onComplete?.Invoke());
                break;

            case AnimationType.SlideRight:
                transform.DOLocalMove(originalPosition + new Vector3(Screen.width, 0, 0), animationDuration * 0.7f)
                    .SetEase(Ease.InCubic)
                    .OnComplete(() => onComplete?.Invoke());
                break;

            case AnimationType.Rotate:
                transform.DOScale(Vector3.zero, animationDuration * 0.7f).SetEase(Ease.InBack);
                transform.DORotate(new Vector3(0, 0, -180), animationDuration * 0.7f)
                    .SetEase(Ease.InBack)
                    .OnComplete(() => onComplete?.Invoke());
                break;

            case AnimationType.Custom:
                transform.DOScale(Vector3.zero, animationDuration * 0.7f)
                    .SetEase(customEaseOut)
                    .OnComplete(() => onComplete?.Invoke());
                break;
        }
    }

    private void OnDestroy()
    {
        // DOTween 정리
        transform.DOKill();
        if (canvasGroup) canvasGroup.DOKill();
        if (dimImage) dimImage.DOKill();

        // Dim 오브젝트 정리
        if (dimObject) Destroy(dimObject);
    }

    [Title("테스트")]
    [Button("애니메이션 테스트", ButtonSizes.Large)]
    private void TestAnimation()
    {
        if (!Application.isPlaying) return;

        Close();
        DOVirtual.DelayedCall(1f, () => {
            gameObject.SetActive(true);
            Open();
        });
    }
}