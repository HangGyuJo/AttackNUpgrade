using UnityEngine;

[CreateAssetMenu(
    fileName = "AbilityData",
    menuName = "AttackNUpgrade/Ability Data"
)]
public class AbilityData : ScriptableObject
{
    [field: SerializeField] public string AbilityId { get; private set; }
    [field: SerializeField] public string DisplayName { get; private set; }
    [field: SerializeField, TextArea] public string Description { get; private set; }
    [field: SerializeField] public Sprite Icon { get; private set; }
    [field: SerializeField] public AbilityType AbilityType { get; private set; }
    [field: SerializeField] public float Value { get; private set; }

    public void ApplyToServer(PlayerStats stats)
    {
        if (stats == null || !stats.IsServer)
        {
            return;
        }

        switch (AbilityType)
        {
            case AbilityType.MaxHealth:
                stats.AddMaxHealthServer(Mathf.RoundToInt(Value));
                break;

            case AbilityType.AttackDamage:
                stats.AddAttackDamageServer(Mathf.RoundToInt(Value));
                break;

            case AbilityType.MoveSpeedPercent:
                stats.AddMoveSpeedPercentServer(Value);
                break;

            case AbilityType.AttackSpeedPercent:
                stats.AddAttackSpeedPercentServer(Value);
                break;

            case AbilityType.ProjectileSpeedPercent:
                stats.AddProjectileSpeedPercentServer(Value);
                break;
        }
    }
}