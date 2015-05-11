using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RMDEditor.Rmd
{
    public interface IContainerElement
    {
        List<Element> Elements { get; }

        int ElementCount { get; }
        int TotalElementCount { get; }

        int OffsetOfElement(Element elem);
    }
}
