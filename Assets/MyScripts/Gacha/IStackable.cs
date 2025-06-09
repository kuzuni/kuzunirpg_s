// 스택 가능한 아이템 인터페이스
public interface IStackable
{
    int CurrentStack { get; set; }
    int MaxStack { get; }
    bool IsStackable { get; }
    
    // 스택 관리
    bool CanStack(int amount);
    int AddToStack(int amount); // 추가된 양 반환
    int RemoveFromStack(int amount); // 제거된 양 반환
}