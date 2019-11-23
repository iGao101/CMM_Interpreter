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
    public partial class Input : Form
    {
        public Input()
        {
            InitializeComponent();
            textBox1.Text = "Input data: ";
            textBox1.ForeColor = Color.LightGray;
            hasText = true;
        }

        bool hasText = false;
        public string text = null;
        //鼠标点击
        private void TextBox1_Enter(object sender, EventArgs e)
        {
            if (hasText)
                textBox1.Text = "";
            textBox1.ForeColor = Color.Black;
        }

        //鼠标离开
        private void TextBox1_Leave(object sender, EventArgs e)
        {
            if (textBox1.Text == "")
            {
                textBox1.Text = "Input data: ";
                textBox1.ForeColor = Color.LightGray;
                hasText = true;
            }
            else
                hasText = false;
        }

        //重置按钮
        private void Button1_Click(object sender, EventArgs e)
        {
            textBox1.Text = "";
            hasText = false;
        }

        //确定按钮
        private void Button2_Click(object sender, EventArgs e)
        {
            text = textBox1.Text;
            Close();
        }
    }
}
