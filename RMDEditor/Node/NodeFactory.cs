using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using RMDEditor.Pac;
using RMDEditor.Rmd;
using RMDEditor.FLW0;

namespace RMDEditor.Node
{
    public static class NodeFactory
    {
        public static FileDataNode FromFile(string path, bool simplifyNode)
        {
            FileDataNode result = null;

            if (path.EndsWith(".Rmd", true, CultureInfo.CurrentCulture))
                result = TryFromRmdFile(path, simplifyNode);
            else if (path.EndsWith(".Pac", true, CultureInfo.CurrentCulture))
                result = TryFromPacFile(path);
            else if (path.EndsWith(".bf", true, CultureInfo.CurrentCulture))
                result = TryFromFlowFile(path);

            if (result == null)
                result = TryFromRmdFile(path, simplifyNode);
            if (result == null)
                result = TryFromPacFile(path);
            if (result == null)
                result = TryFromFlowFile(path);

            if (result == null)
                result = TryFromRawFileData(path);

            return result;
        }

        public static FileDataNode FromRmdFile(string path, bool simplifyNode)
        {
            RmdFileElement element = Element.FromFile(path);

            if (simplifyNode)
                return FromElement(element.ToRmdElement());
            else
                return FromElement(element);
        }

        public static Flw0Node FromFlowFile(string path)
        {
            Flw0 flw0Data = Flw0.FromFile(path);
            return new Flw0Node(Path.GetFileName(path), flw0Data);
        }

        public static PacFileNode FromPacFile(string path)
        {
            PacData[] pacData = PacData.FromFile(path);
            return FromPacFileData(Path.GetFileName(path), pacData);
        }

        public static RawFileDataNode FromRawFileData(string path)
        {
            byte[] data = File.ReadAllBytes(path);
            return new RawFileDataNode(Path.GetFileName(path), data);
        }

        public static RmdNode FromElement(Element elem)
        {
            if (elem is DataElement)
                return FromElement(elem as DataElement);
            else
                return FromElement(elem as ContainerElement);
        }

        public static RmdNode FromElement(DataElement elem)
        {
            switch (elem.Type)
            {
                case 0x116: return new DataNode(elem) { Text = "Rigging Data" };
                case 0x11F: return new DataNode(elem) { Text = "Custom Properties" };
                default: return new DataNode(elem);
            }
        }

        public static RmdNode FromElement(ContainerElement elem)
        {
            try
            {
                switch (elem.Type)
                {
                    case 0x06: return new TextureRefNode(elem);
                    case 0x07: return new MaterialNode(elem);
                    case 0x08: return new MaterialSetNode(elem);
                    case 0x0F: return new MeshNode(elem);
                    case 0x10: return new SceneDataNode(elem);
                    case 0x14: return new DrawCallNode(elem);
                    case 0x15: return new TextureNode(elem);
                    case 0x16: return new TextureSetNode(elem);
                    case 0x1A: return new MeshSetNode(elem);
                    
                    default: return new ContainerNode(elem);
                }
            }
            catch
            {
                return new ContainerNode(elem);
            }
        }

        public static RmdFileNode FromElement(RmdFileElement elem)
        {
            return new RmdFileNode(elem);
        }

        public static PacDataNode FromPacFileData(PacData data)
        {
            return new PacDataNode(data);
        }

        public static PacFileNode FromPacFileData(string name, PacData[] data)
        {
            return new PacFileNode(name, data);
        }



        public static FileDataNode TryFromRmdFile(string path, bool simplifyNode)
        {
            try
            {
                return FromRmdFile(path, simplifyNode);
            }
            catch
            {
                return null;
            }
        }

        public static FileDataNode TryFromPacFile(string path)
        {
            try
            {
                return FromPacFile(path);
            }
            catch
            {
                return null;
            }
        }

        public static FileDataNode TryFromFlowFile(string path)
        {
            try
            {
                return FromFlowFile(path);
            }
            catch
            {
                return null;
            }
        }

        public static FileDataNode TryFromRawFileData(string path)
        {
            try
            {
                return FromRawFileData(path);
            }
            catch
            {
                return null;
            }
        }
    }
}
