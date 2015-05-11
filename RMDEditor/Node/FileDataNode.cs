using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RMDEditor.Node
{
    public abstract class FileDataNode : TreeNode, ISurrogateProvider
    {
        public FileDataNode FileDataRoot { get { return (FileDataParent != null ? FileDataParent.FileDataRoot : this); } }
        public FileDataNode FileDataParent { get { return Parent as FileDataNode; } }

        public int FileOffset { get { return GetFileOffset(); } }

        public int HeaderSize { get { return GetHeaderSize(); } }
        public int SizeInMemory { get { return GetSizeInMemory(); } }

        public virtual object SurrogateObject { get { return _SurrogateObject; } }
        protected SurrogateDataObject _SurrogateObject = null;

        public FileDataNode() : this(null) { }
        public FileDataNode(string text)
            : base(text)
        {
            _SurrogateObject = new SurrogateDataObject(this);

            ContextMenuStrip = new ContextMenuStrip();
            ContextMenuStrip.Items.Add(new ToolStripMenuItem("&Export", null, Export_Click) { Name = "Export" });
            ContextMenuStrip.Items.Add(new ToolStripMenuItem("&Import", null, Import_Click) { Name = "Import" });
            ContextMenuStrip.Items.Add(new ToolStripMenuItem("I&nsert Above", null, InsertAbove_Click) { Name = "Insert Above" });
            ContextMenuStrip.Items.Add(new ToolStripMenuItem("&Replace", null, Replace_Click) { Name = "Replace" });
            ContextMenuStrip.Items.Add(new ToolStripSeparator());
            ContextMenuStrip.Items.Add(new ToolStripMenuItem("Move &Up", null, MoveUp_Click) { Name = "Move Up" });
            ContextMenuStrip.Items.Add(new ToolStripMenuItem("Move D&own", null, MoveDown_Click) { Name = "Move Down" });
            ContextMenuStrip.Items.Add(new ToolStripSeparator());
            ContextMenuStrip.Items.Add(new ToolStripMenuItem("&Delete", null, Delete_Click) { Name = "Delete" });
            ContextMenuStrip.Opening += ContextMenuStrip_Opening;
        }



        public abstract int GetHeaderSize();

        public abstract int GetSizeInMemory();

        public virtual int GetFileOffset()
        {
            return FileDataRoot.OffsetOfNode(this);
        }

        public virtual int OffsetOfNode(FileDataNode node)
        {
            if (node == this)
                return 0;

            int offset = GetHeaderSize();
            foreach (FileDataNode child in Nodes.OfType<FileDataNode>())
            {
                if (child == node)
                    return offset;

                int childOffset = child.OffsetOfNode(node);
                if (childOffset != -1)
                    return offset + childOffset;

                offset += child.GetSizeInMemory();
            }

            return -1;
        }

        #region Node Operations

        public FileDataNode Import(string path)
        {
            FileDataNode node = FromFile(path);

            Import(node);

            return node;
        }

        public void Import(FileDataNode node)
        {
            Insert(Nodes.Count, node);
        }

        public byte[] Export()
        {
            return ToFile();
        }

        public void Export(string path)
        {
            File.WriteAllBytes(path, ToFile());
        }


        public FileDataNode Insert(int index, string path)
        {
            FileDataNode node = FromFile(path);

            Insert(index, node);

            return node;
        }

        public virtual void Insert(int index, FileDataNode node)
        {
            Nodes.Insert(index, node);
        }

        public new void Remove()
        {
            if (FileDataParent != null)
                FileDataParent.Remove(this);
            else if (Parent != null)
                Parent.Nodes.Remove(this);
            else if (TreeView != null && TreeView.Nodes.Contains(this))
                TreeView.Nodes.Remove(this);
        }

        public virtual void Remove(FileDataNode node)
        {
            Nodes.Remove(node);
        }



        public FileDataNode InsertAbove(string path)
        {
            FileDataNode node = FromFile(path);

            InsertAbove(node);

            return node;
        }

        public void InsertAbove(FileDataNode node)
        {
            if (FileDataParent != null)
            {
                int index = FileDataParent.Nodes.IndexOf(this);
                FileDataParent.Insert(index, node);
            }
            else if (Parent != null)
            {
                int index = Parent.Nodes.IndexOf(this);
                Parent.Nodes.Insert(index, node);
            }
            else if (TreeView != null && TreeView.Nodes.Contains(this))
            {
                int index = TreeView.Nodes.IndexOf(this);
                TreeView.Nodes.Insert(index, node);
            }
        }

        public FileDataNode Replace(string path)
        {
            FileDataNode node = FromFile(path);

            Replace(node);

            return node;
        }

        public void Replace(FileDataNode node)
        {
            InsertAbove(node);
            Remove();
        }

        public void Move(int index)
        {
            FileDataNode dataParent = FileDataParent;
            TreeNode nodeParent = Parent;
            TreeView treeParent = (TreeView.Nodes.Contains(this) ? TreeView : null);
            index = Math.Max(index, 0);

            Remove();

            if (dataParent != null)
            {
                index = Math.Min(index, dataParent.Nodes.Count);
                dataParent.Insert(index, this);
            }
            else if (nodeParent != null)
            {
                index = Math.Min(index, nodeParent.Nodes.Count);
                nodeParent.Nodes.Insert(index, this);
            }
            else if (treeParent != null)
            {
                index = Math.Min(index, treeParent.Nodes.Count);
                treeParent.Nodes.Insert(index, this);
            }
        }

        protected abstract FileDataNode FromFile(string path);

        protected abstract byte[] ToFile();

        #endregion

        #region Event Handlers


        private void Export_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveDlg = new SaveFileDialog();
            saveDlg.FileName = Text;
            if (saveDlg.ShowDialog() != DialogResult.OK)
                return;

            Export(saveDlg.FileName);
        }

        private void Import_Click(object sender, EventArgs e)
        {
            OpenFileDialog openDlg = new OpenFileDialog();
            if (openDlg.ShowDialog() != DialogResult.OK)
                return;

            FileDataNode node = Import(openDlg.FileName);

            if (node != null)
                node.TreeView.SelectedNode = node;
        }

        private void InsertAbove_Click(object sender, EventArgs e)
        {
            OpenFileDialog openDlg = new OpenFileDialog();
            if (openDlg.ShowDialog() != DialogResult.OK)
                return;

            FileDataNode node = InsertAbove(openDlg.FileName);

            if (node != null)
                node.TreeView.SelectedNode = node;
        }

        private void Replace_Click(object sender, EventArgs e)
        {
            OpenFileDialog openDlg = new OpenFileDialog();
            if (openDlg.ShowDialog() != DialogResult.OK)
                return;

            FileDataNode node = Replace(openDlg.FileName);

            if (node != null)
            {
                node.TreeView.SelectedNode = node;

                if (IsExpanded)
                    node.Expand();
            }
        }

        private void MoveUp_Click(object sender, EventArgs e)
        {
            int index = -1;

            if (FileDataParent != null)
                index = FileDataParent.Nodes.IndexOf(this);
            else if (Parent != null)
                index = Parent.Nodes.IndexOf(this);
            else if (TreeView != null && TreeView.Nodes.Contains(this))
                index = TreeView.Nodes.IndexOf(this);

            Move(index - 1);

            TreeView.SelectedNode = this;
        }

        private void MoveDown_Click(object sender, EventArgs e)
        {
            int index = -1;

            if (FileDataParent != null)
                index = FileDataParent.Nodes.IndexOf(this);
            else if (Parent != null)
                index = Parent.Nodes.IndexOf(this);
            else if (TreeView != null && TreeView.Nodes.Contains(this))
                index = TreeView.Nodes.IndexOf(this);

            Move(index + 1);

            TreeView.SelectedNode = this;
        }

        private void Delete_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("Delete this node?", "Delete Node", MessageBoxButtons.YesNo);
            if (result != DialogResult.Yes)
                return;

            Remove();
        }

        private void ContextMenuStrip_Opening(object sender, EventArgs e)
        {
            bool manipulatable = Parent != null;
            ContextMenuStrip.Items["Export"].Enabled = true;
            ContextMenuStrip.Items["Import"].Enabled = true;
            ContextMenuStrip.Items["Insert Above"].Enabled = manipulatable;
            ContextMenuStrip.Items["Replace"].Enabled = manipulatable;
            ContextMenuStrip.Items["Move Up"].Enabled = manipulatable;
            ContextMenuStrip.Items["Move Down"].Enabled = manipulatable;
            ContextMenuStrip.Items["Delete"].Enabled = manipulatable;
        }

        #endregion

        public class SurrogateDataObject
        {
            protected FileDataNode _FileDataNode = null;

            [TypeConverter(typeof(NodeDataConverter))]
            [Category("File")]
            public int FileOffset { get { return _FileDataNode.FileOffset; } }

            [Category("File")]
            [TypeConverter(typeof(NodeDataConverter))]
            public int Size { get { return _FileDataNode.SizeInMemory; } }

            public SurrogateDataObject(FileDataNode owner)
            {
                _FileDataNode = owner;
            }
        }
    }
}
