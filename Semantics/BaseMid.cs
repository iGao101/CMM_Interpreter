using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Interpreter.Semantics
{
    //四元式
    class BaseMid
    {
        public string op;                   //运算符
        public string arg1;                //第一个参数
        public string arg2;                //第二个参数
        public string result;              //结果

        public BaseMid(string op, string arg1, string arg2, string result)
        {
            this.op = op;
            this.arg1 = arg1;
            this.arg2 = arg2;
            this.result = result;
        }
    }
}
