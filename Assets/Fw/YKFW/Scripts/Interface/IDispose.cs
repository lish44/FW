//-----------------------------------------------------------------------
//| Autor:Adam                                                             |
//-----------------------------------------------------------------------

using System;
using UnityEngine;
namespace FW
{
    public interface IDispose
    {
        void SetParent(Transform parent);
        void Dispose();

    }
}