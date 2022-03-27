using UnityEngine;

/** Ex:
    在需要用按键的地方的start里 添加监听即可
    FW.Evencenter.Ins.AddEventListener<KeyCode> (EventName.KEY_DOWN, KeyDown);

    void KeyDown (KeyCode key) {
        switch (key) {
            case KeyCode.W:
                // TODO::
            break;
        }
    }
*/
namespace FW
{
    public class InputMgr : SingletonBase<InputMgr>
    {

        private bool m_isStart = false;

        public override void Init()
        {
        }
        public void Init(CallBack _callback = null)
        {
            MonoMgr.Ins.AddUpdateListener(Update);
            _callback?.Invoke();
        }

        void Update()
        {
            if (!m_isStart) return;
            CheckKeyCode(KeyCode.W);
            CheckKeyCode(KeyCode.A);
            CheckKeyCode(KeyCode.S);
            CheckKeyCode(KeyCode.D);
            CheckKeyCode(KeyCode.Space);
            CheckKeyCode(KeyCode.Escape);
            CheckMouse(0);
            CheckMouse(1);
            CheckMouse(2);
            CheckMouseScrollWheel();
        }

        /// <summary>
        /// 控制按键开始关闭
        /// </summary>
        /// <param name="_isOpen">ture : 开 false : 关</param>
        public void StartOrEndCheck(bool _isOpen)
        {
            m_isStart = _isOpen;
        }

        void CheckKeyCode(KeyCode _k)
        {
            if (Input.GetKeyDown(_k))
                Evencenter.Ins.EventTrigger<KeyCode>(EventName.KEY_DOWN, _k);

            if (Input.GetKey(_k))
                Evencenter.Ins.EventTrigger<KeyCode>(EventName.KEY, _k);

            if (Input.GetKeyUp(_k))
                Evencenter.Ins.EventTrigger<KeyCode>(EventName.KEY_UP, _k);

        }

        void CheckMouse(int _type)
        {
            if (Input.GetMouseButtonDown(_type))
                Evencenter.Ins.EventTrigger<int>(EventName.MOUSE_DOWN, _type);

            if (Input.GetMouseButton(_type))
                Evencenter.Ins.EventTrigger<int>(EventName.MOUSE, _type);

            if (Input.GetMouseButtonUp(_type))
                Evencenter.Ins.EventTrigger<int>(EventName.MOUSE_UP, _type);

        }

        void CheckMouseScrollWheel()
        {
            float value = Input.GetAxis("Mouse ScrollWheel");
            if (value != 0)
                Evencenter.Ins.EventTrigger<float>(EventName.MOUSE_SCROLLWHEEL, value);
        }
    }
}