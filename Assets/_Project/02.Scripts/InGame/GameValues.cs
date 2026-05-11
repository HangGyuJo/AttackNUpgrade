/// <summary>
/// 게임 페이즈
/// </summary>
public enum GamePhase
{
    None = 0,
    Preparing = 1,
    Farming = 2,
    BossWarning = 3,
    BossInvasion = 4,
    RewardSelection = 5,
    Intermission = 6,
    Result = 7
}

/// <summary>
/// 게임 결과
/// </summary>
public enum GameResult
{
    None = 0,
    Victory = 1,
    Defeat = 2
}