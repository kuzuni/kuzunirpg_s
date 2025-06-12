
// 몬스터 인스턴스
public class MonsterInstance
{
    public MonsterData data;
    public int CurrentHp { get; set; }
    public int MaxHp => data.maxHp;
    public string MonsterName => data.monsterName;
    
    public MonsterInstance(MonsterData monsterData)
    {
        data = monsterData;
        CurrentHp = MaxHp;
    }
}
