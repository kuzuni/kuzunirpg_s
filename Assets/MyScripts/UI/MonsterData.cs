using System;
using Sirenix.OdinInspector;
// 몬스터 데이터
[Serializable]
public class MonsterData
{
    [TableColumnWidth(100)]
    public string monsterName;
    
    [TableColumnWidth(80)]
    public int maxHp;
    
    [TableColumnWidth(80)]
    public int attackPower;
    
    [TableColumnWidth(80)]
    public int goldReward;
    
    [TableColumnWidth(80)]
    public int expReward;
}
