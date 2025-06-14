using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Sirenix.OdinInspector;
using System;
using DG.Tweening;
using RPG.UI.Popup;
using RPG.UI.Components;
namespace RPG.UI.Popup
{

    public class PopupToggleButton : SerializedMonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
    {
        [Title("팝업 타입 설정")]
        [EnumToggleButtons]
        [InfoBox("이 버튼이 열 팝업 타입을 선택하세요")]
        public PopupManager.PopupType popupType = PopupManager.PopupType.Equipment;

        [Title("버튼 설정")]
        [Required("버튼 컴포넌트")]
        public Button button;

        [Title("GameObject 설정")]
        [Required("아이콘 GameObject")]
        [InfoBox("버튼 내부의 기본 아이콘 GameObject")]
        public GameObject iconObject;

        [Required("X 모양 GameObject")]
        [InfoBox("버튼 내부의 X 모양 GameObject")]
        public GameObject closeObject;

        [Title("팝업 매니저")]
        [Required("ScenePopupManager 참조")]
        public ScenePopupManager popupManager;

        [Title("애니메이션 설정")]
        [FoldoutGroup("DoTween 설정")]
        [FoldoutGroup("DoTween 설정/호버 효과")]
        public float hoverScale = 1.1f;

        [FoldoutGroup("DoTween 설정/호버 효과")]
        public float hoverDuration = 0.2f;

        [FoldoutGroup("DoTween 설정/호버 효과")]
        public Ease hoverEase = Ease.OutQuad;

        [FoldoutGroup("DoTween 설정/클릭 효과")]
        public float clickScale = 0.95f;

        [FoldoutGroup("DoTween 설정/클릭 효과")]
        public float clickDuration = 0.1f;

        [FoldoutGroup("DoTween 설정/클릭 효과")]
        public Ease clickEase = Ease.InOutQuad;

        [FoldoutGroup("DoTween 설정/팝업 열기 효과")]
        public bool usePopupOpenAnimation = true;

        [FoldoutGroup("DoTween 설정/팝업 열기 효과")]
        [ShowIf("usePopupOpenAnimation")]
        public float popupOpenRotation = 180f;

        [FoldoutGroup("DoTween 설정/팝업 열기 효과")]
        [ShowIf("usePopupOpenAnimation")]
        public float popupOpenDuration = 0.3f;

        [FoldoutGroup("DoTween 설정/팝업 열기 효과")]
        [ShowIf("usePopupOpenAnimation")]
        public Ease popupOpenEase = Ease.OutBack;

        [Title("상태")]
        [ShowInInspector]
        [ReadOnly]
        private bool isPopupOpen = false;

        private PopupUI currentPopup;
        private Vector3 originalScale;
        private bool isHovering = false;
        private bool isPressed = false;
        private Sequence currentSequence;

        private void Awake()
        {
            // 원본 스케일 저장
            originalScale = transform.localScale;

            // 버튼이 설정되지 않았다면 자신의 버튼 컴포넌트 찾기
            if (button == null)
            {
                button = GetComponent<Button>();
            }

            // 버튼 클릭 이벤트 등록
            if (button != null)
            {
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(OnButtonClick);
            }
        }

        private void Start()
        {
            // 시작 시 기본 상태로 설정 (아이콘 보이고, X 숨김)
            SetButtonState(false);
        }

        /// <summary>
        /// 버튼 클릭 처리
        /// </summary>
        private void OnButtonClick()
        {
            if (!isPopupOpen)
            {
                // 팝업 열기
                OpenPopup();
            }
            else
            {
                // 팝업 닫기
                ClosePopup();
            }
        }

        /// <summary>
        /// 팝업 열기
        /// </summary>
        private void OpenPopup()
        {
            if (popupManager == null)
            {
                Debug.LogError($"[PopupToggleButton] {popupType} - PopupManager가 설정되지 않았습니다!");
                return;
            }

            // 팝업 열기 애니메이션
            if (usePopupOpenAnimation)
            {
                PlayPopupOpenAnimation();
            }

            // 팝업 열기
            currentPopup = popupManager.Pop(
                popupType,
                onOpen: () =>
                {
                    Debug.Log($"[PopupToggleButton] {popupType} 팝업이 열렸습니다.");
                    isPopupOpen = true;
                    SetButtonState(true);
                },
                onClose: () =>
                {
                    Debug.Log($"[PopupToggleButton] {popupType} 팝업이 닫혔습니다.");
                    isPopupOpen = false;
                    SetButtonState(false);
                    currentPopup = null;

                    // 팝업 닫기 애니메이션
                    if (usePopupOpenAnimation)
                    {
                        PlayPopupCloseAnimation();
                    }
                }
            );
        }

        /// <summary>
        /// 팝업 닫기
        /// </summary>
        private void ClosePopup()
        {
            if (currentPopup != null)
            {
                // PopupUI의 Close 메서드 호출
                currentPopup.Close();
            }
            else if (popupManager != null)
            {
                // PopupUI가 없는 경우 매니저의 Close 호출
                popupManager.Close();
            }

            isPopupOpen = false;
            SetButtonState(false);
            currentPopup = null;
        }

        /// <summary>
        /// 버튼 내부 GameObject 상태 변경
        /// </summary>
        /// <param name="showClose">true면 X 표시, false면 아이콘 표시</param>
        private void SetButtonState(bool showClose)
        {
            if (iconObject != null)
            {
                iconObject.SetActive(!showClose);
            }
            else
            {
                Debug.LogWarning($"[PopupToggleButton] {popupType} - 아이콘 GameObject가 설정되지 않았습니다!");
            }

            if (closeObject != null)
            {
                closeObject.SetActive(showClose);
            }
            else
            {
                Debug.LogWarning($"[PopupToggleButton] {popupType} - X GameObject가 설정되지 않았습니다!");
            }
        }

        /// <summary>
        /// 외부에서 팝업 상태 확인
        /// </summary>
        public bool IsPopupOpen()
        {
            return isPopupOpen;
        }

        /// <summary>
        /// 외부에서 팝업 강제 닫기
        /// </summary>
        public void ForceClosePopup()
        {
            if (isPopupOpen)
            {
                ClosePopup();
            }
        }

        /// <summary>
        /// 다른 팝업이 열릴 때 이 버튼의 팝업이 닫혔는지 확인
        /// </summary>
        private void Update()
        {
            // 팝업이 열려있다고 표시되어 있지만 실제로는 없는 경우
            if (isPopupOpen && currentPopup == null)
            {
                // ScenePopupManager가 다른 팝업을 열어서 이 팝업이 닫힌 경우
                if (popupManager != null && popupManager.GetCurrentPopupType() != popupType)
                {
                    isPopupOpen = false;
                    SetButtonState(false);
                }
            }
        }

        private void OnDestroy()
        {
            // 버튼 이벤트 해제
            if (button != null)
            {
                button.onClick.RemoveListener(OnButtonClick);
            }

            // 진행 중인 애니메이션 정리
            currentSequence?.Kill();
            transform.DOKill();
        }

        #region DoTween 애니메이션

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (!button.interactable) return;

            isHovering = true;
            if (!isPressed)
            {
                transform.DOScale(originalScale * hoverScale, hoverDuration).SetEase(hoverEase);
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            isHovering = false;
            if (!isPressed)
            {
                transform.DOScale(originalScale, hoverDuration).SetEase(hoverEase);
            }
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (!button.interactable) return;

            isPressed = true;
            transform.DOScale(originalScale * clickScale, clickDuration).SetEase(clickEase);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            isPressed = false;
            float targetScale = isHovering ? hoverScale : 1f;
            transform.DOScale(originalScale * targetScale, clickDuration).SetEase(clickEase);
        }

        private void PlayPopupOpenAnimation()
        {
            currentSequence?.Kill();
            currentSequence = DOTween.Sequence();

            // X 아이콘 회전 애니메이션
            if (closeObject != null)
            {
                closeObject.transform.localRotation = Quaternion.identity;
                currentSequence.Append(
                    closeObject.transform.DORotate(new Vector3(0, 0, popupOpenRotation), popupOpenDuration)
                        .SetEase(popupOpenEase)
                );
            }

            // 버튼 전체 펄스 효과
            currentSequence.Join(
                transform.DOPunchScale(originalScale * 0.1f, popupOpenDuration, 10, 1f)
            );
        }

        private void PlayPopupCloseAnimation()
        {
            currentSequence?.Kill();
            currentSequence = DOTween.Sequence();

            // 아이콘 복귀 애니메이션
            if (iconObject != null)
            {
                iconObject.transform.localScale = Vector3.zero;
                currentSequence.Append(
                    iconObject.transform.DOScale(1f, popupOpenDuration * 0.5f)
                        .SetEase(Ease.OutBack)
                );
            }
        }

        #endregion

        private void OnValidate()
        {
            // Inspector에서 값 변경 시 초기 상태 설정
            if (!Application.isPlaying && iconObject != null && closeObject != null)
            {
                iconObject.SetActive(true);
                closeObject.SetActive(false);
            }

            // 버튼 이름 자동 설정 (선택사항)
            if (!Application.isPlaying && gameObject != null)
            {
                gameObject.name = $"{popupType} Toggle Button";
            }
        }

        [Title("디버그")]
        [Button("팝업 토글 테스트", ButtonSizes.Large)]
        private void TestToggle()
        {
            if (!Application.isPlaying) return;
            OnButtonClick();
        }

        [Button("상태 확인", ButtonSizes.Medium)]
        private void CheckStatus()
        {
            Debug.Log($"[PopupToggleButton] {popupType} 팝업 열림 상태: {isPopupOpen}");
            Debug.Log($"[PopupToggleButton] 현재 팝업: {(currentPopup != null ? "있음" : "없음")}");
            Debug.Log($"[PopupToggleButton] 아이콘 활성화: {(iconObject != null ? iconObject.activeSelf : false)}");
            Debug.Log($"[PopupToggleButton] X 활성화: {(closeObject != null ? closeObject.activeSelf : false)}");
        }

        [Button("GameObject 상태 초기화", ButtonSizes.Medium), GUIColor(0.3f, 1f, 0.3f)]
        private void ResetObjectStates()
        {
            SetButtonState(false);
            isPopupOpen = false;
            currentPopup = null;
        }
    }
}