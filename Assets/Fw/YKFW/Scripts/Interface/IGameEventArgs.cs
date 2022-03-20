//-----------------------------------------------------------------------
//| Autor:Adam                                                             |
//-----------------------------------------------------------------------

using System;
namespace FW
{
    public interface IGameEventArgs
    {
        bool IsCancelDefaultAction
        {
            get;
            set;
        }


    }
}