using UnityEngine;
// 체력 재생 시스템
namespace RPG.Player
{
    public class HealthRegenerationSystem : MonoBehaviour
    {
        [SerializeField] private PlayerStatus playerStatus;
        [SerializeField] private HealthSystem healthSystem;

        private float regenTimer = 0f;

        void Update()
        {
            if (playerStatus.CurrentHp < playerStatus.MaxHp && playerStatus.HpRegen > 0)
            {
                regenTimer += Time.deltaTime;

                if (regenTimer >= 1f)
                {
                    int regenAmount = Mathf.RoundToInt(playerStatus.HpRegen);
                    healthSystem.Heal(regenAmount);
                    regenTimer = 0f;
                }
            }
        }
    }

}
