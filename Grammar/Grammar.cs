using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

//使用LL(1)文法进行语义分析
namespace Interpreter.Grammar
{
    class Grammar
    {
        Expression e = new Expression();
        public string[] inputExperssion;
        public string startCharacter = " ";                                                           //文法的开始符号
        public string endCharacter = "$";                                                           //结束符
        public string initCharacter = " ";                                                             //预测分析表默认元素
        public char empty = '~';                                                                         //表示为空
        public Hashtable expression = new Hashtable();                                   //建立非终结符与表达式的映射关系，如 E->TA表示为E : TA
        public Hashtable firstSet = new Hashtable();                                         //保存first集
        public Hashtable followSet = new Hashtable();                                     //保存follow集
        public Hashtable expressionFirst = new Hashtable();                            //保存某一表达式对应的first集合
        public Hashtable predictTable = new Hashtable();                                //预测分析表
        public HashSet<string> terminator = new HashSet<string>();             //终结符集合
        public HashSet<string> nonTerminator = new HashSet<string>();      //非终结符集
        public TreeView treeView = new TreeView();                                         //存储语法树信息
        public ArrayList treeNodes = new ArrayList();                                        //保存语法树所有非终结符结点

        public Grammar()                                                                                   //构造函数
        {
            inputExperssion = e.expression;
        }
        public void Init()                                                                                     //从表达式中提取出first集
        {
            bool isInitialized = false;                                                                    //startCharacter是否已经初始化
            foreach (string input in inputExperssion)                                          //遍历文法,初始化终结符和非终结符集合
            {
                string[] str = input.Split(new string[] { "->" }, 0);                           //依据"->"分割字符串
                if (!isInitialized)
                {
                    startCharacter = str[0];                                                               //初始化
                    isInitialized = true;
                }
                nonTerminator.Add(str[0]);                                                            //非终结符集合
                firstSet.Add(str[0], new HashSet<string>());                                 //建立非终结符与first集的映射关系
                followSet.Add(str[0], new HashSet<string>());                             //建立非终结符与follow集的映射关系
                expression.Add(str[0], str[1]);                                                        //非终结符与文法的映射关系

                string[] right = str[1].Split(new string[] { "|  " }, 0);                         //依据"|"分割字符串
                foreach (string i in right)                                                               //遍历右侧表达式的每一种可能
                {
                    expressionFirst.Add(str[0] + "->" + i, new HashSet<string>());
                    string[] charaters = i.Split(' ');
                    foreach (string j in charaters)
                    {
                        if (j.Length > 0 && !IsCapitalLetter(j[0]))
                            terminator.Add(j);                                                              //添加至终结符集合
                    }
                }
            }
            ((HashSet<string>)followSet[startCharacter]).Add(endCharacter); //开始符号follow集添加结束符：$
            foreach (string i in nonTerminator)
                GetFirst(i);                                                                                     //调用函数，获取非终结符的first集
            foreach (string i in nonTerminator)
                GetFollow(i);                                                                                 //调用函数求follow集（求follow集需要先求出first集，因此再次遍历）
            BuildPredictTable();                                                                          //构建预测分析表
        }
        private bool IsCapitalLetter(char charater)
        {
            if ('A' <= charater && charater <= 'Z')
                return true;
            else
                return false;
        }                                         //判断是否为大写字母
        public bool IsInside(string v, string non)                                               //判断表达式v中是否包含非终结符non
        {
            string[] str = v.Split(' ');
            foreach (string s in str)
                if (s == non)
                    return true;
            return false;
        }
        public int GetNumber(string[] vs, string non)                                       //返回数组中包含非终结符non的个数
        {
            int num = 0;
            foreach (string v in vs)
                if (v == non)
                    num++;
            return num;
        }
        private ArrayList PrintSet(int type)                                                        //根据类型输出hashtable数据
        {
            Hashtable set = new Hashtable();
            ArrayList infos = new ArrayList();
            String kind = "First(";
            switch (type)
            {
                case 1:
                    set = firstSet;
                    break;
                case 2:
                    kind = "Follow(";
                    set = followSet;
                    break;
                case 3:
                    set = expressionFirst;
                    break;
            }
            foreach (DictionaryEntry dictionary in set)
            {
               infos.Add(kind + dictionary.Key + "): ");
                foreach (string i in (HashSet<string>)dictionary.Value)
                    infos.Add(i + "  ");
                infos.Add("\n");
            }
            return infos;
        }
        public  bool IsDecimal(string value)                                                     //判断字符串是否为小数
        {
            return Regex.IsMatch(value, @"^[+-]?\d.{1}?\d*$");
        }
        public bool isNumberic(string value)                                                   //判断字符串是否可数字化
        {
            return Regex.IsMatch(value, @"^[+-]?\d*[.]?\d*$");
        }
        public string ArrayToString(ArrayList stack)                                         //获取分析栈字符串表示
        {
            string info = " ";
            if (stack.Count <= 0)
                return info;
            for (int i = stack.Count - 1; i >= 0; i--)
                info += stack[i];
            return info;
        }
        public  bool IsInt(string value)                                                              //判断某一字符串是否为整数
        {
            return Regex.IsMatch(value, @"^[+-]?\d*$");
        }
        private void BuildPredictTable()                                                            //构建预测分析表
        {
            foreach (string non in nonTerminator)                                            //初始化预测分析表
            {
                Hashtable hashtable = new Hashtable();
                foreach (string t in terminator)
                {
                    if (t.Trim() == empty.ToString())
                        hashtable.Add(endCharacter, initCharacter);
                    else
                        hashtable.Add(t, " ");                                                                //以“”初始化每一项
                }
                predictTable.Add(non, hashtable);
            }

            foreach (string input in inputExperssion)
            {
                Hashtable hashtable = new Hashtable();
                string[] str = input.Split(new string[] { "->" }, 0);
                string[] rights = str[1].Split(new string[] { "|  " }, 0);
                foreach (string right in rights)                                                      //遍历右侧所有推导
                {
                    HashSet<string> first = (HashSet<string>)expressionFirst[str[0] + "->" + right];
                    foreach (string i in first)
                        if (i.Trim() != empty.ToString())
                            ((Hashtable)predictTable[str[0]])[i] = right;

                    if (first.Contains(empty.ToString()))                                          //包含空，另需要求follow集
                    {
                        HashSet<string> follow = (HashSet<string>)followSet[str[0]];
                        foreach (string f in follow)
                        {
                            if ((string)(((Hashtable)predictTable[str[0]])[f]) != initCharacter)
                                ((Hashtable)predictTable[str[0]])[f] = ((Hashtable)predictTable[str[0]])[f] + "#" + right;
                            else
                                ((Hashtable)predictTable[str[0]])[f] = right;
                        }   
                    }
                }
            }
        }
        private void PrintPredictTable()                                                            //输出预测分析表
        {
            Console.WriteLine("预测分析表：");
            Console.Write("N/T\t");
            foreach (string t in terminator)
            {
                if (t.Trim() == empty.ToString())
                    Console.Write(endCharacter + "\t");
                else
                    Console.Write(t + "\t");
            }
            Console.WriteLine();

            foreach (DictionaryEntry dictionary in predictTable)
            {
                Console.Write(dictionary.Key + "\t");
                foreach (string t in terminator)
                {
                    if (t.Trim() == empty.ToString())
                        Console.Write(((Hashtable)(dictionary.Value))[endCharacter] + "\t");
                    else
                        Console.Write(((Hashtable)(dictionary.Value))[t] + "\t");
                }
                Console.WriteLine();
            }
        }
        private ArrayList GetIndex(string non, string[] vs)                               //获取non下标
        {
            ArrayList array = new ArrayList();
            for (int i = 0; i < vs.Length; i++)
                if (vs[i] == non)
                    array.Add(i);
            return array;
        }
        private void GetFirst(string non)                                                          //计算获取non(非终结符)的first集
        {
            HashSet<string> first = new HashSet<string>();                            //用于存储right的first集
            string rightExpression = (string)expression[non];                           //获取非终结符non推导出来的表达式   
            string[] rights = rightExpression.Split(new string[] { "|  " }, 0);        //分割右侧表达式
            foreach (string right in rights)                                                         //循环每个表达式
            {
                HashSet<string> f = new HashSet<string>();                            //计算右侧某一推导的first集
                string[] str = right.Split(' ');                                                          //用空格分割非终结符（文法构造即遵循该规则如：E->T F）
                for (int i = 0; i < str.Length; i++)                                                  //进行遍历
                {
                    if (str[i] == non)
                        continue;
                    if (str[i] == "")                                                                          //避免定义的语法中少数空格带来的错误
                        continue;
                    if (str[i].Length > 0 && str[i][0] == empty)                             //当s为空时，停止循环
                    {
                        f.Add(str[i]);
                        break;
                    }

                    else if (str[i].Length > 0 && !IsCapitalLetter(str[i][0]))             //首字母为小写，不是非终结符
                    {
                        f.Add(str[i]);
                        break;
                    }
                    else                                                                                           //对应非终结符的情况
                    {
                        if(((HashSet<string>)firstSet[str[i]]).Count == 0)
                            GetFirst(str[i]);
                        HashSet<string> sFirst = (HashSet<string>)firstSet[str[i]];     //获取非终结符s对应的first集
                        foreach (string j in sFirst)                                                     //如果sFirst中包含空则判断是否为最后一个元素，若是则添加空；否则跳过
                        {
                            if (j.Trim() != empty.ToString())
                                f.Add(j);
                            if (j.Trim() == empty.ToString() && i == str.Length - 1)
                                f.Add(j);
                        }

                        f.UnionWith(sFirst);                                                             //将sFirst添加至first集合
                        if (!sFirst.Contains(empty.ToString()))                                 //如果s的first集中不包含空，则不用向下继续循环
                            break;
                    }
                }
                ((HashSet<string>)expressionFirst[non + "->" + right]).UnionWith(f);
                first.UnionWith(f);
            }
            ((HashSet<string>)firstSet[non]).UnionWith(first);                       //将求得的first集进行添加
        }
        private void GetFollow(string non)                                                      //计算Non(非终结符)的follow集
        {
            HashSet<string> follow = new HashSet<string>();                       //用于存储non的follow集
            Hashtable str = new Hashtable();                                                    //用于存储包含non的右侧式子
            foreach (DictionaryEntry dictionary in expression)
            {
                if (!((string)dictionary.Value).Contains(non))
                    continue;
                string[] vs = ((string)dictionary.Value).Split(new string[] { "|  " }, 0);
                HashSet<string> hashSet = new HashSet<string>();
                foreach (string v in vs)
                    if (IsInside(v, non))                                                                  //获取右侧包含non的推导
                        hashSet.Add(v);
                if (hashSet.Count != 0)
                    str.Add(dictionary.Key, hashSet);                                            //求解non的follow，可能会用到左侧的follow集
            }

            foreach (DictionaryEntry dictionary1 in str)                                    //遍历右侧所有包含non的推导，求follow集
                foreach (string i in (HashSet<string>)dictionary1.Value)
                {
                    HashSet<string> f = new HashSet<string>();                      //存储每个推导的follow集
                    char[] chspilt = new char[] { ' ' };                                           //去除数组中的空元素
                    string[] vs = i.Split(chspilt, StringSplitOptions.RemoveEmptyEntries);
                    int num = GetNumber(vs, non);                                           //获取vs中非终结符non个数
                    ArrayList index = GetIndex(non, vs);                                    //获取non在数组vs中的下标
                    while (num > 0)
                    {  
                        int j = (int)index[num - 1];
                        if (vs[j] == non)
                            num--;
                        if ((((string)dictionary1.Key == "Loop1" || (string)dictionary1.Key == "Loop4" ) && vs[j] == "Judge") || ((string)dictionary1.Key == "Judge2" && vs[j] == "Judge3"))
                        {
                            f.UnionWith((HashSet<string>)followSet[(string)dictionary1.Key]);
                            continue;
                        }

                        if (j == vs.Length - 1 && vs[j] != (string)dictionary1.Key)   //如果non在数组中是最后一个元素，且non不等于左侧非终结符,则follow(左侧)属于follow(non)
                        {
                            if (((HashSet<string>)followSet[(string)dictionary1.Key]).Count == 0)
                                GetFollow((string)dictionary1.Key);
                            f.UnionWith((HashSet<string>)followSet[(string)dictionary1.Key]);
                        }
                        if (j < vs.Length - 1 && vs[j + 1] != "" && !IsCapitalLetter(vs[j + 1][0]))        //如果non后是终结符，直接添加
                        {
                            f.Add(vs[j + 1]);
                            if(num == 0)
                                break;
                        }
                        if (j < vs.Length - 1 && vs[j + 1] != "" && IsCapitalLetter(vs[j + 1][0]))          //如果non后是非终结符，则判断其first集中是否包含空，若包含继续往后判断
                        {
                            HashSet<string> vs1 = (HashSet<string>)firstSet[vs[j + 1]];                      //保存vs[j+1]的first集
                            foreach (string k in vs1)                                                //follow集中不包含空
                                if (k.Trim() != empty.ToString())
                                    f.Add(k);
                            if(vs1.Contains(empty.ToString()) && j + 1 == vs.Length - 1)                     //如A->BC，若first(C)包含空,则follow(A)属于follow(B)
                            {
                                if (((HashSet<string>)followSet[(string)dictionary1.Key]).Count == 0)
                                    GetFollow((string)dictionary1.Key);
                                f.UnionWith((HashSet<string>)followSet[(string)dictionary1.Key]);
                            }
                            if (!vs1.Contains(empty.ToString()) && num == 0)     //不包含空则停止循环
                                break;
                        }
                    }
                    if (f.Count != 0)
                        follow.UnionWith(f);
                }
            ((HashSet<string>)followSet[non]).UnionWith(follow);               //进行添加
        }
        
        public ArrayList SyntaxAnalysis(ArrayList coding, ArrayList errors)    //语法分析
        {
            int index = 0;                                                                                 //下标      
            if (coding.Count <= 0)
                return null;
            ArrayList stack = new ArrayList();                                                  //分析栈
            ArrayList infos = new ArrayList();                                                  //保留分析过程
            infos.Add("-------------------------------------------------------------------------------------------");
            infos.Add("分析栈\t\t\t\t\t输入栈栈顶\t\t动作\n");
            infos.Add("-------------------------------------------------------------------------------------------");
            stack.Add(endCharacter);                                                              //添加结束符和开始符
            stack.Add(startCharacter);
            
            int near = 0;                                                                                   //存储最近一次出现的if行数
            int near2 = 0;                                                                                 //存储最近一次出现的while行数
            bool once = false;
            bool isFirst = true;                                                                        //判断是否为根节点
            while (true)
            {
                if (stack.Count == 0)
                {
                    if (errors.Count == 0)
                        infos.Insert(0, "Accept!\n");
                    break;
                }
                if (index >= coding.Count)
                    break;
                string kind = ((Lexical.Word)coding[index]).value;                   //获取Word数值
                string nodeValue = kind;                                                          //用于构建语法树
                int tag = ((Lexical.Word)coding[index]).tag;                             //获取Word标签
                if (tag == 5)
                    kind = "identifier";
                if(tag == 4)
                    kind = "constant";

                if (IsCapitalLetter(((string)stack[stack.Count - 1])[0]))               //如果栈顶为非终结符
                {
                    Hashtable hashtable = (Hashtable)predictTable[stack[stack.Count - 1]];
                    string strs = (string)hashtable[kind];                                    //查找预测表
                    string[] all = strs.Split('#');
                    string str = " ";
                    if (all.Length == 1)                                                                //预测表项冲突的情况
                        str = all[0];
                    else
                        foreach (string a in all)
                        {
                            if (a != empty.ToString())
                                str = a;

                        }

                    if (str != initCharacter && str.Trim() != empty.ToString())   //当对应的推导不为空或者出错时
                    {
                        string info = string.Format("{0,-50} | {1,-15} | {2, -30}", ArrayToString(stack), ((Lexical.Word)coding[index]).value, stack[stack.Count - 1] + "->" +str);
                        infos.Add(info);
                        stack.RemoveAt(stack.Count - 1);                                     //移除栈顶元素
                        string[] s = str.Split(' ');
                        for (int i = s.Length - 1; i >= 0; i--)                                   //将对应的动作添加入栈
                            if(s[i] != "")
                                stack.Add(s[i]);

                        if (isFirst == true)                                                               //从根节点开始构建语法树
                        {
                            for (int i = s.Length - 1; i >= 0; i--)
                                if (s[i] != "")
                                {
                                    if (s[i] == "constant" || s[i] == "identifier")
                                        treeView.Nodes.Add(nodeValue);
                                    else
                                        treeView.Nodes.Add(s[i]);
                                }
                            isFirst = false;
                            for (int i = treeView.Nodes.Count - 1; i >= 0; i--)
                                if (IsCapitalLetter(treeView.Nodes[i].Text[0]))            //保存非终结符结点
                                    treeNodes.Add(treeView.Nodes[i]);
                        }
                        else                                                                                    //从子节点继续构建语法树
                        {
                            TreeNode treeNode = (TreeNode)treeNodes[0];         //由该节点向下长树
                            for (int i = s.Length - 1; i >= 0; i--)
                                if (s[i] != "")
                                {
                                    if (s[i] == "constant" || s[i] == "identifier")
                                        treeNode.Nodes.Add(nodeValue);
                                    else
                                        treeNode.Nodes.Add(s[i]);
                                }
                            treeNodes.RemoveAt(0);
                            for (int i = 0; i < treeNode.Nodes.Count; i++)
                                if (IsCapitalLetter(treeNode.Nodes[i].Text[0]))            //保存非终结符结点
                                    treeNodes.Insert(0, treeNode.Nodes[i]);
                        }
                    }
                    else if (str.Trim() == empty.ToString())                                 //移除栈顶代表空的符号"~"
                    {
                        //TreeNode treeNode = (TreeNode)treeNodes[0];         //由该节点向下长树
                        //treeNode.Nodes.Add(empty.ToString());
                        //treeNodes.RemoveAt(0);
                        string info = string.Format("{0,-50} | {1,-15} | {2, -30}", ArrayToString(stack), ((Lexical.Word)coding[index]).value, stack[stack.Count - 1] + "->" + str);
                        infos.Add(info);
                        stack.RemoveAt(stack.Count - 1);
                    }

                    else if(str == initCharacter)                                                    //不匹配，进行错误处理
                    {
                        while (IsCapitalLetter(((string)stack[stack.Count - 1])[0]))  //移除栈顶非终结符元素
                            stack.RemoveAt(stack.Count - 1);
                        int line = ((Lexical.Word)coding[index]).line;
                        string info = " ";
                        if ((string)stack[stack.Count - 1] != "$")
                            info = "missing " + (string)stack[stack.Count - 1];
                        else
                            info = kind;
                        errors.Add(new Lexical.Error(9, line, info));
                        once = true;
                    }
                }

                if (((Lexical.Word)coding[index]).value == "if")
                    near = ((Lexical.Word)coding[index]).line;

                if ((string)stack[stack.Count - 1] == kind)                               //分析栈和输入栈栈顶符号相同则出栈
                {
                    string info = string.Format("{0,-50} | {1,-15} | {2, -30}", ArrayToString(stack), ((Lexical.Word)coding[index]).value, "出栈");
                    infos.Add(info);
                    stack.RemoveAt(stack.Count - 1);
                    index++;
                    continue;
                }

                if(!IsCapitalLetter(((string)stack[stack.Count - 1])[0]))             //分析栈为终结符，但与输入栈栈顶元素并不相等
                {
                    int line = ((Lexical.Word)coding[index]).line;
                    if(once == false)
                    {
                        string info = " ";
                        if ((string)stack[stack.Count - 1] != "$")
                            info = "missing " + (string)stack[stack.Count - 1];
                        else
                            info = kind;
                        errors.Add(new Lexical.Error(9, line, info));
                    }

                    stack.Clear();                                                                       //清空分析栈元素，分析下一语句
                    stack.Add(endCharacter);                                                  //添加结束符和开始符
                    stack.Add(startCharacter);

                    if (near == line)                                                                   //if分支错误，跳过所有if、else代码块
                    {
                        while(index < coding.Count && ((Lexical.Word)coding[index]).line <= line + 1)    //跳过代码块
                        {
                            if (((Lexical.Word)coding[index]).value == "{")
                                while (index < coding.Count && (((Lexical.Word)coding[index]).value != "}"))
                                    index++;
                            index++;
                        }

                        while(index < coding.Count && ((Lexical.Word)coding[index]).value == "else")     //跳过else分支
                        {
                            int temp = ((Lexical.Word)coding[index]).line;
                            index++;
                            while (index < coding.Count && ((Lexical.Word)coding[index]).line <= temp + 1)
                            {
                                if (((Lexical.Word)coding[index]).value == "{")
                                    while (index < coding.Count && (((Lexical.Word)coding[index]).value != "}"))
                                        index++;
                                index++;
                            }
                            index++;
                        }
                    }

                    if (near2 == line)                                                                 //处理while错误
                    {
                        while (index < coding.Count && ((Lexical.Word)coding[index]).line <= line + 1)    //跳过代码块
                        {
                            if (((Lexical.Word)coding[index]).value == "{")
                                while (index < coding.Count && (((Lexical.Word)coding[index]).value != "}"))
                                    index++;
                            index++;
                        }
                    }

                    if(near != line)
                    {
                        while (index < coding.Count)                                       //跳过出错行
                        {
                            if (((Lexical.Word)coding[index]).line != line + 1)
                                index++;
                            else break;
                        }
                    }

                    if (index < coding.Count && (string)stack[stack.Count - 1] == startCharacter && (string)((Hashtable)predictTable[startCharacter])[((Lexical.Word)coding[index]).value] == empty.ToString())
                        index++;
                }

                //S$      输入栈：} else的情况
                if (index < coding.Count && (string)stack[stack.Count - 1] == startCharacter && (string)((Hashtable)predictTable[startCharacter])[((Lexical.Word)coding[index]).value] == initCharacter)
                    index++;
            }
            //foreach (string i in infos)
            //    Console.WriteLine(i);
            return infos;
        }
    }
}