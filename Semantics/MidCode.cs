﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Interpreter.Semantics
{
    //产生中间代码
    class MidCode
    {
        public ArrayList Coding = new ArrayList();                   //程序token
        public ArrayList Errors = new ArrayList();                     //报错信息
        public int n = 0;                                                            //Coding数组下标
        public int level = 0;                                                       //表示层数，当进入if while等代码块时自增
        public int index = 0;                                                      //当if while分支语句出现时，使用此变量便于语义的分析
        public int isJudge = 0;                                                  //用于错误处理
        public int isLoop = 0;
        public bool isArray = false;                                          //用于动态确定数组长度
        public bool isBreak = false;                                         //判断是否发生中断
        public bool isContinue = false;
        public int isC = 0;
        public int isCAdd = 0;
        public bool isStatement = false;                                 //数组是否处于声明阶段，声明索引不能出现标识符
        public int JudgeIndex = 0;                                          //存储循环条件语句位置，便于continue分析
        public int LoopLevel;
        public bool isFunction = false;
        public int isAdd = 0;                                                    //用于纠正LL(1)文法带来的结合性问题
        public int isMul = 0;
        public int isCompare = 0;
        public int isAnd = 0;
        public string next = null;                                              //存储下一个token
        public string result = null;                                            //上一递归运算结果
        public string name = null;                                            //存储函数名
        public string p1 = null;
        public string op = null;
        public ArrayList VarSet = new ArrayList();                    //保存变量
        public ArrayList ListSet = new ArrayList();                    //保存数组
        public ArrayList ArraySet = new ArrayList();                 //多维数组
        public ArrayList FunctionSet = new ArrayList();           //函数集合
        public ArrayList OutputInfo = new ArrayList();            //保存输出信息

        public MidCode(ArrayList array, ArrayList errors)       //获取词法分析的token数组
        {
            Coding = array;
            Errors = errors;
        }
        public void Init(ArrayList varSet, ArrayList listSet, ArrayList functionSet, ArrayList arraySet)  //初始化VarSet\ListSet\FunctionSet
        {
            VarSet = varSet;
            ListSet = listSet;
            FunctionSet = functionSet;
            ArraySet = arraySet;
            n = 0;
            while (n < Coding.Count - 1)
            {
                Analysis();
            }
        }
        public string GetNext(ref int n)
        {
            if (n < Coding.Count - 1)
            {
                n++;
                string s = ((Lexical.Word)Coding[n]).value;
                return s;
            }
            return null;
        }  //取Coding数组下一个token
        private void Analysis()                   //对token进行语义分析
        {
            string value = ((Lexical.Word)Coding[n]).value;
            int tag = ((Lexical.Word)Coding[n]).tag;
            if (tag == 5)
                value = "identifier";
            switch (value)
            {
                case "int":
                case "double":
                case "float":
                case "char":
                case "boolean":
                    Statement();
                    break;
                case "if":
                    Judge();
                    break;
                case "while":
                    While();
                    break;
                case "for":
                    For();
                    break;
                case "do":
                    Do();
                    break;
                case "{":
                    level++;
                    Block();
                    level--;
                    break;
                case "printf":
                    Printf();
                    break;
                case "scanf":
                    Scanf();
                    break;
                case "void":
                    next = GetNext(ref n);
                    Function();
                    break;
                case "return":
                    Return();
                    Jump();
                    break;
                case "break":
                    isBreak = true;
                    break;
                case "continue":
                    isContinue = true;
                    break;
                case "identifier":
                    Assignment();
                    break;
                default:
                    return;
            }
        }
        private void Statement()         //声明语句
        {
            next = GetNext(ref n);   //指向标识符
            string name = next;
            next = GetNext(ref n);   //标识符下一个元素
            string value = next;
            switch (value)
            {
                case ";":    //只声明，未初始化
                    if(level == 0)
                        DefaultVar(name);  //最外层标识符赋默认值
                    next = GetNext(ref n);
                    break; 
                case "=":  //赋值
                    n--;
                    Assignment();
                    break;
                case "[":   //数组
                    n--;
                    Array();
                    if(next == ";")
                        next = GetNext(ref n);  //跳过;或，
                    else if (next == ",")           //多声明的情况
                        Statement();
                    break;
                case "(":  //函数
                    n--;
                    Function();
                    break;
                case ",":
                    if (level == 0)
                        DefaultVar(name);  //最外层标识符赋默认值
                    Statement();
                    break;
            }
        }
        private void Assignment()      //赋值语句
        {
            string name = ((Lexical.Word)Coding[n]).value;   //存储赋值语句左侧的标识符
            next = GetNext(ref n);
            if(next == "[")   //如：a[3] = 1;
            {
                SetListValue(name);  //给数组赋值
            }
            else
            {
                next = GetNext(ref n);  //跳过等号
                Expression();                 //表达式
                VarType v = GetVar(name);
                if(v != null)
                {
                    if (result != null && TypeCheck(v.Type))//类型检查
                    {
                        v.Value = result;
                        v.IsValued = true;
                    }
                    else
                    {
                        Errors.Add(new Lexical.Error(18, ((Lexical.Word)Coding[n - 1]).line, ""));
                        DealError();
                        return;
                    }
                }
                else
                {
                    ListType list = GetList(name);
                   if(list != null)
                    {
                        if (!ValuedList(list))        //数组赋值,若发生错误return
                            return;
                    }
                    else  //多维数组
                    {
                        ArrayType array = GetArray(name);
                        if(array != null)
                        {
                            if (!ValuedArray(array))
                                return;
                        }
                    }
                }
            }
            if (next == ";")
                next = GetNext(ref n);  //跳过;或，
            else if (next == ",")           //多声明的情况
                Statement();
        }
        private void Array()                 //数组
        {
            isStatement = false;
            string name = ((Lexical.Word)Coding[n]).value;   //存储赋值语句左侧的标识符
            next = GetNext(ref n);  //指向数组左侧[
            if (!ListCheck())
                return;

            if (next == "[")  //多维数组
            {
                ArrayType array = GetArray(name);
                if(!isArray)
                    array.Length = int.Parse(result);  //设置多维数组长度
                while (n < Coding.Count && next == "[")
                {
                    next = GetNext(ref n);
                    Expression();
                    if (result == null)  //发生错误
                    {
                        level--;
                        isJudge--;
                        return;
                    }
                    next = GetNext(ref n);  //跳过数组右侧 ]
                    if (!IsInt(result) || (IsInt(result) && int.Parse(result) <= 0))
                    {
                        Errors.Add(new Lexical.Error(11, ((Lexical.Word)Coding[n - 1]).line, ""));
                        DealError();
                        return;
                    }
                    array.Counts.Add(result);
                }
                if (next == "=")  //声明的时候即初始化
                {
                    if (!ValuedArray(array))
                        return;
                }
                isStatement = false;
                return;
            }

            ListType list = GetList(name);
            if(!isArray)
                list.Length = int.Parse(result);  //设置数组长度
            if (next == ";" || next == ",")        //给数组赋初值
                DefaultList(list);
            else //等号的情况,如 int a[3] = {1,2,3}
            {
                next = GetNext(ref n);  //指向左侧大括号
                if (!ValuedList(list))        //数组赋值,若发生错误return
                    return;
            }
            if (isArray)
                isArray = false;
            isStatement = false;
        }
        private void Function()           //函数
        {
            level++;
            string name = ((Lexical.Word)Coding[n]).value;   //存储函数名
            while (n < Coding.Count && next != ")")
                next = GetNext(ref n);
            next = GetNext(ref n);  //指向左侧大括号
            next = GetNext(ref n);
            FunctionType function = GetFunction(name);
            function.Index = n;       //存储函数索引
            int temp = level;           //存储分析前的level值
            while (n < Coding.Count - 1 && (next != "}" || temp != level))
                next = GetNext(ref n);
            function.Terminal = n;
            next = GetNext(ref n);
            level--;
        }
        private void Judge()                //判断语句
        {
            isJudge++;
            level++;
            next = GetNext(ref n);  //此时指向左侧小括号
            next = GetNext(ref n);
            Expression();
            if (result == null)  //发生错误
            {
                level--;
                isJudge--;
                return;
            }
               
            next = GetNext(ref n);  //跳过右侧小括号
            if (IsNumberic(result))  //是否可数字化
            {
                if(double.Parse(result) != 0)  //条件语句正确，执行if体
                {
                    if(next == "{")  //代码体
                        Block();
                    else  //紧跟一条语句
                        Analysis();
                    if (next == "else")            //调用DealError函数，跳过后序判断
                    {
                        DealError();
                    }   
                }
                else  //条件错误，不执行if代码块，向后看
                {
                    if(next == "{")  //跳过if代码块
                    {
                        int count = 1;
                        while (n < Coding.Count - 1 && count != 0)
                        {
                            next = GetNext(ref n);
                            if (next == "{")
                                count++;
                            if (next == "}")
                                count--;
                        }
                        next = GetNext(ref n);
                    }
                    else                  //跳过后面紧跟的语句
                    {
                        while (n < Coding.Count - 1 && next != ";")
                            next = GetNext(ref n);
                        next = GetNext(ref n);  //跳过;
                    }
                    if (next == "else" && !isBreak && !isContinue)  //向后执行else if或else
                    {
                        next = GetNext(ref n);
                        if (next == "if")             //else if语句
                            Judge();
                        else if (next == "{")       //else代码块
                            Block();
                        else                              //else语句
                            Analysis();
                    }
                }
            }
            else
            {
                Errors.Add(new Lexical.Error(21, ((Lexical.Word)Coding[n - 1]).line, ""));
                DealError();
                return;
            }
            level--;
            isJudge--;
        }
        private void Do()                    //循环语句 do{ } while();
        {
            isLoop++;
            level++;
            LoopLevel = level;
            int temp = n;
            int terminal = GetTerminal("do");  //循环结束的位置
            n = temp;
            next = GetNext(ref n);  //跳过do关键字
            int index = n;  //保存条件索引值
            if (next == "{")
                Block();
            else
                Analysis();

            if (isBreak)       //发生中断
            {
                while (n < Coding.Count - 1 && n != terminal)
                    next = GetNext(ref n);
                isBreak = false;  //归位
                level--;
                isLoop--;
                return;
            }

            if (isContinue)  //continue
            {
                GetTerminal("do");
                if (JudgeIndex != 0)
                {
                    n = JudgeIndex;
                }
                isContinue = false;  //归位
                JudgeIndex = 0;
            }

            next = GetNext(ref n);  //跳过while关键字
            next = GetNext(ref n);  //跳过左侧括号
            Expression();
            if (result == null)  //发生错误
            {
                level--;
                isLoop--;
                return;
            }
                
            next = GetNext(ref n);  //跳过右侧括号
           
            if (IsNumberic(result))
            {
                if (double.Parse(result) == 0)
                {
                    next = GetNext(ref n);  //跳过；
                    level--;
                    isLoop--;
                    return;
                }
                while(double.Parse(result) != 0)
                {
                    n = index - 1;
                    next = GetNext(ref n);
                    if (next == "{")
                        Block();
                    else
                        Analysis();

                    if (isBreak)   //发生中断
                    {
                        while (n < Coding.Count - 1 && n != terminal)
                            next = GetNext(ref n);
                        isBreak = false;  //归位
                        return;
                    }
                    if (isContinue)  //continue
                    {
                        GetTerminal("do");
                        if (JudgeIndex != 0)
                        {
                            n = JudgeIndex;
                        }
                        isContinue = false;  //归位
                        JudgeIndex = 0;
                    }

                    next = GetNext(ref n);  //跳过while关键字
                    next = GetNext(ref n);  //跳过左侧括号
                    Expression();
                    if (result == null)  //发生错误
                    {
                        level--;
                        isJudge--;
                        return;
                    }
                    next = GetNext(ref n);  //跳过右侧括号
                }
                next = GetNext(ref n);  //跳过；
            }
            else
            {
                Errors.Add(new Lexical.Error(21, ((Lexical.Word)Coding[n - 1]).line, ""));
                DealError();
                return;
            }
            level--;
            isLoop --;
        }
        private void While()                //循环语句 while
        {
            isLoop++;
            level++;
            LoopLevel = level;
            next = GetNext(ref n);  //指向左侧小括号
            next = GetNext(ref n);
            int t = n;
            int terminal = GetTerminal("while");
            n = t - 1;
            next = GetNext(ref n);
            int index = n;  //保存条件索引值
            Expression();   //执行while条件语句
            if (result == null)
            {
                level--;
                isLoop--;
                return;
            }
            next = GetNext(ref n);  //跳过右侧小括号
            if (IsNumberic(result))
            {
                while(double.Parse(result) != 0)  //满足添加执行代码块
                {
                   if(next == "{")
                    {
                        Block();
                        if(result == null)
                        {
                            level--;
                            isLoop--;
                            return;
                        }
                    }
                   else if(next == ";")  //死循环
                    {
                        while (true) ;
                    }
                    else
                    {
                        Analysis();
                    }

                    if (isBreak)  //循环中止
                    {
                        while (n < Coding.Count - 1 && n != terminal)
                            next = GetNext(ref n);
                        isBreak = false;  //归位
                        level--;
                        isLoop--;
                        return;
                    }
                    if (isContinue)
                        isContinue = false;

                    n = index-1;
                    next = GetNext(ref n);
                    Expression();  //执行while条件语句
                    if (result == null)
                    {
                        level--;
                        isLoop--;
                        return;
                    }
                    next = GetNext(ref n);  //跳过右侧小括号
                }

                if (double.Parse(result) == 0)       //条件不满足，跳过代码块
                {
                    DealError();
                    level--;
                    return;
                }
            }
            else
            {
                Errors.Add(new Lexical.Error(21, ((Lexical.Word)Coding[n - 1]).line, ""));
                DealError();
                return;
            }
            level--;
            isLoop--;
        }
        private void For()                    //循环语句 for
        {
            isLoop++;
            level++;
            LoopLevel = level;
            next = GetNext(ref n);  //指向左侧小括号
            next = GetNext(ref n);
            int t = n;
            int terminal = GetTerminal("for");
            n = t - 1;
            next = GetNext(ref n);
            if (next != ";")
                Analysis();
            if (next == ";")
                next = GetNext(ref n);
            if (next == ";")  //死循环
                while (true) ;
            int index1 = n;  //保存条件索引值
            Expression();
            if (result == null)
            {
                level--;
                isLoop--;
                return;
            }
            next = GetNext(ref n);  //跳过;
            int index2 = n;  //存储赋值索引值
            if (IsNumberic(result))
            {
                while(double.Parse(result) != 0)
                {
                    while (n < Coding.Count && next != ")")
                        next = GetNext(ref n);
                    next = GetNext(ref n);
                    if (next == "{")
                        Block();
                    else
                        Analysis();

                    if (isBreak)  //循环中止
                    {
                        while (n < Coding.Count - 1 && n != terminal)
                            next = GetNext(ref n);
                        isBreak = false;  //归位
                        level--;
                        isLoop--;
                        return;
                    }
                    if (isContinue)
                        isContinue = false;

                    n = index2 - 1;  //操作
                    next = GetNext(ref n);
                    if(next != ")")
                        Assignment();

                    n = index1 - 1;  //判断
                    next = GetNext(ref n);
                    Expression();
                    if (result == null)
                    {
                        level--;
                        isLoop--;
                        return;
                    }
                }
                if (double.Parse(result) == 0)
                {
                    while (n < Coding.Count - 1 && n != terminal)
                        next = GetNext(ref n);
                    isBreak = false;  //归位
                    level--;
                    isLoop--;
                    return;
                }
                if (next == "}")
                    next = GetNext(ref n);
            }
            else
            {
                Errors.Add(new Lexical.Error(21, ((Lexical.Word)Coding[n - 1]).line, ""));
                DealError();
                return;
            }
            level--;
            isLoop--;
        }
        private void Printf()                //输出语句
        {
            next = GetNext(ref n);  //指向printf最左侧小括号
            next = GetNext(ref n);
            if (next.Contains("\"") || next.Contains("'"))
            {
                result = next;
                next = GetNext(ref n);  //跳过该字符常数
            }
            else if(next == ")")
            {
                OutputInfo.Add("");
                next = GetNext(ref n);
                next = GetNext(ref n);   //跳过；
                return;
            }
            else
                Expression();
            if (result == null)  //发生错误
            {
                level--;
                isJudge--;
                return;
            }
            result = result.Replace("'", "");
            result = result.Replace("\"", "");
            next = GetNext(ref n);  //跳过printf最右侧小括号
            OutputInfo.Add(result);
            next = GetNext(ref n);   //跳过；
        }
        private void Scanf()                //输入语句
        {
            next = GetNext(ref n);  //指向左侧小括号
            next = GetNext(ref n);
            string name = next;  //标识符名称
            next = GetNext(ref n);
            if (IsNumberic(name) || name.Contains("\"") || name.Contains("'") || name == ")")
            {
                Errors.Add(new Lexical.Error(22, ((Lexical.Word)Coding[n]).line, ""));
                DealError();
                return;
            }
            if(next != "[")  //标识符赋值
            {
                VarType v = GetVar(name);
                if(v != null)
                {
                    Input input = new Input();
                    input.ShowDialog();
                    result = input.text;
                    if (!TypeCheck(v.Type))
                    {
                        Errors.Add(new Lexical.Error(18, ((Lexical.Word)Coding[n - 1]).line, ""));
                        DealError();
                        return;
                    }
                    v.Value = result;
                    v.IsValued = true;
                }
                next = GetNext(ref n);
                next = GetNext(ref n);  //跳过；
                return;
            }
            if (!ListCheck())  //检查访问是否出错
                return;
            ListType list = GetList(name);
            int index = int.Parse(result);
            if (list != null)
            {
                if (index >= list.Length)       //索引大于数组长度
                {
                    Errors.Add(new Lexical.Error(13, ((Lexical.Word)Coding[n]).line, ""));
                    DealError();
                    return;
                }
                Input input = new Input();
                input.ShowDialog();
                result = input.text;
                if (!TypeCheck(list.Type))
                {
                    Errors.Add(new Lexical.Error(18, ((Lexical.Word)Coding[n - 1]).line, ""));
                    DealError();
                    return;
                }
                list.SetValue(index, result);  //给数组赋值
            }
            else  //多维数组赋值
            {
                ArrayType array = GetArray(name);
                if (array != null)
                {
                    int sum = GetArrayIndex(array);
                    Input input = new Input();
                    input.ShowDialog();
                    result = input.text;
                    if (!TypeCheck(array.Type))
                    {
                        Errors.Add(new Lexical.Error(18, ((Lexical.Word)Coding[n - 1]).line, ""));
                        DealError();
                        return;
                    }
                    array.SetValue(sum, result);
                }
            }
            next = GetNext(ref n);
            next = GetNext(ref n);  //跳过；
        }
        private void Block()                 //程序块
        {
            next = GetNext(ref n);   //跳过左侧大括号
            int temp = level;            //存储分析前的level值
            while (n < Coding.Count - 1 && (next != "}" || temp != level))
            {
                Analysis();
                if (isBreak)
                    return;
                if (isContinue)
                    return;
            }
            next = GetNext(ref n);   //跳过右侧大括号
        }
        private void Return()               //返回值函数
        {
            next = GetNext(ref n);
            FunctionType function = GetFunction(name);
            if(function.Type == 5 && next != ";")
            {
                Errors.Add(new Lexical.Error(18, ((Lexical.Word)Coding[n - 1]).line, ""));
                DealError();
                return;
            }
            Expression();
            next = GetNext(ref n);  //跳过；
            if (!TypeCheck(function.Type))  //类型检查
            { 
                Errors.Add(new Lexical.Error(18, ((Lexical.Word)Coding[n - 1]).line, ""));
                DealError();
                return;
            }
        }
        private void Jump()                 //return,跳过后序语句
        {
            FunctionType function = GetFunction(name);
            while (n < Coding.Count - 1 && n < function.Terminal - 1)
                n++;
            if(n == function.Terminal - 1)
                next = GetNext(ref n);
        }
        private void Expression()        //表达式语句，需要考虑运算的优先级等
        {
            /*
             文法原理
             "Expression->E1 E2",
             "E1->E3 E4",
             "E2->&& E1 E2|  || E1 E2|  ~",
             "E3->E5 E6",
             "E4->< E3 E4|  <= E3 E4|  > E3 E4|  >= E3 E4|  == E3 E4|  <> E3 E4|  ~",
             "E5->E7 E8",
             "E6->+ E5 E6|  - E5 E6|  ~",
             "E7->! E9|  E9",
             "E8->* E7 E8|  / E7 E8|  % E7 E8|  ~",
             "E9->( Expression )|  constant|  identifier E10|  Input",
             "E10->[ Expression ]|  ( Value Value1 )|  ~",
             */
            E1();
            E2();
            //考虑多赋值
        }
        private void E1()                      //使用文法与语法分析时判断表达式的文法相同
        {
            E3();
            E4();
        }
        private void E2()                      //处理&&   ||
        {
            if (next != "&&" && next != "||")
                return;
            if (result == null)
            {
                level--;
                isLoop--;
                return;
            }
            if (isAnd != 0)
            {
                string temp = next;  //存储&&或||
                next = GetNext(ref n);
                if (IsNumberic(result) && int.Parse(result) != 0)
                    result = "1";
                else if (!IsNumberic(result))
                {
                    Errors.Add(new Lexical.Error(15, ((Lexical.Word)Coding[n - 1]).line, ""));
                    DealError();  //错误处理
                    return;
                }
                if(op == "&&")
                {
                    if (p1 == "1" && result == "1")
                        result = "1";
                    else
                        result = "0";
                }
                if(op == "||")
                {
                    if (p1 == "1" || result == "1")
                        result = "1";
                    else
                        result = "0";
                }
                string temp2 = result;
                E1();
                p1 = temp2;                //更新p1
                op = temp;                 //更新运算符
                isCompare++;
                E2();
                isCompare--;
                return;
            }

            if (next == "&&" && result != null)
            {
                bool isRight = false;
                if (IsNumberic(result) && int.Parse(result) != 0)
                    isRight = true;
                else if (!IsNumberic(result))
                {
                    Errors.Add(new Lexical.Error(15, ((Lexical.Word)Coding[n - 1]).line, ""));
                    DealError();  //错误处理
                    return;
                }
                op = next;                     //保存操作符
                next = GetNext(ref n);  //跳过&&
                if (isRight)
                {
                    isRight = false;
                    p1 = "1";                       //前一步运算结果
                    string num1 = p1;
                    string op1 = op;
                    E1();
                    p1 = num1;
                    op = op1;

                    isAnd++;
                    E2();
                    isAnd--;
                    if (IsNumberic(result) && int.Parse(result) != 0)
                        isRight = true;
                    else if (!IsNumberic(result))
                    {
                        Errors.Add(new Lexical.Error(13, ((Lexical.Word)Coding[n - 1]).line, ""));
                        DealError();  //错误处理
                        return;
                    }
                    if (op == "&&")
                    {
                        if (p1 == "1" && result == "1")
                            result = "1";
                        else
                            result = "0";
                    }
                    if (op == "||")
                    {
                        if (p1 == "1" || result == "1")
                            result = "1";
                        else
                            result = "0";
                    }
                }
                else  //对于&&如果左侧式子为假，直接退出
                {
                    result = "0";
                    while (n < Coding.Count - 1 && (next != ")" && next != ";" && next != "," && next != "&&" && next != "||"))
                        next = GetNext(ref n);
                    if (next == "&&" || next == "||")
                        E2();
                }
            }
            else if(next == "||" && result != null)
            {
                bool isRight = false;
                if (IsNumberic(result) && int.Parse(result) != 0)
                    isRight = true;
                else if (!IsNumberic(result))
                {
                    Errors.Add(new Lexical.Error(13, ((Lexical.Word)Coding[n - 1]).line, ""));
                    DealError();  //错误处理
                    return;
                }
                op = next;                     //保存操作符
                next = GetNext(ref n);  //跳过||
                if (isRight)  //对于||如果左侧式子为真，直接退出
                {
                    result = "1";
                    while (n < Coding.Count - 1 && (next != ")" && next != ";" && next != "," && next != "&&" && next != "||"))
                        next = GetNext(ref n);
                    if (next == "&&" || next == "||")
                        E2();
                }
                else
                {
                    p1 = "0";                       //前一步运算结果
                    string num1 = p1;
                    string op1 = op;
                    E1();
                    isAnd++;
                    E2();
                    isAnd--;
                    if (IsNumberic(result) && int.Parse(result) != 0)
                        isRight = true;
                    else if (!IsNumberic(result))
                    {
                        Errors.Add(new Lexical.Error(13, ((Lexical.Word)Coding[n - 1]).line, ""));
                        DealError();  //错误处理
                        return;
                    }
                    if (op == "&&")
                    {
                        if (p1 == "1" && result == "1")
                            result = "1";
                        else
                            result = "0";
                    }
                    if (op == "||")
                    {
                        if (p1 == "1" || result == "1")
                            result = "1";
                        else
                            result = "0";
                    }
                }
            }
        }
        private void E3()
        {
            E5();
            E6();
        }
        private void E4()                      //处理比较符如>
        {
            if ((next == "<" || next == "<=" || next == ">" || next == ">=" || next == "==" || next == "<>") && result != null)
            {
                if (isCompare != 0)          //解决LL1结合性问题
                {
                    string temp = next;
                    next = GetNext(ref n);
                    if (!Compare(p1, result, op))
                        DealError();
                    string temp2 = result;
                    E3();
                    p1 = temp2;                //更新p1
                    op = temp;                 //更新运算符
                    isCompare++;
                    E4();
                    isCompare--;
                    return;
                }
                p1 = result;                   //前一步运算结果
                string num1 = p1;
                op = next;                     //保存操作符
                string op1 = op;
                next = GetNext(ref n);  //跳过比较符
                E3();
                p1 = num1;
                op = op1;
                isCompare++;
                E4();
                isCompare--;
                string num2 = result;
                if (!Compare(p1, num2, op))  //调用比较函数
                {
                    DealError();
                    return;
                }
            }
            else
                return;
        }
        private void E5()
        {
            E7();
            E8();
        }
        private void E6()                      //处理加减
        {
            if ((next == "+" || next == "-") && result != null)
            {
                if(isC != 0)
                {
                    if (isCAdd != 0)          //解决LL1结合性问题
                    {
                        string temp = next;
                        next = GetNext(ref n);
                        if (!Operation(p1, result, op))
                        {
                            DealError();
                            return;
                        }
                        string temp2 = result;
                        E5();
                        p1 = temp2;                //更新p1
                        op = temp;                 //更新运算符
                        isCAdd++;
                        E6();
                        isCAdd--;
                        return;
                    }
                    p1 = result;                   //前一步运算结果
                    string num3 = p1;
                    op = next;                     //保存操作符
                    string op2 = op;
                    next = GetNext(ref n);  //跳过+或-
                    E5();
                    p1 = num3;
                    op = op2;
                    isCAdd++;
                    E6();
                    isCAdd--;
                    string num4 = result;
                    if (!Operation(p1, num4, op))
                        DealError();
                    return;
                }
                if (isAdd != 0)          //解决LL1结合性问题
                {
                    string temp = next;
                    next = GetNext(ref n);
                    if (!Operation(p1, result, op))
                    {
                        DealError();
                        return;
                    }
                    string temp2 = result;
                    E5();
                    p1 = temp2;                //更新p1
                    op = temp;                 //更新运算符
                    isAdd++;
                    E6();
                    isAdd--;
                    return;
                }
                p1 = result;                   //前一步运算结果
                string num1 = p1;
                op = next;                     //保存操作符
                string op1 = op;
                next = GetNext(ref n);  //跳过+或-
                E5();
                p1 = num1;
                op = op1;
                isAdd++;
                E6();
                isAdd--;
                string num2 = result;
                if (!Operation(p1, num2, op))
                    DealError();
            }
            else
                return;
        }
        private void E7()                      //处理取反
        {
            if (next == "!")
            {
                int num = 0;
                while(n < Coding.Count && next == "!")
                {
                    num++;
                    next = GetNext(ref n);
                }
                E9();
                if (!IsInt(result))  //取反运算针对整数(bool类型赋值置为0，1)
                {
                    Errors.Add(new Lexical.Error(16, ((Lexical.Word)Coding[n]).line, ""));
                    DealError();
                    return;
                }
                if(num %2 != 0)
                    result = int.Parse(result) == 0 ? "1" : "0";
            }
            else
                E9();
        }
        private void E8()                      //处理乘除
        {
            if ((next == "*" || next == "/" || next == "%") && result != null)
            {
                if (isMul !=0)          //解决LL1结合性问题
                {
                    string temp = next;
                    next = GetNext(ref n);
                    if (!Operation(p1, result, op))
                    {
                        DealError();
                        return;
                    }
                        
                    string temp2 = result;
                    E7();
                    p1 = temp2;                //更新p1
                    op = temp;                 //更新运算符
                    isMul++;
                    E8();
                    isMul--;
                    return;
                }
                p1 = result;         //前一步运算结果
                op = next;           //保存操作符
                next = GetNext(ref n);  //跳过* /或%
                string num1 = p1;
                string op1 = op;
                E7();
                p1 = num1;
                op = op1;
                isMul++;
                E8();
                isMul--;
                string num2 = result;
                if (!Operation(p1, num2, op))
                    DealError();
            }
            else
                return;
        }
        private void E9()                      //赋值
        {
            if (next == "(")
            {
                isC++;
                next = GetNext(ref n);  //跳过左括号
                Expression();
                next = GetNext(ref n);  //跳过右括号
                isC--;
            }
            else if (IsNumber(next[0]) || next[0] == '.' || next[0] == '-' || next[0] == '+' || next == "true" || next == "false")  //常数，需要考虑不同进制
            {
                if (next == "true")
                    result = "1";
                else if (next == "false")
                    result = "0";
                else if (next.Length > 2 && (next[1] == 'x' || next[1] == 'X'))  //十六进制
                    result = Convert.ToInt32(next, 16).ToString();
                else if (next.Length > 2 && (next[1] == 'b' || next[1] == 'B'))  //二进制
                    result = Convert.ToInt32(next, 2).ToString();
                else  //十进制
                    result = next;
                next = GetNext(ref n);
            }
            else if (next[0] == '\'' || next[0] == '"')  //字符类型
            {
                result = next;
                next = GetNext(ref n);
            }
            
            else  //数组、标识符、函数
            {
                string name = next;  //标识符名称
                next = GetNext(ref n);
                if(next == "(")           //函数
                {
                    ValuedFunction(name);
                }
                else if(next == "[")    //数组
                {
                    GetListValue(name);
                 }
                else                            //标识符    注意不同类型标识符，如数组
                {
                    n--;
                    VarType v = GetVar(name);
                    if (v != null)          //变量
                    {
                        if (v.IsValued)  //取标识符数值
                        {
                            result = v.Value;
                            next = GetNext(ref n);
                        }
                        else                 //标识符未初始化
                        {
                            Errors.Add(new Lexical.Error(17, ((Lexical.Word)Coding[n]).line, name));
                            DealError();
                            return;
                        }
                    }
                    else
                    {
                        ListType list = GetList(name);
                        if(list != null)
                        {
                            if(list.Type == 3)  //输出char数组全部内容
                            {
                                result = "\"";
                                foreach (string i in list.GetValue().Values)
                                    result += i.ToString();
                                result += "\"";
                                next = GetNext(ref n);
                            }
                            else  //其他类型数组即输出第一个元素
                            {
                                if (list.Length != list.GetValue().Count)  //未初始化
                                {
                                    Errors.Add(new Lexical.Error(17, ((Lexical.Word)Coding[n]).line, name));
                                    DealError();
                                    return;
                                }
                                else  //给结果赋值
                                {
                                    result = list.GetValue(0);
                                    next = GetNext(ref n);
                                }   
                            }
                        }
                    }
                }
            }
        }
        private VarType GetVar(string name)                      //根据名字获取标识符
        {
            VarType list = null;
            int line = ((Lexical.Word)Coding[n]).line;
            foreach (VarType va in VarSet)  //优先同层标识符
                if (va.Name == name && va.Level == level && (va.Count >= line || va.Level == 0 || isFunction))
                {
                    list = va;
                    return list;
                }
            foreach (VarType va in VarSet)
                if (va.Name == name && va.Level < level && (va.Count >= line || va.Level == 0 || isFunction))
                {
                    list = va;
                    return list;
                }
            return null;
        }
        private ListType GetList(string name)                      //根据名字获取数组
        {
            ListType list = null;
            int line = ((Lexical.Word)Coding[n]).line;
            foreach (ListType va in ListSet)
                if (va.Name == name && va.Level == level && (va.Count >= line || va.Level == 0))
                {
                    list = va;
                    return list;
                }
            foreach (ListType va in ListSet)
                if (va.Name == name && va.Level < level && (va.Count >= line || va.Level == 0))
                {
                    list = va;
                    return list;
                }
            return list;
        }
        private ArrayType GetArray(string name)                //根据名字获取多维数组
        {
            ArrayType list = null;
            int line = ((Lexical.Word)Coding[n]).line;
            foreach (ArrayType va in ArraySet)
                if (va.Name == name && va.Level == level && (va.Count >= line || va.Level == 0))
                {
                    list = va;
                    return list;
                }
            foreach (ArrayType va in ArraySet)
                if (va.Name == name && va.Level < level && (va.Count >= line || va.Level == 0))
                {
                    list = va;
                    return list;
                }
            return list;
        } 
        private FunctionType GetFunction(string name)     //根据名称获取函数
        {
            FunctionType list = null;
            foreach (FunctionType va in FunctionSet)
                if (va.Name == name && va.Level == level)
                {
                    list = va;
                    return list;
                }
            foreach (FunctionType va in FunctionSet)
                if (va.Name == name && va.Level < level)
                {
                    list = va;
                    return list;
                }
            return list;
        }
        private bool IsNumberic(string value)                     //判断字符串是否可数字化
        {
            return Regex.IsMatch(value, @"^[+|-]?\d*[.]?\d*$");
        }
        private bool IsDecimal(string value)                        //判断字符串是否为小数
        {
            return Regex.IsMatch(value, @"^[+|-]?\d*[.]\d*$");
        }
        private bool IsInt(string value)                                 //判断字符串是否为整数
        {
            return Regex.IsMatch(value, @"^[+|-]?\d*$");
        }
        private bool Compare(string n1, string n2, string op)     //处理比较符语句
        {
            if(!IsNumberic(n1) || !IsNumberic(n1))//比较符两侧不可比较
            {
                Errors.Add(new Lexical.Error(15, ((Lexical.Word)Coding[n]).line, ""));
                result = null;
                return false;
            }
            double num1;  //存储转换后的p1 p2
            double num2;
            if (IsDecimal(n1))
                num1 = double.Parse(n1);
            else
                num1 = int.Parse(n1);

            if (IsDecimal(n2))
                num2 = double.Parse(n2);
            else
                num2 = int.Parse(n2);
            switch (op)    //根据运算符确定比较类型
            {
                case ">":
                    result = num1 > num2 ? 1.ToString() : 0.ToString();
                    break;
                case ">=":
                    result = num1 >= num2 ? 1.ToString() : 0.ToString();
                    break;
                case "<":
                    result = num1 < num2 ? 1.ToString() : 0.ToString();
                    break;
                case "<=":
                    result = num1 <= num2 ? 1.ToString() : 0.ToString();
                    break;
                case "==":
                    result = num1 == num2 ? 1.ToString() : 0.ToString();
                    break;
                case "<>":
                    result = num1 !=  num2 ? 1.ToString() : 0.ToString();
                    break;
            }
            return true;
        }
        private bool Operation(string n1, string n2, string op)   //执行加减乘除等运算
        {
            if(n1.Contains("\"") && n2.Contains("\"") && op == "+")  //字符串相加
            {
                result = n1 + n2;
                return true;
            }
            ListType list1 = GetList(n1);
            ListType list2 = GetList(n2);
            if (list1 != null && list2 != null && op == "+")
                result = n1 + n2;

            if (!IsNumberic(n1) || !IsNumberic(n2))
            {
                Errors.Add(new Lexical.Error(15, ((Lexical.Word)Coding[n]).line, ""));
                result = null;
                return false;
            }
            double num1;  //存储转换后的p1 p2
            double num2;
            bool isInt1 = false;
            bool isInt2 = false;
            if (IsDecimal(n1))
                num1 = double.Parse(n1);
            else
            {
                num1 = int.Parse(n1);
                isInt1 = true;
            }

            if (IsDecimal(n2))
                num2 = double.Parse(n2);
            else
            {
                num2 = int.Parse(n2);
                isInt2 = true;
            }
            double finnal = 0;
            switch (op)
            {
                case "+":
                    finnal = num1 + num2;
                    break;
                case "-":
                    finnal = num1 - num2;
                    break;
                case "*":
                    finnal = (num1 * num2);
                    break;
                case "/":
                    if(num2 == 0)  //除0错误
                    {
                        Errors.Add(new Lexical.Error(19, ((Lexical.Word)Coding[n]).line, ""));
                        return false;
                    }
                    finnal = (num1 / num2);
                    break;
                case "%":
                    finnal = (num1 % num2);
                    break;
            }
            if (isInt1 && isInt2)
                result = ((int)finnal).ToString();
            else
                result = finnal.ToString();
            return true;
        }
        private bool TypeCheck(int type)                             //类型检测，标识符和result
        {
            //int double float char boolean
            bool isRight = false;
            switch (type)
            {
                case 0:  //int
                    if (IsInt(result))
                        isRight = true;
                    if (IsDecimal(result))
                    {
                        string[] str = result.Split('.');
                        result = str[0];
                        isRight = true;
                    }
                    break;
                case 1:  //double
                case 2:  //float
                    if (IsNumberic(result))
                        isRight = true;
                    break;
                case 3:  //char
                    if (result.Contains("'"))
                        isRight = true;
                    break;
                case 4:  //boolean
                    if (IsNumberic(result))  //bool类型赋值时，true为1，false为0
                        return true;
                    break;
            }
            return isRight;
        }
        private bool IsNumber(char a)                                 //判断是否为数字
        {
            if ('0' <= a && a <= '9')
                return true;
            else
                return false;
        }
        private bool IsLetter(char a)                                     //判断是否为字母
        {
            if (('a' <= a && a <= 'z') || ('A' <= a && a <= 'Z'))
                return true;
            else
                return false;
        }
        private void DefaultVar(string name)                       //最外层标识符赋默认值
        {
            VarType v = GetVar(name);
            if (v == null)
                return;
            switch (v.Type)
            {
                case 0:  //int
                case 1:  //double
                case 2:  //float
                case 4:  //boolean
                    v.Value = "0";
                    break;
                case 3:  //char
                    v.Value = "' '";
                    break;
            }
        }
        private void DefaultList(ListType list)                       //最外层数组赋初值
        {
            int length = list.Length;  //数组长度
            if(length == 0)
            {
                Errors.Add(new Lexical.Error(11, ((Lexical.Word)Coding[n - 1]).line, ""));
                DealError();
                return;
            }
            switch (list.Type)
            {
                case 0:  //int
                case 1:  //double
                case 2:  //float
                case 4:  //boolean
                    for (int i = 0; i < length; i++)
                        list.SetValue(i, "0");
                    break;
                case 3:  //char
                    for (int i = 0; i < length; i++)
                        list.SetValue(i, "' '");
                    break;
            }
        }
        private bool ValuedList(ListType list)                       //根据表达式给数组赋值
        {
            result = next;
            if (((Lexical.Word)Coding[n]).tag == 5 && ((Lexical.Word)Coding[n + 1]).value == "+")
                Expression();
            if (list.Type == 3 && result.Contains("\""))            //字符串给char数组赋值的情况
            {
                if (list.Length == 0)
                    list.Length = result.Length - 2;
                else if(list.Length < result.Length - 2)  //错误处理
                {
                    Errors.Add(new Lexical.Error(20, ((Lexical.Word)Coding[n - 1]).line, ""));
                    DealError();
                    return false;
                }
                for (int i = 1; i < result.Length - 1; i++)
                    list.SetValue(i - 1, "'" + result[i].ToString() + "'");
                if (list.Length > result.Length - 2)
                    for (int i = result.Length - 2; i < list.Length; i++)
                        list.SetValue(i, "' '");
                next = GetNext(ref n);
                return true;
            }

            int num = 1;  //记录{}中元素个数
            ArrayList element = new ArrayList();  //保存元素
            while(n < Coding.Count - 1 && next != "}")
            {
                next = GetNext(ref n);  //第一次：跳过左侧大括号
                if (next == ",")
                    num++;
                else if (next != "," && next != "}")
                    element.Add(next);
            }
            if (!isArray && num > list.Length)  //元素数量超过数组长度，报错
            {
                Errors.Add(new Lexical.Error(20, ((Lexical.Word)Coding[n - 1]).line, ""));
                DealError();
                return false;
            }
            if (isArray)  //根据元素个数默认赋值
                list.Length = element.Count;
            for (int i = 0; i < element.Count; i++)  //赋值
            {
                result = (string)element[i];
                if (TypeCheck(list.Type))
                    list.SetValue(i, result);
                else
                {
                    Errors.Add(new Lexical.Error(18, ((Lexical.Word)Coding[n - 1]).line, ""));
                    DealError();
                    return false;
                }
            }
            
            if(element.Count < list.Length)            //后面未赋值的元素进行初始化
            {
                switch (list.Type)
                {
                    case 0:  //int
                    case 1:  //double
                    case 2:  //float
                    case 4:  //boolean
                        for (int i = element.Count; i < list.Length; i++)
                            list.SetValue(i, "0");
                        break;
                    case 3:  //char
                        for (int i = element.Count; i < list.Length; i++)
                            list.SetValue(i, "' '");
                        break;
                }
            }
            next = GetNext(ref n);  //跳过右侧大括号
            return true;
        }
        private bool ValuedArray(ArrayType array)             //根据表达式初始化多维数组
        {
            next = GetNext(ref n);  //跳过等号
            next = GetNext(ref n);
            string t = null;
            while (n < Coding.Count - 1 && next != "}")
            {
                while (n < Coding.Count - 1 && next != "}")
                {
                    if(next == "{")
                    {
                        next = GetNext(ref n);
                        continue;
                    }
                    t += next;
                    next = GetNext(ref n);
                }
                next = GetNext(ref n);
            }
            next = GetNext(ref n);  //跳过最右侧大括号
            t.Replace("{", "");
            t.Replace("}", "");
            string[] parms = t.Split(',');
            if (!isArray && parms.Length > array.Length * array.GetSum(0))
            {
                Errors.Add(new Lexical.Error(20, ((Lexical.Word)Coding[n - 1]).line, ""));
                DealError();
                return false;
            }
            if (isArray)  //根据元素个数默认赋值
                array.Length = parms.Length;
            for (int i = 0; i < parms.Length; i++)  //多维数组添加元素
                array.SetValue(i, parms[i]);
            return true;
        }
        private bool ListCheck()                                           //检查数组访问是否越界等问题
        {
            next = GetNext(ref n);  //指向常数或者表达式
            if (next == "]" && ((Lexical.Word)Coding[n + 1]).value == "=")
            {
                isArray = true;
                next = GetNext(ref n);
                return true;
            }
            if (next == "]" && ((Lexical.Word)Coding[n + 1]).value != "=")
            {
                Errors.Add(new Lexical.Error(11, ((Lexical.Word)Coding[n - 1]).line, ""));
                DealError();
                return false;
            }
            int temp = n;
            if (isStatement && !IsLenghtRight())      //数组长度索引中不能出现标识符
            {
                Errors.Add(new Lexical.Error(11, ((Lexical.Word)Coding[n - 1]).line, ""));
                DealError();
                return false;
            }
            n = temp;
            next = ((Lexical.Word)Coding[n]).value;  //调用IsLengthRight后重新归位next
            Expression();
            next = GetNext(ref n);  //跳过数组右侧 ]
            if (!IsInt(result) || (IsInt(result) && int.Parse(result) < 0))
            {
                Errors.Add(new Lexical.Error(11, ((Lexical.Word)Coding[n - 1]).line, ""));
                DealError();
                return false;
            }
            return true;
        }
        private void SetListValue(string name)                    //赋值
        {
            if (!ListCheck())  //检查访问是否出错
                return;
            ListType list = GetList(name);
            int index = int.Parse(result);
            if(list != null)
            {
                if (index >= list.Length)       //索引大于数组长度
                {
                    Errors.Add(new Lexical.Error(13, ((Lexical.Word)Coding[n]).line, ""));
                    DealError();
                    return;
                }
                next = GetNext(ref n);         //跳过等号
                Expression();
                if (!TypeCheck(list.Type))
                {
                    Errors.Add(new Lexical.Error(18, ((Lexical.Word)Coding[n - 1]).line, ""));
                    DealError();
                    return;
                }
                list.SetValue(index, result);  //给数组赋值
            }
            else  //多维数组赋值
            {
                ArrayType array = GetArray(name);
                if (array != null)
                {
                    int sum = GetArrayIndex(array);
                    next = GetNext(ref n);  //跳过等号
                    Expression();
                    if (!TypeCheck(array.Type))
                    {
                        Errors.Add(new Lexical.Error(18, ((Lexical.Word)Coding[n - 1]).line, ""));
                        DealError();
                        return;
                    }
                    array.SetValue(sum, result);
                }
            }
        }
        private void GetListValue(string name)                    //获取数组值
        {
            if (!ListCheck())
                return;
            ListType list = GetList(name);
            if (list != null)
            {
                if (int.Parse(result) >= list.Length)  //索引大于数组长度
                {
                    Errors.Add(new Lexical.Error(13, ((Lexical.Word)Coding[n - 1]).line, ""));
                    DealError();
                    result = null;
                    return;
                }
                else
                {
                    if (list.Length != list.GetValue().Count)  //未初始化
                    {
                        Errors.Add(new Lexical.Error(17, ((Lexical.Word)Coding[n]).line, name));
                        DealError();
                        result = null;
                        return;
                    }
                    else  //给结果赋值
                        result = list.GetValue(int.Parse(result));
                }
            }
            else  //多维数组取值
            {
                ArrayType array = GetArray(name);
                if (array != null)
                {
                    int sum = GetArrayIndex(array);
                    result = array.GetValue(sum);
                }
            }
        }
        private void ValuedFunction(string name)              //根据参数调用函数
        {
            level++;
            isFunction = true;
            next = GetNext(ref n);  //跳过左侧小括号
            FunctionType function = GetFunction(name);
            this.name = name;
            ArrayList value = new ArrayList();
            ArrayList names = function.GetNames();
            while (n < Coding.Count - 1 && next != ")")  //获取调用函数的参数
            {
                Expression();
                value.Add(result);
                if (next == ",")
                    next = GetNext(ref n);
            }
            next = GetNext(ref n);  //跳过右括号
            VarType v;
            for (int i = 0; i < value.Count; i++)  //函数参数赋值
            {
                v = GetVar((string)names[i]);
                result = (string)value[i];
                if (v != null)
                {
                    if (result != null && TypeCheck(v.Type))  //类型检查
                    {
                        v.Value = result;
                        v.IsValued = true;
                    }
                    else
                    {
                        Errors.Add(new Lexical.Error(18, ((Lexical.Word)Coding[n - 1]).line, ""));
                        DealError();
                        return;
                    }
                }
            }
            int temp = n;                //保存此时的索引

            n = function.Index - 1;  //调用函数，执行函数体
            next = GetNext(ref n);
            int temp2 = level;         //存储分析前的level值
            while (n < Coding.Count - 1 && (next != "}" || temp2 != level))
                Analysis();

            n = temp - 1;
            next = GetNext(ref n);
            this.name = null;
            isFunction = false;
            level--;
        }
        private bool IsLenghtRight()                                    //数组长度或索引不能出现标识符
        {
            while(n < Coding.Count && next != "]")
            {
                if (IsLetter(next[0]))
                    return false;
                next = GetNext(ref n);

            }
            return true;
        }
        private int GetTerminal(string type)                         //获取循环结束位置
        {
            int terminal = 0;
            switch (type)
            {
                case "do":
                    next = GetNext(ref n);  //跳过do
                    if (next == "{")
                    {
                        next = GetNext(ref n);
                        while (n < Coding.Count - 1 && next != "}")
                        {
                            next = GetNext(ref n);
                            if(next == "{")
                            {
                                while (n < Coding.Count - 1 && next != "}")
                                    next = GetNext(ref n);
                                next = GetNext(ref n);
                            }
                        }
                    }
                    while (n < Coding.Count - 1 && next != ";")
                    {
                        next = GetNext(ref n);
                        if (next == "while")
                            JudgeIndex = n;
                    }
                    next = GetNext(ref n);
                    terminal = n;
                    break;

                case "for":
                case "while":
                    DealError();
                    terminal = n;
                    break;
            }
            return terminal;
        }
        private int GetArrayIndex(ArrayType array)             //根据表达式获取多维数组总的索引
        {
            if (int.Parse(result) >= array.Length)  //索引大于数组长度
            {
                Errors.Add(new Lexical.Error(13, ((Lexical.Word)Coding[n - 1]).line, ""));
                DealError();
                result = null;
                return 0;
            }
            int sum = int.Parse(result);  //计算总的索引
            int i = 0;
            sum = sum * array.GetSum(i);
            while (n < Coding.Count - 1 && next == "[")
            {
                if (!ListCheck())
                    return 0;
                i++;
                int index = int.Parse(result);  //获得索引
                if (index > array.Length)
                {
                    Errors.Add(new Lexical.Error(13, ((Lexical.Word)Coding[n - 1]).line, ""));
                    DealError();
                    result = null;
                    return 0;
                }
                index = index * array.GetSum(i);
                sum += index;
            }
            return sum;
        }
        private void DealError()                                            //错误处理，发生错误，跳过当前行
        {
            int line = ((Lexical.Word)Coding[n]).line;  //获取错误行
            if (isJudge != 0 || isLoop != 0) //分支语句错误
            {
                bool isAppear = false;
                while (next != "else" && n < Coding.Count - 1 && ((Lexical.Word)Coding[n]).line <= line + 1)
                {
                    if (next == "{")
                    {
                        isAppear = true;
                        int count = 1;
                        while (n < Coding.Count - 1 && count != 0)
                        {
                            next = GetNext(ref n);
                            if (next == "{")
                                count++;
                            if (next == "}")
                                count--;
                        }

                        if (!isFunction)
                            next = GetNext(ref n);  //跳过右侧大括号
                        break;
                    }
                    next = GetNext(ref n);
                }
                if (next != "else" && !isAppear)          //未出现大括号，跳过；
                    next = GetNext(ref n);
                //if (isLoop)
                //    return;
                while (next == "else")
                {
                    next = GetNext(ref n);
                    if (next == "if")  //else if
                    {
                        next = GetNext(ref n);
                        if(next == "(")
                        {
                            int count = 1;
                            while (n < Coding.Count - 1 && count != 0)
                            {
                                next = GetNext(ref n);
                                if (next == "(")
                                    count++;
                                if (next == ")")
                                    count--;
                            }
                            next = GetNext(ref n);
                        }
                       
                        if (next == "{")
                        {
                            int count = 1;
                            while (n < Coding.Count - 1 && count != 0)
                            {
                                next = GetNext(ref n);
                                if (next == "{")
                                    count++;
                                if (next == "}")
                                    count--;
                            }
                            next = GetNext(ref n);
                        }
                        else
                        {
                            while (n < Coding.Count - 1 && next != ";")
                                next = GetNext(ref n);
                            next = GetNext(ref n);
                        }
                    }
                    else if (next == "{")   //else {}
                    {
                        int temp = level;            //存储分析前的level值
                        while (n < Coding.Count - 1 && (next != "}" || temp != level))
                            next = GetNext(ref n);
                        next = GetNext(ref n);
                    }
                    else                   //else  单条语句
                    {
                        while (n < Coding.Count - 1 && next != ";")
                            next = GetNext(ref n);
                        next = GetNext(ref n);
                    }
                }
            }
            else                   //单行错误
            {
                while (n < Coding.Count - 1 && ((Lexical.Word)Coding[n]).line < line + 1 && next != ";")
                    next = GetNext(ref n);
                next = GetNext(ref n);
            }
        }
    }
}
