using UnityEngine;
using Sirenix.OdinInspector;
// 자동 전투 시스템
public class AutoBattleSystem : MonoBehaviour
{
    [Title("자동 전투 설정")]
    [SerializeField] private bool isAutoEnabled = true;
    [SerializeField] private PlayerController playerController;
    [SerializeField] private StageManager stageManager;
    
    [ShowInInspector, ReadOnly]
    private float attackTimer = 0f;
    
    [ShowInInspector, ReadOnly]
    [ProgressBar(0, "@GetAttackCooldown()", ColorGetter = "GetAttackProgressColor")]
    private float AttackProgress => attackTimer;
    
    private void Start()
    {
        if (playerController == null)
            playerController = FindObjectOfType<PlayerController>();
        
        if (stageManager == null)
            stageManager = FindObjectOfType<StageManager>();
    }
    
    private void Update()
    {
        if (!isAutoEnabled || stageManager?.CurrentMonster == null) return;
        
        attackTimer += Time.deltaTime;
        
        if (attackTimer >= GetAttackCooldown())
        {
            PerformAutoAttack();
            attackTimer = 0f;
        }
    }
    
    private void PerformAutoAttack()
    {
        if (playerController?.Attack == null) return;
        
        int damage = playerController.Attack.CalculateDamage();
        stageManager.DamageMonster(damage);
        
        // 데미지 텍스트 표시 (UI 시스템에서 처리)
        ShowDamageText(damage);
    }
    
    private float GetAttackCooldown()
    {
        return playerController?.Attack?.GetAttackCooldown() ?? 1f;
    }
    
    private Color GetAttackProgressColor(float value)
    {
        float progress = GetAttackCooldown() > 0 ? value / GetAttackCooldown() : 0;
        return Color.Lerp(Color.red, Color.green, progress);
    }
    
    private void ShowDamageText(int damage)
    {
        // TODO: 실제 데미지 텍스트 UI 구현
        Debug.Log($"데미지: {damage}");
    }
    
    [Button("자동 전투 토글", ButtonSizes.Large)]
    [GUIColor("GetToggleColor")]
    private void ToggleAutoBattle()
    {
        isAutoEnabled = !isAutoEnabled;
        Debug.Log($"자동 전투: {(isAutoEnabled ? "ON" : "OFF")}");
    }
    
    private Color GetToggleColor()
    {
        return isAutoEnabled ? new Color(0.3f, 0.8f, 0.3f) : new Color(0.8f, 0.3f, 0.3f);
    }
}
