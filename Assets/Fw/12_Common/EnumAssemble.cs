/**
    枚举集合
*/

/// <summary>
///  游戏运行状态
/// </summary>
public enum E_GameState
{
    Ongoing,
    Stop,
    Over,
    Watch
}

/// <summary>
/// UI 层级
/// </summary>
public enum E_UI_layer
{
    Bot,
    Mid,
    Top,
    System,
}


/// <summary>
/// 角色状态 
/// </summary>
public enum E_FSM_State_Type
{
    PlayerIdle,
    PlayerVictory,
    PlayerJump,
    PlayerLook,
    PlayerDefeat,
    PlayerDead,
    None
}