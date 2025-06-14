namespace RPG.Enhancement
{
    /// <summary>
    /// 레벨을 가진 아이템 인터페이스
    /// </summary>
    public interface ILevelable
    {
        int Level { get; set; }
        int MaxLevel { get; }
        float GetLevelBonus();
    }

    /// <summary>
    /// 강화 가능한 아이템 인터페이스
    /// </summary>
    public interface IEnhanceableItem
    {
        bool CanEnhance();
        void Enhance();
        int GetEnhanceCost();
    }
}