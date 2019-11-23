using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Interpreter
{
    public partial class Tree : Form
    {
        Pi p;
        public Tree(TreeView tree, Pi p)
        {
            this.p = p;
            tree.Location = new Point(5, 5);
            tree.Size = new Size(680, 380);
            tree.ShowLines = true;
            tree.ShowPlusMinus = true;
            tree.ShowRootLines = true;
            tree.ExpandAll();                 //展示所有结点
            tree.Nodes[0].EnsureVisible();
            tree.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Right | AnchorStyles.Left;
            tree.Scrollable = true;
            InitializeComponent();
            this.Controls.Add(tree);
        }

        private void Tree_FormClosing(object sender, FormClosingEventArgs e)
        {
            p.TopMost = true;
        }
    }
}
