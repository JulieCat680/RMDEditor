using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using RMDEditor.Rmd;

namespace RMDEditor.Node
{
    public interface IContainerNode
    {
        IContainerElement ContainerElement { get; }
        
        IContainerNode RmdRoot { get; }
        IContainerNode RmdParent { get; }
    }
}
