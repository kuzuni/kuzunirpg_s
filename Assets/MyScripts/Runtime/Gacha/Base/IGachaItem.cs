using UnityEngine;

namespace RPG.Gacha.Base
{
    // 뽑기 가능한 아이템 인터페이스
    public interface IGachaItem
    {
        string ItemName { get; }
        Sprite Icon { get; }
        int GetRarityLevel(); // Enum을 int로 변환
        string GetRarityName();
        Color GetRarityColor();
    }
}
