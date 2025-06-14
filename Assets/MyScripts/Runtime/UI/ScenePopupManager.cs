using UnityEngine;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;

public class ScenePopupManager : PopupManager
{
    [Title("Scene Popup 설정")]
    [InfoBox("Scene에서 사용되는 팝업을 관리합니다. 새로운 팝업을 열면 기존 팝업은 자동으로 닫힙니다.")]
    
    [Title("활성 팝업 관리")]
    [ShowInInspector]
    [ReadOnly]
    [InfoBox("현재 컨테이너에 있는 모든 활성 팝업들")]
    private List<GameObject> activePopups = new List<GameObject>();

    private void Start()
    {
        // 시작 시 컨테이너에 있는 기존 팝업들 확인
        RefreshActivePopupsList();
    }

    /// <summary>
    /// 팝업 열기 - 기존 팝업들을 모두 닫고 새 팝업 열기
    /// </summary>
    public override PopupUI Pop(PopupType type, Action onOpen = null, Action onClose = null)
    {
        Debug.Log($"[ScenePopupManager] Pop 호출됨: {type}");

        // 컨테이너에 있는 모든 기존 팝업 닫기
        CloseAllPopupsInContainer();

        // 부모 클래스의 Pop 메서드 호출
        return base.Pop(type, onOpen, onClose);
    }

    /// <summary>
    /// 컨테이너에 있는 모든 팝업 닫기
    /// </summary>
    private void CloseAllPopupsInContainer()
    {
        if (popupContainer == null) return;

        Debug.Log($"[ScenePopupManager] 컨테이너의 모든 팝업 닫기");

        // 컨테이너의 모든 자식 오브젝트 확인
        int childCount = popupContainer.childCount;
        for (int i = childCount - 1; i >= 0; i--)
        {
            Transform child = popupContainer.GetChild(i);
            if (child != null)
            {
                // PopupUI 컴포넌트가 있으면 Close 호출
                PopupUI popupUI = child.GetComponent<PopupUI>();
                if (popupUI != null)
                {
                    popupUI.Close();
                }
                else
                {
                    // PopupUI가 없으면 직접 삭제
                    Destroy(child.gameObject);
                }
            }
        }

        // 활성 팝업 리스트 초기화
        activePopups.Clear();
    }

    /// <summary>
    /// 현재 컨테이너에 있는 활성 팝업 리스트 갱신
    /// </summary>
    private void RefreshActivePopupsList()
    {
        activePopups.Clear();
        
        if (popupContainer == null) return;

        for (int i = 0; i < popupContainer.childCount; i++)
        {
            Transform child = popupContainer.GetChild(i);
            if (child != null && child.gameObject.activeSelf)
            {
                activePopups.Add(child.gameObject);
            }
        }

        Debug.Log($"[ScenePopupManager] 활성 팝업 개수: {activePopups.Count}");
    }

    [Title("디버그 기능")]
    [Button("활성 팝업 리스트 갱신", ButtonSizes.Medium)]
    private void DebugRefreshActivePopups()
    {
        RefreshActivePopupsList();
        
        if (activePopups.Count > 0)
        {
            Debug.Log("[ScenePopupManager] 현재 활성 팝업들:");
            foreach (var popup in activePopups)
            {
                Debug.Log($"  - {popup.name}");
            }
        }
        else
        {
            Debug.Log("[ScenePopupManager] 현재 활성 팝업이 없습니다.");
        }
    }

    [Button("모든 팝업 강제 닫기", ButtonSizes.Medium), GUIColor(1f, 0.5f, 0.5f)]
    private void ForceCloseAllPopups()
    {
        if (!Application.isPlaying) return;
        
        CloseAllPopupsInContainer();
        Debug.Log("[ScenePopupManager] 모든 팝업이 강제로 닫혔습니다.");
    }

    [Title("Scene 전용 팝업 타입")]
    [InfoBox("Scene에서 주로 사용되는 팝업 타입들")]
    [EnumToggleButtons]
    public PopupType[] sceneSpecificTypes = new PopupType[]
    {
        PopupType.Game_Base,
        PopupType.Adventure
    };

    /// <summary>
    /// Scene 전용 타입인지 확인
    /// </summary>
    public bool IsSceneSpecificType(PopupType type)
    {
        foreach (var sceneType in sceneSpecificTypes)
        {
            if (sceneType == type) return true;
        }
        return false;
    }

    [Title("편의 메서드")]
    [InfoBox("Scene에서 자주 사용되는 팝업을 빠르게 열 수 있는 메서드들")]
    
    [Button("Game Base 팝업 열기", ButtonSizes.Large)]
    public void OpenGameBase()
    {
        Pop(PopupType.Game_Base);
    }

    [Button("Adventure 팝업 열기", ButtonSizes.Large)]
    public void OpenAdventure()
    {
        Pop(PopupType.Adventure);
    }

    /// <summary>
    /// 컨테이너에 팝업이 있는지 확인
    /// </summary>
    public bool HasAnyPopup()
    {
        if (popupContainer == null) return false;
        return popupContainer.childCount > 0;
    }

    /// <summary>
    /// 특정 타입의 팝업이 현재 열려있는지 확인
    /// </summary>
    public bool IsSpecificPopupOpen(PopupType type)
    {
        if (popupContainer == null) return false;

        for (int i = 0; i < popupContainer.childCount; i++)
        {
            Transform child = popupContainer.GetChild(i);
            if (child != null)
            {
                // 팝업 이름으로 타입 추정 (프리팹 이름이 PopupType과 일치한다고 가정)
                if (child.name.Contains(type.ToString()))
                {
                    return true;
                }
            }
        }

        return false;
    }

    private void OnValidate()
    {
        // Inspector에서 값이 변경될 때 호출
        if (Application.isPlaying)
        {
            RefreshActivePopupsList();
        }
    }
}