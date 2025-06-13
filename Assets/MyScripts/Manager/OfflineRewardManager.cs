using UnityEngine;
using System;
using Sirenix.OdinInspector;
// 오프라인 보상 관리자
public class OfflineRewardManager : MonoBehaviour
{
    [Title("오프라인 보상 설정")]
    [SerializeField] private int goldPerSecond = 10;
    [SerializeField] private int expPerSecond = 5;
    [SerializeField] private int maxOfflineHours = 8;
    
    private CurrencyManager currencyManager;
    private PlayerController playerController;
    
    private void Start()
    {
        currencyManager = FindObjectOfType<CurrencyManager>();
        playerController = FindObjectOfType<PlayerController>();
        
        CheckOfflineRewards();
    }
    
    private void CheckOfflineRewards()
    {
        // 마지막 플레이 시간 불러오기
        string lastPlayTimeStr = PlayerPrefs.GetString("LastPlayTime", "");
        
        if (!string.IsNullOrEmpty(lastPlayTimeStr))
        {
            DateTime lastPlayTime = DateTime.Parse(lastPlayTimeStr);
            TimeSpan offlineTime = DateTime.Now - lastPlayTime;
            
            CalculateOfflineRewards(offlineTime);
        }
        
        // 현재 시간 저장
        PlayerPrefs.SetString("LastPlayTime", DateTime.Now.ToString());
        PlayerPrefs.Save();
    }
    
    private void CalculateOfflineRewards(TimeSpan offlineTime)
    {
        int offlineSeconds = Mathf.Min((int)offlineTime.TotalSeconds, maxOfflineHours * 3600);
        
        if (offlineSeconds > 60) // 1분 이상 오프라인인 경우만
        {
            int goldReward = goldPerSecond * offlineSeconds;
            int expReward = expPerSecond * offlineSeconds;
            
            currencyManager?.AddCurrency(CurrencyType.Gold, goldReward);
            playerController?.Status.AddExperience(expReward);
            
            ShowOfflineRewardPopup(offlineTime, goldReward, expReward);
        }
    }
    
    private void ShowOfflineRewardPopup(TimeSpan offlineTime, int gold, int exp)
    {
        string timeStr = $"{(int)offlineTime.TotalHours}시간 {offlineTime.Minutes}분";
        Debug.Log($"오프라인 보상: {timeStr}동안 골드 {gold}, 경험치 {exp} 획득!");
        
        // TODO: 실제 팝업 UI 표시
    }
}