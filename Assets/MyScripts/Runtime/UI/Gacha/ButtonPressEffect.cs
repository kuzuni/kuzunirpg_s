using UnityEngine;
using DG.Tweening;

namespace RPG.UI.Gacha
{
    /// <summary>
    /// 버튼 클릭 애니메이션 컴포넌트
    /// </summary>
    public class ButtonPressEffect : MonoBehaviour, UnityEngine.EventSystems.IPointerDownHandler, UnityEngine.EventSystems.IPointerUpHandler
    {
        public float pressScale = 0.95f;
        public float duration = 0.1f;

        private Vector3 originalScale;

        private void Awake()
        {
            originalScale = transform.localScale;
        }

        public void OnPointerDown(UnityEngine.EventSystems.PointerEventData eventData)
        {
            transform.DOScale(originalScale * pressScale, duration).SetEase(Ease.OutQuad);
        }

        public void OnPointerUp(UnityEngine.EventSystems.PointerEventData eventData)
        {
            transform.DOScale(originalScale, duration).SetEase(Ease.OutBack);
        }
    }
}