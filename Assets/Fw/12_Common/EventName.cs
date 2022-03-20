/// <summary>
/// 消息事件名
/// </summary>
public enum EventName : short
{

    /// <summary>
    /// loading界面
    /// </summary>
    LOADING,
    /// <summary>
    /// loading界面加载完成
    /// </summary>
    LOADINGFINISH,
    /// <summary>
    /// loading界面加载完成打开一个panel
    /// </summary>
    LOADEDOPENPANEL,
    /// <summary>
    /// 按键按下
    /// </summary>
    KEY_DOWN,
    /// <summary>
    /// 按键持续按下
    /// </summary>
    KEY,
    /// <summary>
    /// 按键抬起
    /// </summary>
    KEY_UP,
    /// <summary>
    /// 鼠标按键按下
    /// </summary>
    MOUSE_DOWN,

    /// <summary>
    /// 鼠标按键持续按下
    /// </summary>
    MOUSE,
    /// <summary>
    /// 鼠标按键抬起
    /// </summary>
    MOUSE_UP,
    /// <summary>
    /// 鼠标中间滑动
    /// </summary>
    MOUSE_SCROLLWHEEL,

}