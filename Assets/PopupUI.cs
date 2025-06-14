using UnityEngine;
using UnityEngine.UI;
using System;
using DG.Tweening;
using Sirenix.OdinInspector;

public class PopupUI : MonoBehaviour
{
    [Title("Dim ����")]
    [EnumToggleButtons]
    public DimType dimType = DimType.Required;

    [ShowIf("@dimType != DimType.None")]
    [Range(0f, 1f)]
    public float dimAlpha = 0.7f;

    [ShowIf("@dimType != DimType.None")]
    [ColorUsage(true, false)]
    public Color dimColor = Color.black;

    [Title("�ִϸ��̼� ����")]
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
        None,           // Dim ����
        Required,       // Dim �ʼ� (Ŭ�� �� �˾� ����)
        Optional        // Dim ������ Ŭ���ص� �� ����
    }

    public enum AnimationType
    {
        ScaleElastic,   // ź�� �ִ� ������
        ScaleBounce,    // �ٿ ������
        FadeScale,      // ���̵� + ������
        SlideUp,        // �Ʒ����� ����
        SlideDown,      // ������ �Ʒ���
        SlideLeft,      // �����ʿ��� ��������
        SlideRight,     // ���ʿ��� ����������
        Rotate,         // ȸ���ϸ� ��Ÿ��
        Custom          // Ŀ���� Ease ����
    }

    private void Awake()
    {
        originalScale = transform.localScale;
        originalPosition = transform.localPosition;

        // CanvasGroup �ڵ� �߰�
        canvasGroup = GetComponent<CanvasGroup>();
        if (!canvasGroup)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
    }

    /// <summary>
    /// �˾� ����
    /// </summary>
    public void Open(Action onOpen = null, Action onClose = null)
    {
        gameObject.SetActive(true);
        onCloseCallback = onClose;

        // Dim ����
        if (dimType != DimType.None)
        {
            CreateDim();
        }

        // �ִϸ��̼� ���� �� �ʱ�ȭ
        transform.DOKill();
        canvasGroup.DOKill();

        PlayOpenAnimation(() => {
            onOpen?.Invoke();
        });
    }

    /// <summary>
    /// Dim ����
    /// </summary>
    private void CreateDim()
    {
        // Dim ������Ʈ ����
        dimObject = new GameObject("Dim");
        dimObject.transform.SetParent(transform.parent);
        dimObject.transform.SetSiblingIndex(transform.GetSiblingIndex());

        // RectTransform ����
        RectTransform dimRect = dimObject.AddComponent<RectTransform>();
        dimRect.anchorMin = Vector2.zero;
        dimRect.anchorMax = Vector2.one;
        dimRect.sizeDelta = Vector2.zero;
        dimRect.anchoredPosition = Vector2.zero;

        // Image ������Ʈ �߰�
        dimImage = dimObject.AddComponent<Image>();
        dimImage.color = new Color(dimColor.r, dimColor.g, dimColor.b, 0);
        dimImage.raycastTarget = true;

        // Dim Ŭ�� �̺�Ʈ
        if (dimType == DimType.Required)
        {
            Button dimButton = dimObject.AddComponent<Button>();
            dimButton.onClick.AddListener(() => Close());
        }

        // Dim ���̵� ��
        dimImage.DOFade(dimAlpha, animationDuration * 0.8f);

        // �˾��� Dim ���� �̵�
        transform.SetAsLastSibling();
    }

    /// <summary>
    /// �˾� �ݱ�
    /// </summary>
    public void Close()
    {
        // �ߺ� ȣ�� ����
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;

        // Dim ���̵� �ƿ�
        if (dimObject && dimImage)
        {
            dimImage.DOFade(0, animationDuration * 0.6f);
        }

        PlayCloseAnimation(() => {
            // Dim ����
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
        // DOTween ����
        transform.DOKill();
        if (canvasGroup) canvasGroup.DOKill();
        if (dimImage) dimImage.DOKill();

        // Dim ������Ʈ ����
        if (dimObject) Destroy(dimObject);
    }

    [Title("�׽�Ʈ")]
    [Button("�ִϸ��̼� �׽�Ʈ", ButtonSizes.Large)]
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