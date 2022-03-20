using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SingletonMono<T> : MonoBehaviour where T : SingletonMono<T>
{
    private static T instance;

    public static T Ins
    {
        get
        {

            if (instance == null)
            {

                instance = FindObjectOfType<T>();

                if (instance == null)
                {

                    var managerGo = GameObject.Find("MgrRoots");

                    if (managerGo == null)
                    {

                        managerGo = new GameObject("MgrRoots");

                    }

                    instance = managerGo.AddComponent<T>();
                    DontDestroyOnLoad(managerGo);

                }

                instance.Init();

            }

            return instance;
        }
    }

    public virtual void Init() { }
}