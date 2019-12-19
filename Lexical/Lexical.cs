using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
//@Author: 高战立 2017302580230
//@Data: 2019.10.16
//@Function: 此类用于从源文件中提取Token，做词法分析
namespace Interpreter.Lexical
{
    class Lexical
    {
        public ArrayList Coding = new ArrayList();                     //用于保存token
        public ArrayList Errors = new ArrayList();                       //保存错误信息

        public ArrayList ReadFromTxt(string path)                     //从文件中读取源程序并进行预处理
        {
            ArrayList codes = new ArrayList();                               //用于保存经过预取里的源程序
            string text = System.IO.File.ReadAllText(path);           //读取源程序至字符串中
            Console.WriteLine("源程序：");                                   //输出源程序
            Console.WriteLine(text);
            int line = 1;                                                                   //行数
            int i = 0;
            while(i < text.Length)                                                   //以字符遍历
            {
                if (text[i] == '\n')
                {
                    i++;
                    line++;
                    codes.Add('\n');                                                   //添加换行符，以便正确指示位置
                    continue;
                }
                   
                if (text[i] == '\t')                                                     //遇到制表符进行过滤，此处暂时不处理空格和换行
                {
                    i++;
                    continue;
                }
                   
                if(text[i] == '/' && i < text.Length - 1)                    //去除注释
                {
                    if(text[i+1] == '/')                                                //为单行注释
                    {
                        line++;
                        //codes.Add('\n');
                        while (text[i] != '\n' && i < text.Length)         //跳过改行数据
                            i++;
                        continue;
                    }

                    if(text[i+1] == '*')                                                //多行注释
                    {
                        while (i < text.Length - 1 && !(text[i] == '*' && text[i + 1] == '/'))
                        {
                            if (text[i] == '\n')
                            {
                                line++;
                                codes.Add('\n');
                            }       
                            i++;
                        }            
                        if (i == text.Length - 1)                                   // "*/" 缺失，错误码设置为4
                            Errors.Add(new Error(4, line, "/*"));               
                        if(text[i+2] == '\n' || text[i + 2] == '\r')
                        {
                            line++;                                                         //不出错情况下，跳出循环后i指向*号
                            //codes.Add('\n');
                        }
                        i += 3;                                                              //i+=2,跳过 */
                        continue;
                    }
                }
                codes.Add(text[i]);
                i++;
            }
            return codes;
        }
        public ArrayList ReadFromScreen(string text)                //从文件中读取源程序并进行预处理
        {
            ArrayList codes = new ArrayList();                               //用于保存经过预取里的源程序
            int line = 1;                                                                   //行数
            int i = 0;
            while (i < text.Length)                                                   //以字符遍历
            {
                if (text[i] == '\n')
                {
                    i++;
                    line++;
                    codes.Add('\n');                                                   //添加换行符，以便正确指示位置
                    continue;
                }

                if (text[i] == '\t')                                                     //遇到制表符进行过滤，此处暂时不处理空格和换行
                {
                    i++;
                    continue;
                }

                if (i < text.Length - 1 && text[i] == '/' )                    //去除注释
                {
                    if (text[i + 1] == '/')                                                //为单行注释
                    {
                        line++;
                        while (i < text.Length && text[i] != '\n' )         //跳过改行数据
                            i++;
                        continue;
                    }

                    if (text[i + 1] == '*')                                                //多行注释
                    {
                        while (i < text.Length - 1 && !(text[i] == '*' && text[i + 1] == '/'))
                        {
                            if (i < text.Length - 1 && text[i] == '\n')
                            {
                                line++;
                                codes.Add('\n');
                            }
                            i++;
                        }
                        if (i == text.Length - 1)                                   // "*/" 缺失，错误码设置为4
                            Errors.Add(new Error(4, line, "/*"));
                        if (i < text.Length - 1 && text[i + 2] == '\n' || text[i + 2] == '\r')
                        {
                            line++;                                                         //不出错情况下，跳出循环后i指向*号
                        }
                        i += 3;                                                              //i+=2,跳过 */
                        continue;
                    }
                }
                codes.Add(text[i]);
                i++;
            }
            return codes;
        }

        private bool IsLetter(char a)                                            //判断是否为字母
        {
            if (('a' <= a && a <= 'z') || ('A' <= a && a <= 'Z')) 
                return true;
            else
                return false;
        }
        private bool IsNumber(char a)                                        //判断是否为数字
        {
            if ('0' <= a && a <= '9')
                return true;
            else
                return false;
        }
        private bool IsNumberAndIdentifier()                            //判断Coding中最后一个元素是否为常数；
        {
            if (Coding.Count <= 0)
                return false;
            else
            {
                if (((Word)Coding[Coding.Count - 1]).tag == 4)
                    return true;
            }
            if (Token.Identifiers.Contains(((Word)Coding[Coding.Count - 1]).value) || ((Word)Coding[Coding.Count - 1]).value == ")" || ((Word)Coding[Coding.Count - 1]).value == "]")
                return true;
            return false;
        }
        private bool IsHexadecimal(char a)                                //判断是否为符合十六进制的字符
        {
            if (IsNumber(a) || ('a' <= a && a <= 'f') || ('A' <= a && a <= 'F'))
                return true;
            return false;
        }
        private bool IsBound(char a)                                          //判断是否是界限符
        {
            if (a == '(' || a == ')' || a == '{' || a == '}' || a == '[' || a == ']' || a == ':' || a == '?' || a == ';' || a==',')
                return true;
            else
                return false;
        }
        private bool IsOperators(char a)                                     //判断是否是运算符
        {
            if (a == '+' || a == '-' || a == '*' || a == '/' || a == '%' || a == '&' || a == '!' || a == '<' || a == '>' || a == '|'  || a == '=')
                return true;
            else
                return false;
        }
        private bool IsKeyWords(string temp)                            //判断是否为保留字
        {
            for(int i = 0; i < Token.KeyWords.Length; i++)
            {
                if (temp == Token.KeyWords[i])
                    return true;
            }
            return false;
        }
        public void GetToken(ArrayList array)                            //获取Token
        {
            int line = 1;                                                                 //记录行数
            int i = 0;                                                                      //遍历下标
            while(i < array.Count)                                                //遍历预处理后的源程序
            {
                if ((char)array[i] == '\n')                                        //遇到换行符行数自增
                {
                    line++;  
                    i++;   
                }
                if (i < array.Count && IsLetter((char)array[i]))                                             //当是字母时
                {
                    string temp = ((char)array[i++]).ToString();            //保存token
                    while ( i < array.Count && (IsLetter((char)array[i]) || IsNumber((char)array[i]) || (char)array[i] == '_'))
                        temp = temp + ((char)array[i++]).ToString();     //遍历添加

                    if (IsKeyWords(temp))                                             //如果是关键字
                        Coding.Add(new Word(1, line, temp));
                    else
                    {
                        if (temp[temp.Length - 1] != '_')                          //标识符不能以下划线结尾
                        {                                                                           //标识符类别
                            Coding.Add(new Word(5, line, temp));
                            Token.Identifiers.Add(temp);                          //添加至标识符数组
                        }
                        else                                                                     //标识符错误错误码定义为1
                            Errors.Add(new Error(1, line, temp));
                    }
                    continue;
                }

                //当是数字时
                bool isAppeared = false;
                if (i < array.Count && (IsNumber((char)array[i]) || (char)array[i] == '.' || (((char)array[i] == '+' || (char)array[i] == '-') && !isAppeared && !IsNumberAndIdentifier())))                                         
                {
                    if ((char)array[i] == '+' || (char)array[i] == '-')        //表示正负号已出现过
                        isAppeared = true;
                    string temp = ((char)array[i]).ToString();                //保存token
                    int type = 1;                                                            //数据有可能是不同进制，默认十进制
                    int num = 0;                                                           //小数点出现次数
                    bool isNum = false;                                               //进制标志符后需要出现满足条件的数字符号
                    bool isDecimal = true;                                          //判断是否为十进制

                    if (isAppeared == true)
                        while (i < array.Count && (char)array[i+1] == ' ')
                            i++;

                    //二进制前缀：0b   
                    if (i < array.Count - 1 && (char)array[i] == '0' && ((char)array[i + 1] == 'b' || (char)array[i + 1] == 'B'))
                    {
                        temp = temp + ((char)array[++i]).ToString();
                        type = 2;
                        i++;
                    }
                    //十六进制前缀：0x
                    if (i < array.Count - 1 && (char)array[i] == '0' && ((char)array[i + 1] == 'x' || (char)array[i + 1] == 'X'))
                    {
                        temp = temp + ((char)array[++i]).ToString();
                        type = 3;
                        i++;
                    }

                    if (type == 1)                                                                //对十进制常数的判断
                    {
                        if ((char)array[i] == '.')
                            num++;
                        i++;
                        while (i < array.Count && (IsNumber((char)array[i]) || (char)array[i] == '.'))
                        {
                            temp = temp + ((char)array[i]).ToString();         //遍历添加
                            if ((char)array[i] == '.')
                                num++;
                            i++;
                        }
                        // 4e+3 科学计数置为常数
                        if (i < array.Count - 1 && ((char)array[i] == 'e' || (char)array[i] == 'E') && ((char)array[i+1] == '-' || (char)array[i+1] == '+'))
                        {
                            temp = temp + ((char)array[i++]).ToString();
                            temp = temp + ((char)array[i++]).ToString();
                            while (i < array.Count && IsNumber((char)array[i]))     //循环添加科学计数法的指数部分
                                temp = temp + ((char)array[i++]).ToString();
                        }
                    }
                   
                    if(type == 2)                                                             //对二进制常数的判断
                    {
                        isDecimal = false;
                        while (i < array.Count && ((char)array[i] == '0' || (char)array[i] == '1' || (char)array[i] == '.'))
                        {
                            temp = temp + ((char)array[i]).ToString();       //遍历添加
                            if ((char)array[i] == '.')
                            {
                                if (temp[temp.Length - 2] == '0' || temp[temp.Length - 2] == '1')
                                    isNum = true;
                                num++;
                            }                                
                            i++;
                        }
                    }

                    if(type == 3)                                                              //对十六进制常数的判断
                    {
                        isDecimal = false;
                        while (i < array.Count && (IsHexadecimal((char)array[i]) || (char)array[i] == '.'))
                        {
                            temp = temp + ((char)array[i]).ToString();        //遍历添加
                            if ((char)array[i] == '.')
                            {                                                                         //0x.  这种情况报错
                                if ((temp[temp.Length - 2] != 'x' || temp[temp.Length - 1] != 'X') && temp[temp.Length - 3] != '0')
                                    isNum = true;                                   
                                num++;                                                          //小数点个数自增
                            }                               
                            i++;
                        }
                    }
                    if (num == 0)
                        isNum = true;

                    if(num > 1 || (isNum == false && isDecimal == false))
                        Errors.Add(new Error(2, line, temp));                  //常数定义错误错误码为2
                    else
                        Coding.Add(new Word(4, line, temp));               //常数标签为4
                    //预留常数越界的情况
                    continue;
                }

                if (i < array.Count && IsOperators((char)array[i]))                                     //当是运算符时
                {
                    char character = (char)array[i];
                    string temp = ((char)array[i++]).ToString();            //保存token
                    char next = ' ';
                    if (i < array.Count)
                        next = (char)array[i];
                    if (character == '^' || character == '~')
                    {
                        Coding.Add(new Word(3, line, temp));
                        continue;
                    }

                    switch (character)                                                    //最长匹配原则
                    {
                        case '=':
                            if (next == '=')                                                 //==
                            {
                                temp = temp + next.ToString();
                                i++;
                            }
                            break;
                        case '-':
                            if (next == '=' || next == '-')
                            {
                                temp = temp + next.ToString();
                                i++;
                            }
                            break;
                        case '+':
                            if (next == '=' || next == '+')
                            {
                                temp = temp + next.ToString();
                                i++;
                            }
                            break;
                        case '&':
                            if (next == '=' || next == '&')
                            {
                                temp = temp + next.ToString();
                                i++;
                            }
                            break;
                        case '|':
                            if (next == '=' || next == '|')
                            {
                                temp = temp + next.ToString();
                                i++;
                            }
                            break;

                        case '*':
                        case '/':
                        case '%':
                        case '!':
                        case '>':
                            if (next == '=')
                            {
                                temp = temp + next.ToString();
                                i++;
                            }
                            break;
                        case '<':
                            if (next == '=' || next == '>')
                            {
                                temp = temp + next.ToString();
                                i++;
                            }
                            break;

                    }
                    Coding.Add(new Word(3, line, temp));
                    continue;
                }

                if (i < array.Count && IsBound((char)array[i]))                                           //当是界限符时
                {
                    string temp = ((char)array[i++]).ToString();                 //保存token
                    if (i< array.Count && (char)array[i] == '\r')                 //文本换行为\r\n，所以指向\r时后移
                        i++;
                    Coding.Add(new Word(2, line, temp));
                    continue;
                }

                if(i < array.Count && (char)array[i] == '"')                                                  //处理字符常量
                {
                    string temp = ((char)array[i++]).ToString(); 
                    while (i < array.Count && (char)array[i] != '"')
                        temp = temp + ((char)array[i++]).ToString();
                    if(i < array.Count)
                    {
                        temp = temp + ((char)array[i++]).ToString();          //添加另一个 “
                        Coding.Add(new Word(4, line, temp));                  //字符串类型类别码定义为4
                    }
                    else                                                                               //另一个“缺失
                        Errors.Add(new Error(5, line,"\""));                           //错误码定义为5

                    continue;
                }

                if(i < array.Count && (char)array[i] == '\'')
                {
                    string temp = ((char)array[i++]).ToString();
                    int num = 0;
                    while(i < array.Count - 1 && (char)array[i] != '\'')
                    {
                        temp += ((char)array[i]).ToString();
                        if ((char)array[i] != '\\')
                            num++;
                        i++;
                    }
                    temp += ((char)array[i++]).ToString();
                    if(num <= 1)
                        Coding.Add(new Word(4, line, temp));                  //字符类型类别码定义为4
                    else
                        Errors.Add(new Error(6, line, temp));                           //错误码定义为6
                    continue;
                }

                if (i < array.Count && ((char)array[i] == ' ' || (char)array[i] == '\r'))           //跳过空格和换行符（文本换行好像为\r\n）
                    i++;
                else                                                                              //错误符号，错误码定义为0
                { 
                    if(i < array.Count && (char)array[i] != '\n')             //保证访问不越界
                    {
                        Errors.Add(new Error(0, line, ((char)array[i]).ToString()));
                        i++;
                    }
                }
            }
            Coding.Add(new Word(0, line, "$"));                          //添加结束符号
        }
        public ArrayList ErrorInfo()                                             //错误处理函数
        {
            ArrayList errorInfos = new ArrayList();
            if (Errors.Count > 0)
                errorInfos.Add(String.Format("\nThere are {0} errors in the program.", Errors.Count));
            for(int i = 0; i < Errors.Count; i++)
            {
                switch (((Error)Errors[i]).type)
                {
                    case 0:                               //错误码0代表非法字符      
                        errorInfos.Add(String.Format("Illegal character: {0} at lines {1}", ((Error)Errors[i]).value, ((Error)Errors[i]).line));
                        break;
                    case 1:                               //错误码1代表标识符错误
                        errorInfos.Add(String.Format("Illegal identifier: {0} at lines {1}", ((Error)Errors[i]).value, ((Error)Errors[i]).line));
                        break;
                    case 2:                              //错误码2代表常数错误
                        errorInfos.Add(String.Format("Illegal constant: {0} at lines {1}", ((Error)Errors[i]).value, ((Error)Errors[i]).line));
                        break;
                    case 3:    //非法运算符
                        errorInfos.Add(String.Format("Illegal operator: {0} at lines {1}", ((Error)Errors[i]).value, ((Error)Errors[i]).line));
                        break;
                    case 4:   //多行注释错误
                        errorInfos.Add(String.Format("*/ not found:  at lines {0}", ((Error)Errors[i]).line));
                        break;
                    case 5:
                        errorInfos.Add(String.Format("\" not found: at line {0}", ((Error)Errors[i]).line));
                        break;
                    case 6:
                        errorInfos.Add(String.Format("Char error: {0} at line {1}", ((Error)Errors[i]).value, ((Error)Errors[i]).line));
                        break;
                    case 7:
                        errorInfos.Add(String.Format("Identifier not defined: {0} at line {1}", ((Error)Errors[i]).value, ((Error)Errors[i]).line));
                        break;
                    case 8:
                        errorInfos.Add(String.Format("Identifier redefined: {0} at line {1}", ((Error)Errors[i]).value, ((Error)Errors[i]).line));
                        break;
                    case 9:   
                        errorInfos.Add(String.Format("Syntax error: {0} at line {1}", ((Error)Errors[i]).value, ((Error)Errors[i]).line));
                        break;
                    case 10:   //跳转语句错误，比如非函数体内，if中出现return
                        errorInfos.Add(String.Format("Jump error: {0} at line {1}", ((Error)Errors[i]).value, ((Error)Errors[i]).line));
                        break;
                    case 11:   //数组长度错误
                        errorInfos.Add(String.Format("Array length error: {0} at line {1}", ((Error)Errors[i]).value, ((Error)Errors[i]).line));
                        break;
                    case 12:  //函数重定义
                        errorInfos.Add(String.Format("Function redefined: {0} at line {1}", ((Error)Errors[i]).value, ((Error)Errors[i]).line));
                        break;
                    case 13:   //数组越界
                        errorInfos.Add(String.Format("Array index over bound: {0} at line {1}", ((Error)Errors[i]).value, ((Error)Errors[i]).line));
                        break;
                    case 14:   //数组未定义
                        errorInfos.Add(String.Format("Array not defined: {0} at line {1}", ((Error)Errors[i]).value, ((Error)Errors[i]).line));
                        break;
                    case 15:   //比较符两侧类型不同
                        errorInfos.Add(String.Format("Different types of comparators on both sides: {0} at line {1}", ((Error)Errors[i]).value, ((Error)Errors[i]).line));
                        break;
                    case 16:   //取反运算仅适用于整数
                        errorInfos.Add(String.Format("Operator ! only suit for numbers: {0} at line {1}", ((Error)Errors[i]).value, ((Error)Errors[i]).line));
                        break;
                    case 17:   //标识符未初始化
                        errorInfos.Add(String.Format("Identifier not initialized: {0} at line {1}", ((Error)Errors[i]).value, ((Error)Errors[i]).line));
                        break;
                    case 18:   //类型不匹配
                        errorInfos.Add(String.Format("Type dismatch: {0} at line {1}", ((Error)Errors[i]).value, ((Error)Errors[i]).line));
                        break;
                    case 19:   //除0错误
                        errorInfos.Add(String.Format("Attempted to divide by zero: {0} at line {1}", ((Error)Errors[i]).value, ((Error)Errors[i]).line));
                        break;
                    case 20:   //除0错误
                        errorInfos.Add(String.Format("Number of elements is greater than array length: {0} at line {1}", ((Error)Errors[i]).value, ((Error)Errors[i]).line));
                        break;
                    case 21:   //条件判断错误
                        errorInfos.Add(String.Format("Condition statement error: {0} at line {1}", ((Error)Errors[i]).value, ((Error)Errors[i]).line));
                        break;
                    case 22:   //输入错误
                        errorInfos.Add(String.Format("Scanf error: {0} at line {1}", ((Error)Errors[i]).value, ((Error)Errors[i]).line));
                        break;
                }
            }
            return errorInfos;
        }
        public string[] SaveToken()                                            //true代表将token保存至文件夹，false代表输出至屏幕
        {
            //string path = @"..\..\..\Token.txt";                               //Token文件路径
            string[] infos = new string[Coding.Count + 1];           //将token信息保存至string数组
            infos[0] = "顺序\t行数\t类别    \t\t数值";                           //写入说明
            string type = "";                                                           //保存token类型
            for (int i=0; i < Coding.Count; i++)
            {
                int tag = ((Word)Coding[i]).tag;
                switch (tag)
                {
                    case 0:
                        type = "结束符";
                        break;
                    case 1:
                        type = "保留字  ";
                        break;
                    case 2:
                        type = "界限符  ";
                        break;
                    case 3:
                        type = "运算符  ";
                        break;
                    case 4:
                        type = "常数    ";
                        break;
                    case 5:
                        type = "标识符  ";
                        break;
                }
                infos[i + 1] = (i + 1).ToString()+"\t"+ ((Word)Coding[i]).line.ToString() + "\t" + type +"\t\t" + ((Word)Coding[i]).value;
            }
            //if (output && Coding.Count != 0)
            //    System.IO.File.WriteAllLines(path, infos, Encoding.UTF8);
            //else if(!output && Coding.Count != 0)
            //{
            //    foreach (string info in infos)
            //        Console.WriteLine(info);
            //}
            return infos;
        }
    }
}