using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using RMDEditor.MSG1;

namespace RMDEditor.Node
{
    public class Msg1Node : FileDataNode
    {
        private Msg1 _Data = null;

        public Msg1Node(Msg1 data)
            : base("Msg0")
        {
            _Data = data;

            foreach (MessageData message in _Data.Messages.Where(x => x != null))
                Nodes.Add(new MessageNode(message));
        }

        public override int GetHeaderSize()
        {
            return _Data.GetHeaderSize();
        }

        public override int GetSizeInMemory()
        {
            return _Data.GetSizeInMemory();
        }

        protected override FileDataNode FromFile(string path)
        {
            throw new NotImplementedException();
        }

        protected override byte[] ToFile()
        {
            throw new NotImplementedException();
        }
    }
}
