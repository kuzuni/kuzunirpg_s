using UnityEngine;
using System;
using Sirenix.OdinInspector;
// 보스 시스템
public class BossSystem : MonoBehaviour
{
    [Title("보스 설정")]
    [SerializeField] private bool isBossStage = false;
    [SerializeField] private float bossTimerDuration = 30f;
    
    [ShowInInspector, ReadOnly]
    [ProgressBar(0, "@bossTimerDuration", 0.8f, 0.3f, 0.3f)]
    private float remainingTime;
    
    public event Action<bool> OnBossDefeated; // true: 성공, false: 실패
    
    public void StartBossTimer()
    {
        isBossStage = true;
        remainingTime = bossTimerDuration;
        StartCoroutine(BossTimerCoroutine());
    }
    
    private System.Collections.IEnumerator BossTimerCoroutine()
    {
        while (remainingTime > 0 && isBossStage)
        {
            remainingTime -= Time.deltaTime;
            yield return null;
        }
        
        if (isBossStage)
        {
            // 시간 초과 - 보스 전투 실패
            OnBossDefeated?.Invoke(false);
            isBossStage = false;
        }
    }
    
    public void DefeatBoss()
    {
        if (!isBossStage) return;
        
        isBossStage = false;
        OnBossDefeated?.Invoke(true);
        
        // 보스 보상 지급
        GiveBossRewards();
    }
    
    private void GiveBossRewards()
    {
        Debug.Log("보스 처치 보상 지급!");
        // TODO: 실제 보상 로직
    }
}
