using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

//@Author: 高战立 2017302580230
//@Data: 2019.10.16
//@Function: 此类用于保存Token表

namespace Interpreter
{
    //token表包括五部分，即：保留字、界限符、运算符、常数以及标识符
    class Token
    {
        //保留字数组 标签1
        public static string[] KeyWords = { "break","case","char","continue","default","double","public","private","true","boolean",
                                                               "else","enum","float","for","if", "int","long","new","return","short","printf","scanf","false",
                                                               "sizeof","struct","switch","void","while","protected","class", "new","typeof","real","do"};
        //界限符数组 标签2
        public static string[] BoundSymbols = { "(", ")", "[", "]", "{", "}", "?", ":", ";" };
        //运算符数组 标签3
        public static string[] Operators = { "+","-","*","/","%","++","--",         //算术运算符
	                                                           "&","|","~","^","<<",">>",            //位操作运算符
	                                                           "!","&&","||",                                 //逻辑运算符
	                                                           "<",">",">=","<=","==","!=",       //比较运算符
	                                                           "=","+=","-=","*=","/=","%="      //赋值运算符
        };
        //常数数组  标签4
        public static ArrayList Constants = new ArrayList();
        //标识符  标签5
        public static ArrayList Identifiers = new ArrayList();
    };
}