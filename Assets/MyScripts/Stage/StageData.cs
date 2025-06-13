using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
// 스테이지 데이터
[Serializable]
public class StageData
{
    [HorizontalGroup("Stage", 0.3f)]
    [VerticalGroup("Stage/Info")]
    public int stageNumber;
    
    [VerticalGroup("Stage/Info")]
    public string stageName;
    
    [VerticalGroup("Stage/Monsters")]
    [TableList]
    public List<MonsterData> monsters;
    
    [VerticalGroup("Stage/Rewards")]
    [LabelText("클리어 골드")]
    public int clearGold;
    
    [VerticalGroup("Stage/Rewards")]
    [LabelText("클리어 경험치")]
    public int clearExp;
}
