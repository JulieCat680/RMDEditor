using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using RMDEditor.FLW0;

namespace RMDEditor.Node
{
    public class FlowTrackNode : TreeNode
    {
        public FlowTrack Data { get { return _Data; } }
        private FlowTrack _Data = null;

        public FlowTrackNode(FlowTrack data)
            : base(data.Name)
        {
            _Data = data;

            foreach (int cmd in _Data.Data)
            {
                Nodes.Add(new TreeNode(cmd.ToString("X8")));
            }
        }
    }
}
