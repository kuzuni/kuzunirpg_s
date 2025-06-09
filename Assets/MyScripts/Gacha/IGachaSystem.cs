using System.Collections.Generic;

// 뽑기 시스템 공통 인터페이스
public interface IGachaSystem<T> where T : class
{
    // 단일 뽑기
    T PullSingle();
    
    // 다중 뽑기
    List<T> PullMultiple(int count);
    
    // 10+1 뽑기 (보장 포함)
    List<T> Pull11();
    
    // 천장 진행도 (0~1)
    float GetPityProgress();
    
    // 현재 천장 카운트
    int GetCurrentPityCount();
    
    // 천장 리셋
    void ResetPity();
    
    // 뽑기 확률 정보
    Dictionary<int, float> GetRateTable();
}