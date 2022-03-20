using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using Object = UnityEngine.Object;


namespace FW
{
    /// <summary>
    /// 缓存池管理器
    /// </summary>
    public class FPoolManager
    {
        /// <summary>
        /// 池列表
        /// </summary>
        Dictionary<string, GameObject> Pools = new Dictionary<string, GameObject>();


        //缓冲上限
        private int NodeLimit = 10;

        private const string DEFAULTPOOL = "defaultPool";


        /// <summary>
        /// 建立池
        /// </summary>
        /// <param name="poolName">池名字</param>
        public void CreatePool(string poolName)
        {
            GameObject pool = new GameObject(poolName);
            pool.transform.parent = FResourcesManager.Inst.transform;
            pool.transform.localPosition = new Vector3(0, 99999, 0);
            pool.transform.localEulerAngles = Vector3.zero;
            pool.transform.localScale = Vector3.one;
            Pools.Add(poolName, pool);

        }

        /// <summary>
        /// 销毁池
        /// </summary>
        public void DestroyPool(string poolName)
        {

            GameObject.Destroy(Pools[poolName]);
            Pools.Remove(poolName);

        }


        /// <summary>
        /// 清空池
        /// </summary>
        public void CleanPool(string poolName = DEFAULTPOOL)
        {
            if (Pools.Count == 0)
                return;

            List<GameObject> objArray = new List<GameObject>();
            foreach (Transform each in Pools[poolName].transform)
            {
                objArray.Add(each.gameObject);
            }


            for (int i = 0; i < objArray.Count; i++)
            {

                GameObject.DestroyImmediate(objArray[i]);

            }



        }

        /// <summary>
        /// 从池中获得物体
        /// </summary>
        ///  <param name="path">读取路径</param>
        /// <param name="poolName">目标池</param>
        /// <param name="handle">回调</param>
        public void LoadObjectFromPool(string path, CallBack<object> handle, string poolName = DEFAULTPOOL)
        {

        }

        /// <summary>
        /// 缓冲物体
        /// </summary>
        /// <param name="path">缓冲物体的路径</param>
        /// <param name="handle">回调</param>
        /// <param name="poolName">目标池</param>
        public void CacheObjToPool(string path, CallBack<Object> handle, string poolName = DEFAULTPOOL)
        {

            FResourcesManager.Inst.LoadObject(path, null);


        }

        /// <summary>
        /// 是否存在对应缓冲池
        /// </summary>
        /// <param name="poolName"></param>
        /// <returns></returns>
        private bool hasPool(string poolName)
        {

            return true;
        }



    }

}

