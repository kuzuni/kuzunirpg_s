namespace RPG.Enhancement
{
    /// <summary>
    /// ������ ���� ������ �������̽�
    /// </summary>
    public interface ILevelable
    {
        int Level { get; set; }
        int MaxLevel { get; }
        float GetLevelBonus();
    }

    /// <summary>
    /// ��ȭ ������ ������ �������̽�
    /// </summary>
    public interface IEnhanceableItem
    {
        bool CanEnhance();
        void Enhance();
        int GetEnhanceCost();
    }
}