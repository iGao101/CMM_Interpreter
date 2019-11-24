using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Interpreter.Semantics
{
    //保存多维数组
    class ArrayType
    {
        private string name;          //变量名
        private int type;                //变量类型int double float char boolean类型数组分别用0，1，2，3，4表示
        private int level;                //变量层次，考虑到局部变量
        private int length = 0;      //数组长度,可由后面元素动态确定
        private int count;              //用于区分不同的局部变量
        private int dimension;      //数组维度
        private Dictionary<int, string> values = new Dictionary<int, string>();  //保存数组元素,元素有可能仍为多维数组
        private ArrayList counts = new ArrayList();  //保存元素的维度
        public ArrayType(string name, int type, int level, int count , int dimension)   //声明时带有长度的情况
        {
            this.name = name;
            this.type = type;
            this.level = level;
            this.count = count;
            this.dimension = dimension;
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
        public int Dimension
        {
            get { return dimension; }
            set { dimension = value; }
        }
        public Dictionary<int, string> GetValue() { return values; }
        public string GetValue(int index)
        {
            return values[index];
        }
        public void SetValue(int index, string value) { values[index] = value; }

        public ArrayList Counts
        {
            get { return counts; }
            set { counts.Add(value); }
        }

        public int GetSum(int i)
        {
            if (i + 1 > counts.Count)
                return 1;
            int sum = int.Parse((string)counts[i]);
            if(i != counts.Count - 1)
            {
                for (int j = i + 1; j < counts.Count; j++)
                    sum = sum * int.Parse((string)counts[j]);
            }
            return sum;
        }
    }
}
