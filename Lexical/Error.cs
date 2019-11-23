using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Interpreter.Lexical
{
    //该类用于存储错误信息
    class Error
    {
        public int line;             //行数
        public string value;     //字符串
        public int type;           //错误码

        public Error(int type, int line, string value)
        {
            this.type = type;
            this.line = line;
            this.value = value;
        }
    }
}
