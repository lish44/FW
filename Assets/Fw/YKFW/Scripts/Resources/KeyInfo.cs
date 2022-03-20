using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FW
{
    public class KeyInfo
    {
        public string path;
        public Type type;

        public KeyInfo(string path, Type type)
        {
            this.path = path;
            this.type = type;
        }

    }
}
