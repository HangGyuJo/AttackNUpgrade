using UnityEngine;

public class EnemyReward : MonoBehaviour
{
    [field: SerializeField] public int CurrencyReward { get; private set; } = 1;
    [field: SerializeField] public int ExpReward { get; private set; } = 10;
}