using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using RMDEditor.Node;
using RMDEditor.Rmd;
using RMDEditor.Pac;

namespace RMDEditor
{
    public partial class Form1 : Form
    {
        private Version _Version = null;
        private string _Title = null;
        private string _Path = null;

        public Form1()
        {
            InitializeComponent();

            _Version = System.Reflection.Assembly.GetEntryAssembly().GetName().Version;
            _Title = string.Format("RMD Editor v{0}.{1}", _Version.Major, _Version.Minor);

            Text = _Title;
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            openFileDialog1.FileName = "";
            if (openFileDialog1.ShowDialog() != DialogResult.OK)
                return;

            OpenFile(openFileDialog1.FileName);
        }

        private void closeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CloseFile();
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFile(_Path);
        }

        private void saveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (treeView1.Nodes.Count == 0 || !(treeView1.Nodes[0] is FileDataNode))
                return;

            FileDataNode node = treeView1.Nodes[0] as FileDataNode;

            saveFileDialog1.FileName = node.Text;
            if (saveFileDialog1.ShowDialog() != DialogResult.OK)
                return;

            SaveFile(saveFileDialog1.FileName);
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            ResetData();

            ISurrogateProvider surrogateProvider = treeView1.SelectedNode as ISurrogateProvider;

            if (surrogateProvider == null)
                return;

            
            object surrogate = surrogateProvider.SurrogateObject;

            propertyGrid1.SelectedObject = surrogate;

            if (surrogate is IDataSurrogate)
            {
                FileDataNode node = treeView1.SelectedNode as FileDataNode;
                ShowHexPanel(surrogate as IDataSurrogate, node.GetFileOffset() + node.GetHeaderSize());
            }

            if (surrogate is IImageSurrogate)
                ShowImageData(surrogate as IImageSurrogate);
        }

        private void pageControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            ToolStripButton[] buttons = toolStrip1.Items.OfType<ToolStripButton>().ToArray();
            for (int i = 0; i < buttons.Length; i++)
                buttons[i].Checked = (i == pageControl1.SelectedIndex);
        }

        private void toolStripButton_Click(object sender, EventArgs e)
        {
            int selectedIndex = toolStrip1.Items.IndexOf(sender as ToolStripButton);

            if (selectedIndex != -1)
            {
                pageControl1.SelectedIndex = selectedIndex;
            }
        }

        private void copyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Control source = ((sender as ToolStripMenuItem).Owner as ContextMenuStrip).SourceControl;
            PictureBox pictureBox = source as PictureBox;

            if (pictureBox != null)
                Clipboard.SetImage(pictureBox.Image);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            FileDataNode node = treeView1.SelectedNode as FileDataNode;

            if (node != null)
            {
                IDataSurrogate dataSurrogate = node.SurrogateObject as IDataSurrogate;
                int offset = node.GetFileOffset() + node.GetHeaderSize();
                ShowHexData(dataSurrogate, offset);
            }

            button1.Visible = false;
        }

        private unsafe void OpenFile(string path)
        {
            CloseFile();

            _Path = path;

            treeView1.Nodes.Add(NodeFactory.FromFile(path, false));
            closeToolStripMenuItem.Enabled = true;
            saveToolStripMenuItem.Enabled = true;
            saveAsToolStripMenuItem.Enabled = true;
            Text = string.Format("{0} - {1}", _Title, path);
        }

        private void CloseFile()
        {
            _Path = null;

            treeView1.Nodes.Clear();
            ResetData();

            closeToolStripMenuItem.Enabled = false;
            saveToolStripMenuItem.Enabled = false;
            saveAsToolStripMenuItem.Enabled = false;
            Text = _Title;
        }

        private void SaveFile(string path)
        {
            if (treeView1.Nodes.Count == 0 || !(treeView1.Nodes[0] is FileDataNode))
                return;

            FileDataNode node = treeView1.Nodes[0] as FileDataNode;
            node.Export(path);

            _Path = path;
            Text = string.Format("{0} - {1}", _Title, path);
        }

        private void ResetData()
        {
            button1.Visible = false;
            textBox1.Text = "";
            pictureBox1.Image = null;
            pictureBox2.Image = null;
            toolStripButton1.Visible = false;
            toolStripButton2.Visible = false;
            toolStripButton3.Visible = false;
            propertyGrid1.SelectedObject = null;
        }

        private void ShowHexPanel(IDataSurrogate surrogate, int offset)
        {
            toolStripButton1.Visible = true;
            pageControl1.SelectedTab = tabPage1;

            if (surrogate.DataSize < 0x10000)
                ShowHexData(surrogate, offset);
            else
                button1.Visible = true;
        }

        private void ShowHexData(IDataSurrogate surrogate, int offset)
        {
            byte[] data = surrogate.BufferedData;
            string hexView = "";
            string hexData = "";
            string asciiData = "";

            for (int i = 0; i < data.Length; i++)
            {
                if (i % 2 == 1)
                    hexData += string.Format("{0:X2} ", data[i]);
                else
                    hexData += string.Format("{0:X2}", data[i]);

                if (char.IsControl((char)data[i]))
                    asciiData += '.';
                else
                    asciiData += (char)data[i];

                if ((i + 1) % 0x10 == 0 || i == data.Length - 1)
                {
                    string line = string.Format("{0:X8}: ", offset);
                    line += hexData;
                    line = line.PadRight(59);
                    line += asciiData;
                    line += Environment.NewLine;
                    hexView += line;

                    offset += 0x10;
                    hexData = "";
                    asciiData = "";
                }
            }

            textBox1.Text = hexView;
        }

        private void ShowImageData(IImageSurrogate surrogate)
        {
            toolStripButton2.Visible = true;
            toolStripButton3.Visible = true;
            pageControl1.SelectedTab = tabPage2;

            pictureBox1.Image = surrogate.Image;
            pictureBox2.Image = null;
            if (surrogate.PaletteImage != null)
            {
                pictureBox2.Image = new Bitmap(128, 128);
                using (Graphics g = Graphics.FromImage(pictureBox2.Image))
                {
                    g.InterpolationMode = InterpolationMode.NearestNeighbor;
                    g.PixelOffsetMode = PixelOffsetMode.Half;
                    g.DrawImage(surrogate.PaletteImage, new Rectangle(0, 0, 128, 128));
                }
            }
        }
    }
}
