using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Interpreter.Lexical
{
    //该类用于保存token标签、行数和值等
    class Word
    {
        public int tag;                 //标签
        public int line;                //行号
        public string value;        //字符串
        public Word(int tag, int line, string value)
        {
            this.tag = tag;
            this.line = line;
            this.value = value;
        }
    }
}
