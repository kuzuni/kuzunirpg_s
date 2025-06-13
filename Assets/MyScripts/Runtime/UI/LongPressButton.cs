using UnityEngine;
using UnityEngine.UI;
using System;
using UnityEngine.EventSystems;
using System.Collections;
// 버튼 길게 누르기 인터페이스 (Interface Segregation Principle)
public interface ILongPressHandler
{
    void OnLongPressStart();
    void OnLongPressEnd();
    bool IsLongPressing { get; }
}
// 버튼 길게 누르기 구현 컴포넌트 (Single Responsibility Principle)
[RequireComponent(typeof(Button))]
public class LongPressButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler, ILongPressHandler
{
    [SerializeField] private float longPressDelay = 0.3f;
    [SerializeField] private float repeatInterval = 0.1f;

    private Button button;
    private Coroutine longPressCoroutine;
    private bool isPointerDown = false;

    public bool IsLongPressing { get; private set; }

    // 이벤트
    public event Action OnClick;
    public event Action OnLongPressRepeat;

    private void Awake()
    {
        button = GetComponent<Button>();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (!button.interactable) return;

        isPointerDown = true;
        OnLongPressStart();
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (!isPointerDown) return;

        bool wasLongPressing = IsLongPressing;
        OnLongPressEnd();

        // 길게 누르지 않았으면 일반 클릭으로 처리
        if (!wasLongPressing)
        {
            OnClick?.Invoke();
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (isPointerDown)
        {
            OnLongPressEnd();
        }
    }

    public void OnLongPressStart()
    {
        if (longPressCoroutine != null)
        {
            StopCoroutine(longPressCoroutine);
        }

        longPressCoroutine = StartCoroutine(LongPressRoutine());
    }

    public void OnLongPressEnd()
    {
        isPointerDown = false;
        IsLongPressing = false;

        if (longPressCoroutine != null)
        {
            StopCoroutine(longPressCoroutine);
            longPressCoroutine = null;
        }
    }

    private IEnumerator LongPressRoutine()
    {
        // 초기 지연
        yield return new WaitForSeconds(longPressDelay);

        IsLongPressing = true;

        // 연속 실행
        while (isPointerDown && button.interactable)
        {
            OnLongPressRepeat?.Invoke();
            yield return new WaitForSeconds(repeatInterval);
        }
    }

    private void OnDisable()
    {
        OnLongPressEnd();
    }

    private void OnDestroy()
    {
        OnLongPressEnd();
    }
}
