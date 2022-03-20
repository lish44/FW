/// <summary>
/// 单例基类 不继承mono
/// </summary>
/// <typeparam name="T"></typeparam>
public class SingletonBase<T> where T : SingletonBase<T>, new()
{
    private static T instance;
    public bool IsFinish;
    public static T Ins
    {
        get
        {
            if (instance == null)
            {
                instance = new T();
                instance.Init();
            }
            return instance;
        }
    }
    public virtual void Init() { } // 实例化时调用 有且仅有一次
}