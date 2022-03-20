using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FW
{
    public class PoolMgr : SingletonBase<PoolMgr>
    {

        private Dictionary<string, PoolData> m_poolDic;
        GameObject m_unActivePoolGo = null;
        GameObject m_activePoolGo = null;

        public override void Init()
        {
            m_poolDic = new Dictionary<string, PoolData>();
        }

        /// <summary>
        /// 拿
        /// </summary>
        /// <param name="_name">对象池名</param>
        /// <param name="_prefabName">预设体名</param>
        /// <returns>GameObject</returns>
        public void Get<T>(string _name, string _prefabName, System.Action<GameObject> _callback = null) where T : UnityEngine.Object
        {
            //第一次拿 没对应池子需创建
            if (!m_poolDic.ContainsKey(_name))
            {
                if (m_activePoolGo == null) m_activePoolGo = new GameObject("AcitvePool");
                if (m_unActivePoolGo == null) m_unActivePoolGo = new GameObject("UnActivePool");
                m_poolDic.Add(_name, new PoolData(_name, m_unActivePoolGo, m_activePoolGo));
            }

            GameObject go = m_poolDic[_name].PopGo();

            if (go == null)
            {
                go = FW.ResMgr.Ins.Load<GameObject>(_prefabName);
                m_poolDic[_name].m_poolList.Add(go);
                go.transform.parent = m_poolDic[_name].m_activefatherGo.transform;
            }
            //step3: 设置Go对象 位置 旋转 TODO::
            Utility.TransformOperation.ResetTransform(go.transform);

            //step4: 重置Go对象初始化 回调 需要继承IResetable接口
            // var item = go.GetComponent<IResetable>();
            // if (item != null) item.OnRest();
            foreach (var item in go.GetComponents<IResetable>())
            {
                item.OnRest();
            }
            _callback?.Invoke(go);
        }

        /// <summary>
        /// 回收
        /// </summary>
        /// <param name="_name">对象池名字</param>
        /// <param name="_go">回收对象</param>
        /// <param name="_delay">延时</param>
        public void Put(string _name, GameObject _go, float _delay = 0)
        {
            MonoMgr.Ins.StartCoroutine(CollectGoDelay(_name, _go, _delay));
        }

        private IEnumerator CollectGoDelay(string _name, GameObject _go, float _delay)
        {
            yield return new WaitForSeconds(_delay);
            m_poolDic[_name].PushGo(_go);
        }

        /// <summary>
        /// 清空对象池
        /// </summary>
        /// <param name="_name">对象池名</param>
        public void Clean(string _name)
        {
            if (!m_poolDic.ContainsKey(_name)) return;
            for (int i = m_poolDic[_name].m_poolList.Count - 1; i >= 0; i--)
                GameObject.Destroy(m_poolDic[_name].m_poolList[i]);

            GameObject.Destroy(m_poolDic[_name].m_activefatherGo);
            GameObject.Destroy(m_poolDic[_name].m_unActivefatherGo);
            m_poolDic[_name].m_poolList.Clear();
            m_poolDic.Remove(_name);
        }

        public void CleanAll()
        {
            foreach (string name in new List<string>(m_poolDic.Keys))
            {
                Clean(name);
            }
            GameObject.Destroy(m_activePoolGo);
            GameObject.Destroy(m_unActivePoolGo);
            m_poolDic.Clear();
        }
    }

    class PoolData
    {

        public GameObject m_unActivefatherGo = null;
        public GameObject m_activefatherGo = null;
        public List<GameObject> m_poolList;

        public PoolData(string _goName, GameObject _unActivePoolGo, GameObject _activePoolGo)
        {
            //设置衣柜的格子的名字
            m_unActivefatherGo = new GameObject(_goName + "_root");
            m_activefatherGo = new GameObject(_goName + "_root");

            //把格子和父节点“衣柜”关联
            m_unActivefatherGo.transform.parent = _unActivePoolGo.transform;
            m_activefatherGo.transform.parent = _activePoolGo.transform;

            m_poolList = new List<GameObject>();
        }

        public void PushGo(GameObject _go)
        {
            _go.SetActive(false);
            _go.transform.parent = m_unActivefatherGo.transform;
        }

        public GameObject PopGo()
        {
            var go = m_poolList.Find(g => !g.activeInHierarchy);
            if (go != null)
            {
                go.transform.parent = m_activefatherGo.transform;
                go.SetActive(true);
                return go;
            }
            return null;
        }

    }

    public interface IResetable
    {
        void OnRest();
    }
}