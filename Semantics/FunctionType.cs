using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Interpreter.Semantics
{
    class FunctionType
    {
        private string name;          //函数名
        private int type;                //返回值类型int double float char boolean void类型分别用0，1，2，3，4, 5表示
        private int level;                //函数层次，考虑到嵌套函数
        private int index = 0;        //函数在程序中的位置索引
        private int terminal = 0;   //方便return后的跳转
        private ArrayList parm = new ArrayList();   //函数参数类型集合
        private ArrayList names = new ArrayList(); //函数参数名称集合
        public FunctionType(string name, int type, int level)
        {
            this.name = name;
            this.type = type;
            this.level = level;
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
        public int Index
        {
            get { return index; }
            set { index = value; }
        }

        public int Terminal
        {
            get { return terminal; }
            set { terminal = value; }
        }
        public int Level
        {
            get { return level; }
            set { level = value; }
        }
        public ArrayList GetParm()
        {
            return parm;
        }
        public void AddParm(int type)
        {
            parm.Add(type);
        }
        public ArrayList GetNames()
        {
            return names;
        }
        public void AddName(string name)
        {
            names.Add(name);
        }
        public bool IsSame(FunctionType function)   //判断函数是否重定义
        {
            if (function.Name != name)
                return false;
            ArrayList p = function.GetParm();
            if (p.Count != parm.Count)
                return false;
            for (int i = 0; i < p.Count; i++)
                if ((int)p[i] != (int)parm[i])
                    return false;
            return true;
        }
    }
}
