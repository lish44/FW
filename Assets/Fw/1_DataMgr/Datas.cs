/**
2020-8-10 rehma
所有表对应的属性 写在这个类
表名和类命保持一致
*/
using System.Collections.Generic;
namespace FW
{
    public delegate void CallBack();
    public delegate void CallBack<T>(T arg0);
    public delegate void CallBack<T, T1>(T arg0, T1 arg1);
    public delegate void CallBack<T, T1, T2>(T arg0, T1 arg1, T2 args2);
    public delegate void CallBack<T, T1, T2, T3>(T arg0, T1 arg1, T2 args2, T3 args3);
    public delegate void CallBack<T, T1, T2, T3, T4>(T arg0, T1 arg1, T2 args2, T3 args3, T4 args4);
    public delegate void CallBack<T, T1, T2, T3, T4, T5>(T arg0, T1 arg1, T2 args2, T3 args3, T4 args4, T5 args5);



    public delegate bool CallbackBool();
    public delegate bool CallbackBool<T>(T arg0);
    public delegate bool CallbackBool<T, T1>(T arg0, T1 arg1);

    public delegate object CallbackObj();
    public delegate object CallbackObj<T>(T arg0);
    public delegate object CallbackObj<T, T1>(T arg0, T1 arg1);

    public delegate R RCallback<R>();
    public delegate R RCallback<R, T>(T arg);
    public delegate R RCallback<R, T, T1, T2, T3, T4, T5>(T arg0, T1 arg1, T2 args2, T3 args3, T4 args4, T5 args5);

    public interface IGetPath
    {
        string GetPath { get; }
        string GetName { get; }
        void SetInof(string _str1, string _str2);
    }
    public class PathMappingBase : IGetPath
    {
        private string Name { get; set; }
        private string Path { get; set; }
        public string GetName => Name;
        public string GetPath => Path;
        public PathMappingBase() { }
        public PathMappingBase(string _name, string _path)
        {
            SetInof(_name, _path);
        }

        public void SetInof(string _str1, string _str2)
        {
            this.Name = _str1;
            this.Path = _str2;
        }
    }
    public class AudioPath : PathMappingBase
    {
        public AudioPath()
        {
        }

        public AudioPath(string _name, string _path) : base(_name, _path)
        {
        }
    }

    public class PrefabPath : PathMappingBase
    {
        public PrefabPath()
        {
        }

        public PrefabPath(string _name, string _path) : base(_name, _path)
        {
        }
    }

    public class SpritePath : PathMappingBase
    {
        public SpritePath()
        {
        }

        public SpritePath(string _name, string _path) : base(_name, _path)
        {
        }
    }



    public class RoleBaseAttributeType
    {
        public int Lv { get; set; }
        public int HP { get; set; }
        public int Exp { get; set; }
        public int Money { get; set; }
        public int Rep { get; set; }
        public int Step { get; set; }
        public IEnumerator<int> GetEnumerator()
        {
            yield return Lv;
            yield return HP;
            yield return Exp;
            yield return Money;
            yield return Rep;
            yield return Step;
        }
    }

}