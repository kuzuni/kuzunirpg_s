// ===== 1. 인터페이스 정의 (Interface Segregation Principle) =====

using System;
using System.Collections.Generic;
using UnityEngine;
using RPG.Gacha.Base;
using DG.Tweening;
using RPG.Gacha.Interfaces;
using RPG.UI.Popup;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine.UI;









// ===== 5. 가챠 결과 팝업 (결과 표시만 담당) =====


namespace RPG.UI.Gacha
{
    /// <summary>
    /// 가챠 결과 팝업 - 결과 표시만 담당
    /// </summary>
    public class GachaResultPopup : PopupUI, IGachaResultDisplay
    {
        [Title("UI References")]
        [SerializeField] private Transform resultContainer;
        [SerializeField] private GameObject resultItemPrefab;
        [SerializeField] private Button confirmButton;
        [SerializeField] private TextMeshProUGUI summaryText;

        [Title("Animation Settings")]
        [SerializeField] private float itemShowDelay = 0.1f;
        [SerializeField] private float itemAnimDuration = 0.3f;

        public event Action OnDisplayComplete;

        private List<GameObject> currentItems = new List<GameObject>();

        private void Start()
        {
            if (confirmButton)
                confirmButton.onClick.AddListener(() => OnDisplayComplete?.Invoke());
        }

        public void DisplayResults(GachaResultData results)
        {
            if (results == null || results.Items == null) return;

            ClearDisplay();

            // 요약 텍스트 표시
            if (summaryText)
            {
                summaryText.text = $"{results.PullCount}회 뽑기 결과";
            }

            // 등급별 정렬
            results.Items.Sort((a, b) => b.GetRarityLevel().CompareTo(a.GetRarityLevel()));

            // 결과 표시
            StartCoroutine(ShowResultsAnimated(results.Items));
        }

        private System.Collections.IEnumerator ShowResultsAnimated(List<IGachaItem> items)
        {
            for (int i = 0; i < items.Count; i++)
            {
                CreateResultItem(items[i], i);
                yield return new WaitForSeconds(itemShowDelay);
            }
        }

        private void CreateResultItem(IGachaItem item, int index)
        {
            if (!resultItemPrefab || !resultContainer) return;

            var obj = Instantiate(resultItemPrefab, resultContainer);
            currentItems.Add(obj);

            // 아이템 정보 설정
            var iconImage = obj.GetComponentInChildren<Image>();
            if (iconImage && item.Icon)
                iconImage.sprite = item.Icon;

            var nameText = obj.GetComponentInChildren<TextMeshProUGUI>();
            if (nameText)
            {
                nameText.text = item.ItemName;
                nameText.color = item.GetRarityColor();
            }

            // 애니메이션
            obj.transform.localScale = Vector3.zero;
            obj.transform.DOScale(1f, itemAnimDuration)
                .SetEase(Ease.OutBack)
                .SetDelay(index * 0.02f);
        }

        public void ClearDisplay()
        {
            foreach (var item in currentItems)
            {
                if (item != null) Destroy(item);
            }
            currentItems.Clear();
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