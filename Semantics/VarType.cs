using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Interpreter.Semantics
{
    //普通变量类型
    class VarType
    {
        private string name;          //变量名
        private int type;                 //变量类型, int double float char boolean分别用0，1，2，3，4表示
        private int level;                //变量层次，考虑到局部变量
        private bool isValued;      //是否初始化
        private string value;         //变量数值
        private int count;              //用于区分不同的局部变量

        public VarType(string name, int type, int level, bool isValued, int count = 0)
        {
            this.name = name;
            this.type = type;
            this.level = level;
            this.isValued = isValued;
            this.count = count;
        }

        public VarType(string name, int type, int level, int count = 0)
        {
            this.name = name;
            this.type = type;
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

       public bool IsValued
        {
            get { return isValued; }
            set { isValued = value; }
        }
        public int Count
        {
            get { return count; }
            set { count = value; }
        }
        public string Value
        {
            get { return value; }
            set { this.value = value; }
        }
    }
}
