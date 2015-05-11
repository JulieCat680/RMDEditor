using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RMDEditor.FLW0
{
    [DebuggerDisplay("Name={Name}, Count={Data.Count}")]
    public class FlowTrack
    {
        public string Name { get { return _Name; } set { _Name = value; } }
        private string _Name = null;

        public List<int> Data { get { return _Data; } set { _Data = value; } }
        private List<int> _Data = null;

        public List<KeyValuePair<string, int>> Labels { get { return _Labels; } }
        private List<KeyValuePair<string, int>> _Labels = null;

        public FlowTrack()
        {
            _Name = null;
            _Data = new List<int>();
            _Labels = new List<KeyValuePair<string, int>>();
        }

        public FlowTrack(string name)
        {
            _Name = name;
            _Data = new List<int>();
            _Labels = new List<KeyValuePair<string, int>>();
        }
    }
}
