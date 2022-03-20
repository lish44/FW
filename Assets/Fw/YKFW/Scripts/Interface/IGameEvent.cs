//-----------------------------------------------------------------------
//| Autor:Adam                                                             |
//-----------------------------------------------------------------------

using System;
namespace FW
{
    public interface IGameEvent
    {
        void RemoveEvent(Enum type);
        void ClearEvent();
        void DispatchEvent(Enum type, params object[] args);
        void DispatchAsyncEvent(Enum type, params object[] args);
        bool HasEvent(Enum type);
        void Dispose();
        bool IsDispose
        {
            get;
        }

    }
}