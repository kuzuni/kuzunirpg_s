using UnityEngine;
using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;

public class StageManager : MonoBehaviour, IStageManager
{
    [Title("스테이지 설정")]
    [SerializeField] private List<StageData> stages = new List<StageData>();
    
    [ShowInInspector, ReadOnly]
    private int currentStageIndex = 0;
    
    [ShowInInspector, ReadOnly]
    private int currentMonsterIndex = 0;
    
    [ShowInInspector, ReadOnly]
    private int monstersKilledInStage = 0;
    
    public StageData CurrentStage => stages[currentStageIndex];
    public MonsterInstance CurrentMonster { get; private set; }
    
    // 이벤트
    public event Action<StageData> OnStageChanged;
    public event Action<int> OnMonsterKilled;
    public event Action<float> OnStageProgress;
    public event Action OnStageComplete;
    
    private CurrencyManager currencyManager;
    private PlayerController playerController;
    
    private void Start()
    {
        currencyManager = FindObjectOfType<CurrencyManager>();
        playerController = FindObjectOfType<PlayerController>();
        
        // 테스트용 스테이지 생성
        if (stages.Count == 0)
        {
            CreateTestStages();
        }
        
        StartStage(1);
    }
    
    public void StartStage(int stageNumber)
    {
        currentStageIndex = Mathf.Clamp(stageNumber - 1, 0, stages.Count - 1);
        currentMonsterIndex = 0;
        monstersKilledInStage = 0;
        
        SpawnNextMonster();
        OnStageChanged?.Invoke(CurrentStage);
    }
    
    private void SpawnNextMonster()
    {
        if (currentMonsterIndex < CurrentStage.monsters.Count)
        {
            var monsterData = CurrentStage.monsters[currentMonsterIndex];
            CurrentMonster = new MonsterInstance(monsterData);
            Debug.Log($"몬스터 생성: {CurrentMonster.MonsterName}");
        }
    }
    
    public void DamageMonster(int damage)
    {
        if (CurrentMonster == null) return;
        
        CurrentMonster.CurrentHp -= damage;
        
        if (CurrentMonster.CurrentHp <= 0)
        {
            KillCurrentMonster();
        }
    }
    
    private void KillCurrentMonster()
    {
        if (CurrentMonster == null) return;
        
        // 보상 지급
        currencyManager?.AddCurrency(CurrencyType.Gold, CurrentMonster.data.goldReward);
        playerController?.Status.AddExperience(CurrentMonster.data.expReward);
        
        monstersKilledInStage++;
        currentMonsterIndex++;
        
        OnMonsterKilled?.Invoke(monstersKilledInStage);
        OnStageProgress?.Invoke(GetStageProgress());
        
        // 다음 몬스터 또는 스테이지 완료
        if (currentMonsterIndex >= CurrentStage.monsters.Count)
        {
            CompleteStage();
        }
        else
        {
            SpawnNextMonster();
        }
    }
    
    public void CompleteStage()
    {
        // 스테이지 클리어 보상
        currencyManager?.AddCurrency(CurrencyType.Gold, CurrentStage.clearGold);
        playerController?.Status.AddExperience(CurrentStage.clearExp);
        
        OnStageComplete?.Invoke();
        
        // 다음 스테이지로
        if (currentStageIndex < stages.Count - 1)
        {
            StartStage(currentStageIndex + 2);
        }
        else
        {
            Debug.Log("모든 스테이지 완료!");
        }
    }
    
    public float GetStageProgress()
    {
        if (CurrentStage.monsters.Count == 0) return 0;
        return (float)monstersKilledInStage / CurrentStage.monsters.Count;
    }
    
    private void CreateTestStages()
    {
        for (int i = 1; i <= 10; i++)
        {
            var stage = new StageData
            {
                stageNumber = i,
                stageName = $"숲의 입구 {i}",
                monsters = new List<MonsterData>(),
                clearGold = 100 * i,
                clearExp = 50 * i
            };
            
            // 몬스터 추가
            for (int j = 0; j < 5 + i; j++)
            {
                stage.monsters.Add(new MonsterData
                {
                    monsterName = j < 5 ? "슬라임" : "고블린",
                    maxHp = 100 * i,
                    attackPower = 10 * i,
                    goldReward = 10 * i,
                    expReward = 5 * i
                });
            }
            
            stages.Add(stage);
        }
    }
    
    [Title("디버그")]
    [Button("현재 몬스터 즉사", ButtonSizes.Large)]
    [GUIColor(0.8f, 0.3f, 0.3f)]
    private void DebugKillMonster()
    {
        if (CurrentMonster != null)
        {
            DamageMonster(CurrentMonster.CurrentHp);
        }
    }
    
    [Button("스테이지 스킵", ButtonSizes.Large)]
    [GUIColor(0.3f, 0.3f, 0.8f)]
    private void DebugSkipStage()
    {
        CompleteStage();
    }
}
