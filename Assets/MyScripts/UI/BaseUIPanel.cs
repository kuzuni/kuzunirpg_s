using UnityEngine;
using System;
using Sirenix.OdinInspector;
// UI 패널 베이스 클래스 (Open/Closed Principle)
public abstract class BaseUIPanel : MonoBehaviour, IUIPanel
{
    [Title("패널 설정")]
    [SerializeField] protected GameObject panelRoot;
    [SerializeField] protected CanvasGroup canvasGroup;
    
    [SerializeField] protected bool useAnimation = true;
    [SerializeField, ShowIf("useAnimation")] protected float animationDuration = 0.3f;
    
    [ShowInInspector, ReadOnly]
    public bool IsVisible { get; private set; }
    
    protected bool isAnimating = false;
    
    public event Action OnPanelShown;
    public event Action OnPanelHidden;
    
    protected virtual void Awake()
    {
        if (panelRoot == null) panelRoot = gameObject;
        if (canvasGroup == null) canvasGroup = GetComponent<CanvasGroup>();
    }
    
    public virtual void Show()
    {
        if (IsVisible || isAnimating) return;
        
        IsVisible = true;
        panelRoot.SetActive(true);
        
        if (useAnimation && canvasGroup != null)
        {
            StartCoroutine(FadeIn());
        }
        else
        {
            OnShowComplete();
        }
    }
    
    public virtual void Hide()
    {
        if (!IsVisible || isAnimating) return;
        
        IsVisible = false;
        
        if (useAnimation && canvasGroup != null)
        {
            StartCoroutine(FadeOut());
        }
        else
        {
            OnHideComplete();
        }
    }
    
    public abstract void UpdatePanel();
    
    protected virtual void OnShowComplete()
    {
        OnPanelShown?.Invoke();
        UpdatePanel();
    }
    
    protected virtual void OnHideComplete()
    {
        panelRoot.SetActive(false);
        OnPanelHidden?.Invoke();
    }
    
    private System.Collections.IEnumerator FadeIn()
    {
        isAnimating = true;
        float elapsed = 0;
        
        while (elapsed < animationDuration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / animationDuration;
            canvasGroup.alpha = Mathf.Lerp(0, 1, progress);
            yield return null;
        }
        
        canvasGroup.alpha = 1;
        isAnimating = false;
        OnShowComplete();
    }
    
    private System.Collections.IEnumerator FadeOut()
    {
        isAnimating = true;
        float elapsed = 0;
        
        while (elapsed < animationDuration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / animationDuration;
            canvasGroup.alpha = Mathf.Lerp(1, 0, progress);
            yield return null;
        }
        
        canvasGroup.alpha = 0;
        isAnimating = false;
        OnHideComplete();
    }
}
