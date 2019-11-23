using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Interpreter.Semantics
{
    //数组类型
    class ListType
    {
        private string name;          //变量名
        private int type;                //变量类型int double float char boolean类型数组分别用0，1，2，3，4表示
        private int level;                //变量层次，考虑到局部变量
        private int length=0;        //数组长度,可由后面元素动态确定
        private int count;              //用于区分不同的局部变量
        private Dictionary<int, string> values = new Dictionary<int, string>();  //数组数值,元素以string形式存放，因此类型判断需借助type

        public ListType(string name, int type, int level, int count =0)
        {
            this.name = name;
            this.type = type;
            this.level = level;
            this.count = count;
        }

        public ListType(string name, int type, int length, int level, int count = 0)   //声明时带有长度的情况
        {
            this.name = name;
            this.type = type;
            this.length = length;
            this.level = level;
            this.count = count;
        }

        //get set 方法
        public string Name
        {
            get { return name; }
            set { name = value; }
        }

        public int Type
        {
            get { return type; }
            set { type = value; }
        }

        public int Level
        {
            get { return level; }
            set { level = value; }
        }

        public int Length
        {
            get { return length; }
            set { length = value; }
        }
        public int Count
        {
            get { return count; }
            set { count = value; }
        }

        public Dictionary<int, string> GetValue() { return values; }
        public string GetValue(int index)
        {
            return values[index];
        }
        public void SetValue(int index, string value) { values[index] = value; }   //程序判断，保证不越界
    }
}
