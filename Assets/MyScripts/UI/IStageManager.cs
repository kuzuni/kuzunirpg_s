
// 스테이지 관리자 (Dependency Inversion)
public interface IStageManager
{
    StageData CurrentStage { get; }
    MonsterInstance CurrentMonster { get; }
    void StartStage(int stageNumber);
    void CompleteStage();
    float GetStageProgress();
}
