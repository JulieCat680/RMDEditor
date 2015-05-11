using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RMDEditor.Node
{
    public unsafe interface IDataSurrogate
    {
        byte* Data { get; }

        int DataSize { get; }

        byte[] BufferedData { get; }
    }
}
