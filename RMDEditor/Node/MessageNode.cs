using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using RMDEditor.MSG1;

namespace RMDEditor.Node
{
    public class MessageNode : TreeNode, ISurrogateProvider
    {
        private MessageData _Data = null;

        public object SurrogateObject { get { return _SurrogateObject; } }
        private object _SurrogateObject = null;

        public MessageNode(MessageData data)
            : base(data.Name)
        {
            _Data = data;
            _SurrogateObject = new SurrogateDataObject(this);
        }

        public class SurrogateDataObject
        {
            private MessageNode _Owner = null;

            public string Name { get { return _Owner._Data.Name; } }

            public string[] Text { get { return _Owner._Data.Text.ToArray(); } }

            public string SpeakerName { get { return _Owner._Data.SpeakerName; } }

            public SurrogateDataObject(MessageNode owner)
            {
                _Owner = owner;
            }
        }
    }
}
