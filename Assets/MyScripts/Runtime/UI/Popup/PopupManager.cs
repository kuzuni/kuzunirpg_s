using UnityEngine;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using System;
using System.Linq;

namespace RPG.UI.Popup
{
    public class PopupManager : SerializedMonoBehaviour
    {
        [Title("팝업 타입")]
        public enum PopupType
        {
            Game_Base,
            Game_Exit,
            Loading,
            Mission,
            MyInfo,
            Options,
            Notice,
            Ranking,
            Success,
            Team_Message,
            Gamble,
            Shop,
            Attendance,
            Equipment,
            Pet,
            Adventure,
            GachaResult  // 가챠 결과 팝업 추가
        }

        [Title("컨테이너 설정")]
        [Required("팝업이 생성될 컨테이너를 지정하세요")]
        [InfoBox("지정하지 않으면 첫 번째 Canvas를 자동으로 찾습니다", InfoMessageType.Info)]
        public Transform popupContainer;

        [Title("팝업 프리팹 설정")]
        [InfoBox("각 팝업 타입에 해당하는 프리팹을 설정하세요")]
        [SerializeField]
        private PopupPrefabEntry[] popupEntries = new PopupPrefabEntry[]
        {
            new PopupPrefabEntry { type = PopupType.Game_Base },
            new PopupPrefabEntry { type = PopupType.Game_Exit },
            new PopupPrefabEntry { type = PopupType.Loading },
            new PopupPrefabEntry { type = PopupType.Mission },
            new PopupPrefabEntry { type = PopupType.MyInfo },
            new PopupPrefabEntry { type = PopupType.Options },
            new PopupPrefabEntry { type = PopupType.Notice },
            new PopupPrefabEntry { type = PopupType.Ranking },
            new PopupPrefabEntry { type = PopupType.Success },
            new PopupPrefabEntry { type = PopupType.Team_Message },
            new PopupPrefabEntry { type = PopupType.Gamble },
            new PopupPrefabEntry { type = PopupType.Shop },
            new PopupPrefabEntry { type = PopupType.Attendance },
            new PopupPrefabEntry { type = PopupType.Equipment },
            new PopupPrefabEntry { type = PopupType.Pet },
            new PopupPrefabEntry { type = PopupType.Adventure },
            new PopupPrefabEntry { type = PopupType.GachaResult }  // 가챠 결과 추가
        };

        [System.Serializable]
        public class PopupPrefabEntry
        {
            [HorizontalGroup("Entry", Width = 120)]
            [HideLabel]
            [ReadOnly]
            public PopupType type;

            [HorizontalGroup("Entry")]
            [HideLabel]
            [PreviewField(50, ObjectFieldAlignment.Left)]
            public GameObject prefab;
        }

        [Title("유틸리티")]
        [Button("프리팹 배열 초기화", ButtonSizes.Large), GUIColor(1f, 1f, 0.3f)]
        [InfoBox("새로운 타입이 추가되었다면 이 버튼을 눌러 배열을 다시 초기화하세요")]
        private void ResetPrefabArray()
        {
            var oldEntries = popupEntries;
            popupEntries = new PopupPrefabEntry[]
            {
                new PopupPrefabEntry { type = PopupType.Game_Base },
                new PopupPrefabEntry { type = PopupType.Game_Exit },
                new PopupPrefabEntry { type = PopupType.Loading },
                new PopupPrefabEntry { type = PopupType.Mission },
                new PopupPrefabEntry { type = PopupType.MyInfo },
                new PopupPrefabEntry { type = PopupType.Options },
                new PopupPrefabEntry { type = PopupType.Notice },
                new PopupPrefabEntry { type = PopupType.Ranking },
                new PopupPrefabEntry { type = PopupType.Success },
                new PopupPrefabEntry { type = PopupType.Team_Message },
                new PopupPrefabEntry { type = PopupType.Gamble },
                new PopupPrefabEntry { type = PopupType.Shop },
                new PopupPrefabEntry { type = PopupType.Attendance },
                new PopupPrefabEntry { type = PopupType.Equipment },
                new PopupPrefabEntry { type = PopupType.Pet },
                new PopupPrefabEntry { type = PopupType.Adventure },
                new PopupPrefabEntry { type = PopupType.GachaResult }
            };

            // 기존 프리팹 복사
            if (oldEntries != null)
            {
                foreach (var oldEntry in oldEntries)
                {
                    foreach (var newEntry in popupEntries)
                    {
                        if (newEntry.type == oldEntry.type)
                        {
                            newEntry.prefab = oldEntry.prefab;
                            break;
                        }
                    }
                }
            }

            Debug.Log("프리팹 배열이 초기화되었습니다. 새로운 타입들이 추가되었습니다.");
        }

        [Title("현재 활성 팝업")]
        [ShowInInspector]
        [ReadOnly]
        private PopupUI currentPopup;

        [ShowInInspector]
        [ReadOnly]
        private PopupType? currentPopupType;

        private void Awake()
        {
            ValidateContainer();

            // 시작 시 모든 팝업이 닫혀있는지 확인
            currentPopup = null;
            currentPopupType = null;
        }

        private void ValidateContainer()
        {
            if (!popupContainer)
            {
                Canvas canvas = FindObjectOfType<Canvas>();
                if (canvas)
                {
                    popupContainer = canvas.transform;
                    Debug.Log($"[PopupManager] Container 자동 설정: {canvas.name}");
                }
            }

            Debug.Log($"[PopupManager] Container: {(popupContainer ? popupContainer.name : "NULL")}");
        }

        /// <summary>
        /// 팝업 열기
        /// </summary>
        public virtual PopupUI Pop(PopupType type, Action onOpen = null, Action onClose = null)
        {
            Debug.Log($"[PopupManager] Pop 호출됨: {type}");

            // 기존 팝업 닫기
            if (currentPopup != null)
            {
                Destroy(currentPopup.gameObject);
                currentPopup = null;
                currentPopupType = null;
            }

            // 프리팹 확인
            GameObject prefab = null;
            foreach (var entry in popupEntries)
            {
                if (entry.type == type)
                {
                    prefab = entry.prefab;
                    break;
                }
            }

            if (prefab == null)
            {
                Debug.LogError($"[PopupManager] {type} 팝업 프리팹이 설정되지 않았습니다!");
                return null;
            }

            ValidateContainer();

            if (!popupContainer)
            {
                Debug.LogError("[PopupManager] 팝업 컨테이너를 찾을 수 없습니다!");
                return null;
            }

            // 팝업 생성
            GameObject popupObj = Instantiate(prefab, popupContainer);
            currentPopup = popupObj.GetComponent<PopupUI>();

            Debug.Log($"[PopupManager] 팝업 생성됨: {popupObj.name} in {popupObj.transform.parent.name}");

            currentPopupType = type;

            // PopupUI 컴포넌트가 있으면 Open 호출, 없으면 그냥 활성화
            if (currentPopup != null)
            {
                currentPopup.Open(onOpen, () =>
                {
                    onClose?.Invoke();
                    if (currentPopup != null && currentPopup.gameObject == popupObj)
                    {
                        currentPopup = null;
                        currentPopupType = null;
                    }
                });
            }
            else
            {
                // PopupUI 컴포넌트가 없는 경우 단순 활성화
                popupObj.SetActive(true);
                onOpen?.Invoke();
            }

            return currentPopup;
        }

        /// <summary>
        /// 현재 팝업 닫기
        /// </summary>
        public void Close()
        {
            if (currentPopup != null)
            {
                currentPopup.Close();
            }
            else if (currentPopupType.HasValue)
            {
                // PopupUI 컴포넌트가 없는 경우 직접 삭제
                Transform child = popupContainer.GetChild(popupContainer.childCount - 1);
                if (child != null)
                {
                    Destroy(child.gameObject);
                }
                currentPopupType = null;
            }
        }

        /// <summary>
        /// 현재 열려있는 팝업 타입 확인
        /// </summary>
        public PopupType? GetCurrentPopupType()
        {
            return currentPopupType;
        }

        /// <summary>
        /// 특정 팝업이 열려있는지 확인
        /// </summary>
        public bool IsPopupOpen(PopupType type)
        {
            return currentPopupType == type;
        }

        [Title("테스트")]
        [EnumToggleButtons]
        public PopupType testPopupType = PopupType.Loading;

        [Button("테스트 팝업 열기", ButtonSizes.Large)]
        private void TestPopup()
        {
            if (!Application.isPlaying) return;
            Pop(testPopupType);
        }

        [Button("현재 팝업 닫기", ButtonSizes.Large), GUIColor(1f, 0.3f, 0.3f)]
        private void TestClose()
        {
            if (!Application.isPlaying) return;
            Close();
        }

        [Title("디버그 정보")]
        [Button("컨테이너 정보 출력", ButtonSizes.Medium)]
        private void DebugContainers()
        {
            Debug.Log($"Container: {(popupContainer ? popupContainer.name : "Not Set")}");
        }

        #region 버튼용 Public 메서드

        [Title("버튼 OnClick용 메서드")]
        [InfoBox("Unity Button의 OnClick 이벤트에서 직접 호출 가능합니다")]

        public void PopGameBase() => Pop(PopupType.Game_Base);
        public void PopGameExit() => Pop(PopupType.Game_Exit);
        public void PopLoading() => Pop(PopupType.Loading);
        public void PopMission() => Pop(PopupType.Mission);
        public void PopMyInfo() => Pop(PopupType.MyInfo);
        public void PopOptions() => Pop(PopupType.Options);
        public void PopNotice() => Pop(PopupType.Notice);
        public void PopRanking() => Pop(PopupType.Ranking);
        public void PopSuccess() => Pop(PopupType.Success);
        public void PopTeamMessage() => Pop(PopupType.Team_Message);
        public void PopGamble() => Pop(PopupType.Gamble);
        public void PopShop() => Pop(PopupType.Shop);
        public void PopAttendance() => Pop(PopupType.Attendance);
        public void PopEquipment() => Pop(PopupType.Equipment);
        public void PopPet() => Pop(PopupType.Pet);
        public void PopAdventure() => Pop(PopupType.Adventure);
        public void PopGachaResult() => Pop(PopupType.GachaResult);  // 가챠 결과 추가

        #endregion
    }
}