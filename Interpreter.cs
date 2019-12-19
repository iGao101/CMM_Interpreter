using System;
using System.Collections;
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
    public partial class Interpreter : Form
    {
        public Interpreter()
        {
            InitializeComponent();
            //buttonEx1.DoubleClick += new EventHandler(btn_DoubleClick);    //绑定按钮双击事件
        }

        TreeView tree = new TreeView();
        //按钮双击事件
        private void btn_DoubleClick(object sender, EventArgs e)
        {
            bool show = false;
            if (lexical.Errors.Count == 0)
                show = true;
            if (show)
            {
                Tree t = new Tree(this, tree);
                this.Hide();
                t.ShowDialog();
            }
            else
            {
                MessageBox.Show("Lexical error");
            }  
        }

        //richtextbox1内容改变自动调用该函数
        private void RichTextBox1_TextChanged(object sender, EventArgs e)
        {
            int index = this.richTextBox1.GetFirstCharIndexOfCurrentLine();
            int line = this.richTextBox1.GetLineFromCharIndex(index) + 1;
            SetLine(line);
            updateLabelRowIndex();
        }

        //滚动时调用
        private void RichTextBox1_VScroll(object sender, EventArgs e)
        {
            updateLabelRowIndex();
        }

        private void updateLabelRowIndex()
        {
            //获取第一行第一个数字或字母的索引
            Point pos = new Point(0, 0);
            int firstIndex = this.richTextBox1.GetCharIndexFromPosition(pos);
            int firstLine = this.richTextBox1.GetLineFromCharIndex(firstIndex);

            //获得最后一行最后一个数字或字幕得索引
            pos.X += this.richTextBox1.ClientRectangle.Width;
            pos.Y += this.richTextBox1.ClientRectangle.Height;
            int lastIndex = this.richTextBox1.GetCharIndexFromPosition(pos);
            int lastLine = this.richTextBox1.GetLineFromCharIndex(lastIndex);
            pos = this.richTextBox1.GetPositionFromCharIndex(lastIndex);
        }

        //鼠标点击
        private void RichTextBox1_MouseDown(object sender, MouseEventArgs e)
        {
            int index = this.richTextBox1.GetFirstCharIndexOfCurrentLine();
            int line = this.richTextBox1.GetLineFromCharIndex(index) + 1;
            SetLine(line);
        }

        //设置光标当前行数
        private void SetLine(int line)
        {
            label1.Text = String.Format("第{0}行", line);
        }

        //选择文件
        private void OpenToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Multiselect = false;
            dialog.Title = "请选择文件夹：";
            dialog.Filter = "所有文件( *.txt ) | *.txt";
            if(dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                string file = dialog.FileName;      //文件路径
                string text = System.IO.File.ReadAllText(file);
                richTextBox1.Text = text;
                richTextBox1.Font = new System.Drawing.Font(FontFamily.GenericSansSerif, 11);
            }
        }

        Lexical.Lexical lexical;
        Grammar.Grammar grammar;
        //词法
        private void Button1_Click(object sender, EventArgs e)
        {
            if (richTextBox1.Text == "")
                return;
            lexical = new Lexical.Lexical();
            ArrayList source = lexical.ReadFromScreen(richTextBox1.Text); //源文件字符数组
            lexical.GetToken(source);
            string[] infos = lexical.SaveToken();
            richTextBox3.Text = "";
            foreach (string info in infos)
                richTextBox3.Text = richTextBox3.Text + info + "\r\n";
            ArrayList errorInfos = lexical.ErrorInfo();      //推导过程中的错误
            foreach (string error in errorInfos)
                richTextBox3.Text = richTextBox3.Text + error + "\r\n";
        }

        //语法
        private void Button3_Click(object sender, EventArgs e)
        {
            lexical = new Lexical.Lexical();                      
            ArrayList source = lexical.ReadFromScreen(richTextBox1.Text);
            lexical.GetToken(source);

            if (lexical == null)  //如果为空，退出
                return;
            grammar = new Grammar.Grammar();
            grammar.Init();
            ArrayList infos = new ArrayList();                 //语法推导过程
            if (lexical.Errors.Count == 0)
                infos = grammar.SyntaxAnalysis(lexical.Coding, lexical.Errors);
            richTextBox3.Text = "";
            foreach (string info in infos)
                richTextBox3.Text = richTextBox3.Text + info + "\r\n";
            ArrayList errorInfos = lexical.ErrorInfo();      //推导过程中的错误
            foreach (string error in errorInfos)
                richTextBox3.Text = richTextBox3.Text + error + "\r\n";
            tree = grammar.treeView;
        }

        //语义
        private void Button2_Click(object sender, EventArgs e)
        {
            lexical = new Lexical.Lexical();  //执行语义前，先执行词法和语法部分
            ArrayList source = lexical.ReadFromScreen(richTextBox1.Text);
            lexical.GetToken(source);
            grammar = new Grammar.Grammar();
            grammar.Init();
            if (lexical.Errors.Count == 0)
                grammar.SyntaxAnalysis(lexical.Coding, lexical.Errors);

            Semantics.IdentifierAnalysis identifier = new Semantics.IdentifierAnalysis(lexical.Coding, lexical.Errors);
            Semantics.MidCode midCode = new Semantics.MidCode(lexical.Coding, lexical.Errors);
            identifier.Init();
            ArrayList VarSet = identifier.VarSet;  //初始化变量、数组、函数、多维数组集合
            ArrayList ListSet = identifier.ListSet;
            ArrayList FunctionSet = identifier.FunctionSet;
            ArrayList ArraySet = identifier.ArraySet;
            if (lexical.Errors.Count == 0)
                midCode.Init(VarSet, ListSet, FunctionSet, ArraySet);

            ArrayList Output = midCode.OutputInfo;
            richTextBox3.Text = "";
            foreach (string info in Output)
                richTextBox3.Text = richTextBox3.Text + info + "\r\n";
            ArrayList errorInfos = lexical.ErrorInfo();      //推导过程中的错误
            foreach (string error in errorInfos)
                richTextBox3.Text = richTextBox3.Text + error + "\r\n";
        }

        //保存token
        private void SaveTokenToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (lexical == null)
                return;
            string[] infos = lexical.SaveToken();          //token信息
            SaveFileDialog saveFile = new SaveFileDialog();
            saveFile.Title = "请选择保存文件路径：";
            saveFile.Filter = "所有文件( *.txt ) | *.txt";
            saveFile.OverwritePrompt = true;            //允许覆盖
            if(saveFile.ShowDialog() == DialogResult.OK)
            {
                string path = saveFile.FileName;
                System.IO.File.WriteAllLines(path, infos, Encoding.UTF8);
            }
        }
        //鼠标点击事件
        private void ButtonEx1_MouseDown(object sender, MouseEventArgs e)
        {
            if(e.Button == MouseButtons.Left)
            {
                Button3_Click(sender, e);
            }
            if(e.Button == MouseButtons.Right)
            {
                btn_DoubleClick(sender, e);
            }
        }
    }
}
