using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RMDEditor.Node
{
    public interface IImageSurrogate
    {
        Bitmap Image { get; }
        Bitmap PaletteImage { get; }
    }
}
