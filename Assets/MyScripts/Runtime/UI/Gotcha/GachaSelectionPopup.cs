// ===== 1. 인터페이스 정의 (Interface Segregation Principle) =====

using System;
using UnityEngine;
using RPG.UI.Popup;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine.UI;








// ===== 4. 가챠 선택 팝업 (UI만 담당) =====
namespace RPG.UI.Gacha
{
    /// <summary>
    /// 가챠 선택 팝업 - UI 표시만 담당
    /// </summary>
    public class GachaSelectionPopup : PopupUI
    {
        [Title("UI References")]
        [SerializeField] private Button pull1Button;
        [SerializeField] private Button pull11Button;
        [SerializeField] private Button pull55Button;

        [SerializeField] private TextMeshProUGUI cost1Text;
        [SerializeField] private TextMeshProUGUI cost11Text;
        [SerializeField] private TextMeshProUGUI cost55Text;

        public event Action<int> OnPullRequested;

        private void Start()
        {
            SetupButtons();
        }

        private void SetupButtons()
        {
            if (pull1Button) pull1Button.onClick.AddListener(() => OnPullRequested?.Invoke(1));
            if (pull11Button) pull11Button.onClick.AddListener(() => OnPullRequested?.Invoke(11));
            if (pull55Button) pull55Button.onClick.AddListener(() => OnPullRequested?.Invoke(55));
        }

        public void UpdateCostDisplay(int cost1, int cost11, int cost55)
        {
            if (cost1Text) cost1Text.text = cost1.ToString();
            if (cost11Text) cost11Text.text = cost11.ToString();
            if (cost55Text) cost55Text.text = cost55.ToString();
        }

        public void UpdateButtonStates(bool can1, bool can11, bool can55)
        {
            if (pull1Button) pull1Button.interactable = can1;
            if (pull11Button) pull11Button.interactable = can11;
            if (pull55Button) pull55Button.interactable = can55;
        }
    }
}

// ===== 7. 사용 예시 =====

/*
1. PopupManager의 PopupType에 추가:
   - Gacha (가챠 선택 팝업)
   - GachaResult (가챠 결과 팝업)

2. 씬 구성:
   - GachaFlowController (GameObject)
     ├── EquipmentGachaExecutor (Component)
     ├── DiamondCostHandler (Component)
     └── PopupManager 참조

3. 프리팹 구성:
   - GachaSelectionPopup (선택 UI만)
   - GachaResultPopup (결과 표시만)

4. 실행:
   gachaFlowController.OpenGachaSelection();
*/