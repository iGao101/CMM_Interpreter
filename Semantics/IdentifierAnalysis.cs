using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Interpreter.Semantics
{
    //完善标识符信息，比如层数、类型等
    class IdentifierAnalysis
    {
        public ArrayList Coding = new ArrayList();                  //token集合
        public ArrayList Errors = new ArrayList();                    //错误集合
        public ArrayList VarSet = new ArrayList();                   //保存变量
        public ArrayList ListSet = new ArrayList();                   //保存集合
        public ArrayList FunctionSet = new ArrayList();          //函数集合
        public int i = 0;                                                             //Coding索引
        public int level = 0;                                                       //表示层数，当进入if while等代码块时自增
        public int count = 0;                                                     //用于区分不同分支的局部变量
        public bool isFunction = false;                                     //判断是否为函数
        public int isJudge = 0;                                                 //判读是否为判断语句
        public int  isLoop = 0;                                                  //判断是否为循环语句
        public int isBlock = 0;                                                   //判断是否为代码块
        public bool isFor = false;
        public bool isSingle = false;                                         //辅助计算局部变量作用域
        public bool isBelongToJudge = false;
        public bool isValued = false;                                       //是否初始化
        public string name = null;                                            //标识符名称
        public string next = null;                                              //下一个token
        public int type = -1;                                                     //标识符类型
        FunctionType funtion;                                                  //函数

        public IdentifierAnalysis(ArrayList arrayList, ArrayList errors) { Coding = arrayList; Errors = errors; }
        //
        //待完善内容 else if 和 函数的return
        //
        public string GetNext(ref int i)        //取下一个标识符值
        {
            if (i < Coding.Count - 1)
            {
                i++;
                string s = ((Lexical.Word)Coding[i]).value;
                return s;
            }
            return null;
        }
        public void Init()                              //遍历Coding，设置标识符类型等信息
        {
            while (i < Coding.Count - 1)
            {
                Analysis();
            }
        }
        private void Analysis()
        {
            string value = ((Lexical.Word)Coding[i]).value;
            int tag = ((Lexical.Word)Coding[i]).tag;
            if (tag == 5)
                value = "identifier";
            switch (value)
            {
                case "int":           //可能定义局部变量的情况
                case "double":
                case "float":
                case "char":
                case "boolean":
                    if(!isBelongToJudge)
                        isSingle = true;
                    Assignment();
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
                    Block();
                    break;
                case "printf":
                    Printf();
                    break;
                case "scanf":
                    Scanf();
                    break;
                case "void":
                    type = 5;                      //void type值为5
                    next = GetNext(ref i);  
                    name = next;               //函数名
                    next = GetNext(ref i);  //使i指向函数左括号
                    Function();
                    break;
                case "return":
                    if(!isFunction)              //非函数体的return报错
                        Errors.Add(new Lexical.Error(10, ((Lexical.Word)Coding[i]).line, ""));
                    Return();
                    break;
                case "continue":
                case "break":
                    if(isLoop == 0)                    //非循环结构出现continue break报错
                        Errors.Add(new Lexical.Error(10, ((Lexical.Word)Coding[i]).line, ""));
                    Jump();
                    break;
                case "identifier":
                    if (!Check())
                        Errors.Add(new Lexical.Error(7, ((Lexical.Word)Coding[i]).line, name));
                    next = GetNext(ref i);
                    if(next == "[")
                    {
                        next = GetNext(ref i);
                        string num = next;                  //判断[]中是否为常数
                        next = GetNext(ref i);
                        if (int.Parse(num) < 0 && next == "]")
                            Errors.Add(new Lexical.Error(11, ((Lexical.Word)Coding[i]).line, ""));
                    }
                    Jump();
                    break;
                default:   //比如赋值、输出语句等，直接跳过
                    Jump();
                    break;
            }
        }
        private void Assignment()               //声明语句
        {
            type = -1;                       //标识符类型默认为-1
            string value = ((Lexical.Word)Coding[i]).value;
            name = GetNext(ref i);  //标识符名称

            next = GetNext(ref i);    //标识符后跟元素，可能是 ; = [ , )
            switch (value)                          //标识符类型
            {
                case "int":
                    type = 0;
                    break;
                case "double":
                    type = 1;
                    break;
                case "float":
                    type = 2;
                    break;
                case "char":
                    type = 3;
                    break;
                case "boolean":
                    type = 4;
                    break;
            }
            if (next == ";")   //仅声明，未赋值 
            {
                SaveVar();
                next = GetNext(ref i);
            }
            if (next == "," && isFunction)  //函数参数
            {
                SaveVar();
                while (i < Coding.Count - 1 && next != ")")
                {
                    if (next == ",")
                    {
                        next = GetNext(ref i);   //函数参数比较特别: int x, int y
                        MulAssignment();
                    }
                    else
                        next = GetNext(ref i);
                }
            }

            if(next == "=" && isFunction)
            {
                isValued = true;
                SaveVar();
                isValued = false;
                while (i < Coding.Count - 1 && next != ",")
                    next = GetNext(ref i);
                next = GetNext(ref i);
            }

            if (next == "," && !isFunction)   //多声明
            {
                SaveVar();
                SaveMul();
            }

            if (next == "=" && !isFunction)   //多声明
            {
                isValued = true;
                SaveVar();
                isValued = false;
                SaveMul();
            }

            if (next == "[")      //数组
            {
                Array();
                SaveMul();
            }

            if(next == "(")       //函数
            {
                Function();
            }
        }
        private void SaveMul()                    //遍历多声明语句
        {
            while (i < Coding.Count - 1 && next != ";")
            {
                if (next == ",")
                    MulAssignment();
                else
                    next = GetNext(ref i);
            }
            next = GetNext(ref i);
        }
        private void MulAssignment()        //多声明
        {
            switch (next)
            {
                case "int":
                    type = 0;
                    break;
                case "double":
                    type = 1;
                    break;
                case "float":
                    type = 2;
                    break;
                case "char":
                    type = 3;
                    break;
                case "boolean":
                    type = 4;
                    break;
            }
            name = GetNext(ref i);
            next = GetNext(ref i);
            int range = GetRange();
            if (level == 0)
                range = 0;
            if (next == ";" || next == "," || (next==")" && isFunction))       //仅声明，未赋值
            {
                VarType v = null;
                if (level == 0)
                    v = new VarType(name, type, level, true, range);
                else
                    v = new VarType(name, type, level, range);

                if (!IsReDefined(name, level, range))
                    VarSet.Add(v);
                else
                    Errors.Add(new Lexical.Error(8, ((Lexical.Word)Coding[i]).line, name));
            }

            if (next == "=")   //声明并赋值
                VarSet.Add(new VarType(name, type, level, true, range));
            if(next == "[")     //数组情况
            {
                Array();
            }
            if (isFunction)
            {
                funtion.AddParm(type);        //添加参数类型
                funtion.AddName(name);
            }
        }
        private void Array()                         //数组
        {
            next = GetNext(ref i);
            string num = next;                  //判断[]中是否为常数

            int tag = ((Lexical.Word)Coding[i]).tag;
            next = GetNext(ref i);
            ListType list;
            int range = GetRange();
            if (level == 0)
                range = 0;
            if (tag == 4 && next == "]")  //数组长度已定
            {
                if (int.Parse(num) <= 0)
                {
                    Errors.Add(new Lexical.Error(11, ((Lexical.Word)Coding[i]).line, ""));
                    return;
                }
                else
                    list = new ListType(name, type, int.Parse(num), level, range);
            }
            else                                         //动态决定长度或者长度为表达式的情况
                list = new ListType(name, type, level, range);

            if (!IsReDefined(name, level, range))  //判断是否重定义
                ListSet.Add(list);
            else
                Errors.Add(new Lexical.Error(8, ((Lexical.Word)Coding[i]).line, name));

            string value = ((Lexical.Word)Coding[i +1]).value;  //向后看，判断数组是否初始化
            if (value == "=")  //int a[3]={1,2,3};
            {
                if(type == 3 && ((Lexical.Word)Coding[i + 2]).value.Contains("\""))  //此为字符串给char数组赋值的情况，MidCode有长度处理，因此这里略过
                {  
                    next = GetNext(ref i);
                    next = GetNext(ref i);
                }
                else
                {
                    int count = 1;  //数组元素个数
                    while (i < Coding.Count && next != "}")
                    {
                        next = GetNext(ref i);
                        if (next == ",")
                            count++;
                    }
                    list.Length = count;
                }
                next = GetNext(ref i); //跳过右侧大括号
            }
            else                                //int a[3];  int a[3], b;
                return;
        }
        private void Judge()                        //判断语句
        {
            isJudge++;
            count++;
            level++;
            while (i < Coding.Count - 1 && next != ")")    //不用考虑条件语句，因为（）中不涉及变量的定义
            {
                next = GetNext(ref i);
                ConditionCheck();
            }
            next = GetNext(ref i);     //指向{ 或者 其他语句
            if(next == "{")   //代码块
            {
                isSingle = true;
                next = GetNext(ref i);
                while (i < Coding.Count - 1 && next != "}")
                    Analysis();
                next = GetNext(ref i); //跳过右侧大括号
            }
            else                   //若不是 { 
            {
                isBelongToJudge = true;
                isSingle = false;
                Analysis();
                isBelongToJudge = false;
            }

            while(i < Coding.Count && next == "else")  //处理else if和else 分支
            {
                next = GetNext(ref i);
                if (next == "if")        //else if分支
                    Judge();
                else if(next == "{")  //else {}
                {
                    next = GetNext(ref i);
                    while (i < Coding.Count - 1 && next != "}")
                        Analysis();
                    next = GetNext(ref i); //跳过右侧大括号
                }
                else                          //else 语句
                    Analysis();
            }
            level--;
            isJudge--;
        }
        private void Block()                         //代码块
        {
            isSingle = true;
            isBlock++;
            count++;
            level++;            //层数自增
            next = GetNext(ref i);
            while(i < Coding.Count - 1 && next != "}")
                Analysis();
            next = GetNext(ref i);                  //跳过右侧大括号
            level--;
            isBlock--;
        }
        private void Jump()                         //跳过不需要分析的语句
        {
            int tag;
            while (i < Coding.Count && next != ";")
            {
                next = GetNext(ref i);
                tag = ((Lexical.Word)Coding[i]).tag;
                if (tag == 5 && !Check())
                    Errors.Add(new Lexical.Error(7, ((Lexical.Word)Coding[i]).line, name));
            }
            next = GetNext(ref i);
        }
        private void While()                         //while循环语句
        {
            isLoop++;
            Judge();  //代码流程和Judge一样
            isLoop--;
        }
        private void For()                             //for循环语句
        {
            i++;
            isFor = true;
            count++;
            isLoop++;
            next = GetNext(ref i);
            level++;
            if (next == "int" || next == "double" || next == "float" || next == "char" || next == "boolean")
                Assignment();

            while (i < Coding.Count && next != ")")  //不用考虑条件语句，因为后序不涉及变量的定义
            {
                next = GetNext(ref i);
                ConditionCheck();
            }
                
            next = GetNext(ref i);    //指向{ 或者 其他语句
            if (next == "{")      //代码块
            {
                isSingle = true;
                next = GetNext(ref i);
                while (i < Coding.Count && next != "}")
                    Analysis();
                next = GetNext(ref i);
                i++;                   //跳过右侧大括号
                level--;
            }
            else                       //若不是 { 
            {
                isSingle = false;
                Analysis();
                level--;
            }
            isLoop--;
            isFor = true;
        }
        private void Do()                             //do while循环语句
        {
            isLoop++;
            count++;
            level++;
            next = GetNext(ref i);    //指向{ 或者 其他语句
            if (next == "{")   //代码块
            {
                isSingle = true;
                next = GetNext(ref i);
                while (i < Coding.Count - 1 && next != "}")
                    Analysis();
                next = GetNext(ref i); //跳过右侧大括号
            }
            else                   //若不是 { 
            {
                isSingle = false;
                Analysis();
            }
            while (i < Coding.Count && next != ";")    //跳过while部分
            {
                next = GetNext(ref i);
                ConditionCheck();
            }
            next = GetNext(ref i);
            level--;
            isLoop--;
        }
        private void Scanf()                         //检查scanf中的标识符是否定义
        {
            next = GetNext(ref i);
            while (i < Coding.Count && next != ";")
            {
                next = GetNext(ref i);
                ConditionCheck();
            }
            next = GetNext(ref i);
        }
        private void Printf()                         //检查printf中的标识符是否定义
        {
            Scanf();
        }
        private void Function()                    //函数
        {
            count++;
            isFunction = true;
            next = GetNext(ref i);
            funtion = new FunctionType(name, type, level);
            level++;
            while (i < Coding.Count - 1 && next != ")")
                Assignment();
            next = GetNext(ref i);
            next = GetNext(ref i);  //跳过左侧大括号
            bool isReDefined = FunctionCheck(funtion);
            if (!isReDefined)  //未重定义
            {
                FunctionSet.Add(funtion);
                while (i < Coding.Count && next != "}")
                    Analysis();
            }
            else                    //重定义
                while (i < Coding.Count - 1 && next != "}")
                    next = GetNext(ref i);

            next = GetNext(ref i);  //跳过右侧大括号
            level--;
            isFunction = false;
        }
        private void Return()                       //返回值语句
        {
            while(i < Coding.Count - 1 && next != ";")
            {
                next = GetNext(ref i);
                ConditionCheck();
            }
            next = GetNext(ref i);
        }
        private void ConditionCheck()         //检查if 循环条件中是否存在未定义的变量
        {
            int tag = ((Lexical.Word)Coding[i]).tag;
            if (tag == 5 && !Check())
                Errors.Add(new Lexical.Error(7, ((Lexical.Word)Coding[i]).line, name));
        }
        private bool Check()                        //检查标识符是否声明
        {
            name = ((Lexical.Word)Coding[i]).value;
            int line = ((Lexical.Word)Coding[i]).line;
            foreach (VarType va in VarSet)
            {
                if (va.Name == name && va.Level <= level && (va.Count >= line || va.Level == 0))
                    return true;
            }
            foreach (ListType list in ListSet)
            {
                if (list.Name == name && list.Level <= level && (list.Count >= line || list.Level == 0))
                    return true;
            }
            foreach (FunctionType function in FunctionSet)
            {
                if (function.Name == name && function.Level <= level)
                    return true;
            }
            return false;
        }
        private bool FunctionCheck(FunctionType f)  //检查函数是否重定义
        {
            foreach(FunctionType function in FunctionSet)
            {
                if (f.IsSame(function))
                {
                    Errors.Add(new Lexical.Error(12, ((Lexical.Word)Coding[i]).line, f.Name));
                    return true;
                }
            }
            return false;
        }
        private void SaveVar()                      //保存标识符
        {
            int range = GetRange();
            if (level == 0)
                range = 0;
            VarType v = null;
            if (isValued || level == 0)
                v = new VarType(name, type, level, true, range);
            else
                v = new VarType(name, type, level, range);
            if (isFunction)
            {
                funtion.AddParm(type);        //添加参数类型
                funtion.AddName(name);
            }

            if (!IsReDefined(name, level, range))
                VarSet.Add(v);
            else
                Errors.Add(new Lexical.Error(8, ((Lexical.Word)Coding[i]).line, name));
        }
        private bool IsReDefined(string name, int level, int count)
        {
            foreach(VarType va in VarSet)
            {
                if (va.Name == name && va.Level == level && va.Count == count)
                    return true;
            }
            foreach(ListType list in ListSet)
            {
                if (list.Name == name && list.Level == level && list.Count == count)
                    return true;
            }
            return false;
        }
        private int GetRange()                   //获取局部变量的作用范围
        {
            int temp = i;
            if(((isJudge !=0 || isLoop != 0 || isBlock != 0) && isSingle) || isFunction || isFor)
            {
                int temp2 = count;
                while (i < Coding.Count - 1 && next != "}")
                    next = GetNext(ref i);
            }
            int index = ((Lexical.Word)Coding[i]).line;
            i = temp - 1;
            next = GetNext(ref i);
            return index;
        }
    }
}
