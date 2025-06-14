using RPG.Common;
using RPG.Core.Events;
using RPG.Gacha.Interfaces;
using RPG.UI.Gacha;
using RPG.UI.Popup;
using Sirenix.OdinInspector;
using UnityEngine;

namespace RPG.Gacha.Core
{
    /// <summary>
    /// 가챠 플로우 컨트롤러 - 전체 흐름 관리
    /// Dependency Injection으로 필요한 컴포넌트들을 받음
    /// </summary>
    public class GachaFlowController : MonoBehaviour
    {
        [Title("Dependencies")]
        [SerializeField] private PopupManager popupManager;

        // 인터페이스 대신 구체적인 클래스 타입으로 변경
        [SerializeField] private EquipmentGachaExecutor equipmentGachaExecutor;
        [SerializeField] private DiamondCostHandler diamondCostHandler;

        // 인터페이스는 private 필드로 사용
        private IGachaExecutor gachaExecutor;
        private IGachaCostHandler costHandler;

        private GachaSelectionPopup selectionPopup;
        private GachaResultPopup resultPopup;

        private void Start()
        {
            // 의존성 주입
            if (equipmentGachaExecutor == null)
                equipmentGachaExecutor = GetComponent<EquipmentGachaExecutor>();

            if (diamondCostHandler == null)
                diamondCostHandler = GetComponent<DiamondCostHandler>();

            // 인터페이스에 할당
            gachaExecutor = equipmentGachaExecutor;
            costHandler = diamondCostHandler;

            // 초기화
            if (equipmentGachaExecutor != null)
            {
                equipmentGachaExecutor.Initialize(costHandler);
            }

            // 이벤트 구독
            GameEventManager.OnCurrencyChanged += OnCurrencyChanged;
        }

        private void OnDestroy()
        {
            GameEventManager.OnCurrencyChanged -= OnCurrencyChanged;
        }

        /// <summary>
        /// 가챠 선택 팝업 열기
        /// </summary>
        public void OpenGachaSelection()
        {
            var popup = popupManager.Pop(PopupManager.PopupType.Gacha);
            selectionPopup = popup as GachaSelectionPopup;

            if (selectionPopup != null)
            {
                // 이벤트 연결
                selectionPopup.OnPullRequested += HandlePullRequest;

                // UI 업데이트
                UpdateSelectionUI();
            }
        }

        /// <summary>
        /// 뽑기 요청 처리
        /// </summary>
        private void HandlePullRequest(int pullCount)
        {
            if (gachaExecutor == null || !gachaExecutor.CanExecute(pullCount))
            {
                ShowErrorMessage("다이아몬드가 부족합니다!");
                return;
            }

            // 가챠 실행
            var results = gachaExecutor.Execute(pullCount);

            if (results != null)
            {
                // 선택 팝업 닫기
                popupManager.Close();

                // 결과 팝업 열기
                ShowResults(results);
            }
        }

        /// <summary>
        /// 결과 팝업 표시
        /// </summary>
        private void ShowResults(GachaResultData results)
        {
            var popup = popupManager.Pop(PopupManager.PopupType.GachaResult);
            resultPopup = popup as GachaResultPopup;

            if (resultPopup != null)
            {
                // 이벤트 연결
                resultPopup.OnDisplayComplete += HandleResultConfirm;

                // 결과 표시
                resultPopup.DisplayResults(results);
            }
        }

        /// <summary>
        /// 결과 확인 처리
        /// </summary>
        private void HandleResultConfirm()
        {
            // 결과 팝업 닫기
            popupManager.Close();

            // 선택 팝업 다시 열기 (선택사항)
            // OpenGachaSelection();
        }

        /// <summary>
        /// 선택 UI 업데이트
        /// </summary>
        private void UpdateSelectionUI()
        {
            if (selectionPopup == null || gachaExecutor == null) return;

            // 비용 표시
            selectionPopup.UpdateCostDisplay(
                gachaExecutor.GetCost(1),
                gachaExecutor.GetCost(11),
                gachaExecutor.GetCost(55)
            );

            // 버튼 상태
            selectionPopup.UpdateButtonStates(
                gachaExecutor.CanExecute(1),
                gachaExecutor.CanExecute(11),
                gachaExecutor.CanExecute(55)
            );
        }


        private void OnCurrencyChanged(CurrencyType type, long amount)
        {
            if (type == CurrencyType.Diamond && selectionPopup != null)
            {
                UpdateSelectionUI();
            }
        }

        private void ShowErrorMessage(string message)
        {
            Debug.LogError(message);
            // TODO: 에러 팝업 표시
        }

        [Title("디버그")]
        [Button("가챠 선택 팝업 열기", ButtonSizes.Large)]
        private void DebugOpenGachaSelection()
        {
            if (Application.isPlaying)
            {
                OpenGachaSelection();
            }
        }
    }
}