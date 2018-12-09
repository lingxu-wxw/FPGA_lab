using System;
using System.Collections.Generic;

namespace Interpreter
{
    static class TokenType
    {
        public static string getTokenTypeString(int tokenType)
        {
            switch (tokenType)
            {
                case TOKEN_IF:
                    return "\"if\"";
                case TOKEN_ELSE:
                    return "\"else\"";
                case TOKEN_WHILE:
                    return "\"while\"";
                case TOKEN_FUNCTION:
                    return "\"function\"";
                case TOKEN_NUMBER:
                    return "Number";
                case TOKEN_ID:
                    return "Identifier";
                case TOKEN_EQUAL:
                    return "\"==\"";
                case TOKEN_GEQ:
                    return "\">=\"";
                case TOKEN_LEQ:
                    return "\"<=\"";
                case TOKEN_NEQU:
                    return "\"!=\"";
                case TOKEN_RETURN:
                    return "\"return\"";
                case TOKEN_LOCAL:
                    return "\"local\"";
                case TOKEN_LOGICAL_AND:
                    return "\"&&\"";
                case TOKEN_LOGICAL_OR:
                    return "\"||\"";
                case TOKEN_BREAK:
                    return "\"break\"";
                case TOKEN_CONTINUE:
                    return "\"continue\"";
                case TOKEN_EOF:
                    return "<EOF>";
                default:
                    return '"' + Convert.ToChar(tokenType).ToString() + '"';
            }
        }
        public const int TOKEN_IF = 0x101;
        public const int TOKEN_ELSE = 0x102;
        public const int TOKEN_WHILE = 0x103;
        public const int TOKEN_FUNCTION = 0x104;
        public const int TOKEN_NUMBER = 0x105;
        public const int TOKEN_ID = 0x106;
        public const int TOKEN_EQUAL = 0x107;
        public const int TOKEN_GEQ = 0x108;
        public const int TOKEN_LEQ = 0x109;
        public const int TOKEN_NEQU = 0x110;
        public const int TOKEN_RETURN = 0x111;
        public const int TOKEN_LOCAL = 0x112;
        public const int TOKEN_LOGICAL_AND = 0x113;
        public const int TOKEN_LOGICAL_OR = 0x114;
        public const int TOKEN_BREAK = 0x115;
        public const int TOKEN_CONTINUE = 0x116;
        public const int TOKEN_EOF = 0x117;
        public const int TOKEN_STRING = 0x118;
        public const int TOKEN_OUT = 0x119;
        public const int TOKEN_IN = 0x120;
    }
    struct Token
    {
        public int TokenType;
        public string strToken;
        public double numToken;
    }

    public interface IFunction
    {
        Varible invoke(Machine m,int numParams);
    }

    static class OpCode
    {
        public const short OUT = 0x0;
        public const short IN = 0x10;
        public const short PUSHNUM = 0x1;
        public const short POP = 0x2;
        public const short GETLOCAL = 0x3;
        public const short SETLOCAL = 0x4;
        public const short GETGLOBAL = 0x5;
        public const short SETGLOBAL = 0x6;
        public const short CALL = 0x7;
        public const short RET = 0x8;
        public const short CMP = 0x9;
        public const short JUMP = 0xA;
        public const short JUMPA = 0xB;
        public const short ADJLOCAL = 0xC;
        public const short NOT = 0xD;
        public const short ARITH = 0xE;
        public const short PUSHSTRING = 0xF;
        public const short PSEUDO_BREAK = 0x61;
        public const short PSEUDO_CONTINUE = 0x62;
        public const short ARITH_ADD = 1;
        public const short ARITH_SUB = 2;
        public const short ARITH_MUL = 3;
        public const short ARITH_DIV = 4;
        public const short COMPARE_LESS = 1;
        public const short COMPARE_EQUAL = 2;
        public const short COMPARE_GREATER = 4;
    }
    static class VarType
    {
        public const short TYPE_NUMBER = 1;
        public const short TYPE_FUNCTION = 2;
        public const short TYPE_C_FUNCTION = 3;
        public const short TYPE_STRING = 4;
        private static short curType = 4;
        public static short newType()
        {
            curType++;
            return curType;
        }
        public static string getTypeString(int varType)
        {
            switch (varType)
            {
                case TYPE_NUMBER:
                    return "Number";
                case TYPE_C_FUNCTION:
                    return "Native Function";
                case TYPE_FUNCTION:
                    return "Function";
                case TYPE_STRING:
                    return "String";
                default:
                    return "<Unknown Type>";
            }
        }
    }
    static class CompareMode
    {
        public const int COMPARE_LESS = 1;
        public const int COMPARE_GREATER = 2;
        public const int COMPARE_EQUAL = 4;
    }

    public struct Varible
    {
        public int pointer;
        public Varible(int number)
        {
            pointer = number;
        }
    }
    public struct Instruction
    {
        public short opCode;
        public int pointer;
    }
    public struct StackInfo
    {
        public int line;
        public string funcName;
        public List<Varible> parmList;
    }
    class RuntimeException
    : System.Exception
    {
        private string _description;
        public string Description
        {
            get
            {
                return _description;
            }
        }
        public RuntimeException(string description)
        {
            _description = description;
        }
    }
    class CompileException
        : System.Exception
    {
        private int _line;
        private string _description;
        public int Line
        {
            get
            {
                return _line;
            }
        }
        public string Description
        {
            get
            {
                return _description;
            }
        }
        public CompileException(int line, string description)
        {
            _description = description;
            _line = line;
        }
    }
    class BreakpointException
        : System.Exception
    {
    }
}
