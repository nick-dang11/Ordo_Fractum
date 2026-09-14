using UnityEngine;

public class AttackTimelineDriver : MonoBehaviour
{
    [SerializeField] private WeaponHitbox weaponHitbox;

    private AttackData currentAttackData;
    private bool hitboxActive;

    private int currentAttackToken = 0;

    public int BeginAttack(AttackData attackData)
    {
        if(hitboxActive) weaponHitbox.DisableHitbox();
        currentAttackToken++;

        currentAttackData = attackData;
        hitboxActive = false;

        if (currentAttackData == null)
        {
            Debug.LogWarning($"{name}: BeginAttack called with no AttackData.");

            return currentAttackToken;
        }

        Debug.Log(
            $"AttackTimelineDriver beginning {currentAttackData.name}. " +
            $"Start = {currentAttackData.StartNormalized:F3}, " +
            $"End = {currentAttackData.EndNormalized:F3}, " +
            $"Token = {currentAttackToken}."
            );

        return currentAttackToken;
    }

    public void EvaluateAttack(int attackToken, float normalizedTime, float damage)
    {
        if (attackToken != currentAttackToken) return;
        if (currentAttackData == null) return;

        bool shouldHitboxBeActive = 
            normalizedTime >= currentAttackData.StartNormalized && 
            normalizedTime < currentAttackData.EndNormalized;

        if(shouldHitboxBeActive && !hitboxActive)
        {
            weaponHitbox.EnableHitbox(damage);
            hitboxActive = true;

            Debug.Log($"AttackTimelineDriver successfully enabled hitbox at {normalizedTime:F3}");
        }
        else if(!shouldHitboxBeActive && hitboxActive)
        {
            weaponHitbox.DisableHitbox();
            hitboxActive = false;

            Debug.Log($"AttackTimelineDriver successfully disabled hitbox at {normalizedTime:F3}");
        }
    }

    public void EndAttack(int attackToken)
    {
        if (attackToken != currentAttackToken) return;
        if (hitboxActive) weaponHitbox.DisableHitbox();

        hitboxActive= false;
        currentAttackData = null;
        Debug.Log("AttackTimelineDriver attack ended.");
    }
}
