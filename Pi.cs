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
    public partial class Pi : Form
    {
        Interpreter interpreter = new Interpreter();
        TreeView tree = new TreeView();
        bool isShow = false;                                       //语法分析是否发生错误
        public Pi(Interpreter interpreter, TreeView tree, bool show)
        {
            this.isShow = show;
            this.interpreter = interpreter;
            this.tree = tree;
            this.TopMost = true;
            InitializeComponent();
        }

        private void Pi_Load(object sender, EventArgs e)
        {
            this.BackgroundImage = Properties.Resources.Pi;
            this.TransparencyKey = Color.Black;
        }

        //实现窗体移动
        private Point mouse_offset;
        private void Pi_MouseDown(object sender, MouseEventArgs e)
        {
            mouse_offset = new Point(-e.X, -e.Y);
        }

        private void Pi_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                Point mousePos = Control.MousePosition;
                mousePos.Offset(mouse_offset.X, mouse_offset.Y);
                Location = mousePos;
            }
        }

        //显示TreeView
        private void TreeViewToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (isShow)
            {
                this.TopMost = false;
                Tree t = new Tree(tree, this);
                t.ShowDialog();
            }
            else
                MessageBox.Show("语法分析发生错误.");
        }

        //当前窗体退出
        private void QuitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            interpreter.Show();
            this.Close();
        }
    }
}
