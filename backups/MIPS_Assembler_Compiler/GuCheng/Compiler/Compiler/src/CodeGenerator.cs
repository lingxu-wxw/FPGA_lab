using System.Collections.Generic;

namespace Interpreter
{
    namespace Inner
    {
        namespace CodeGenerator
        {
            interface Statement
            {
                int getInstruction(Instruction[] buffer, int startPos);
            }
            interface Expression
            {
                int getInstruction(Instruction[] buffer, int startPos);
            }

            #region Expressions
            class exp_compare
                : Expression
            {
                private int _line;
                private Expression _left;
                private Expression _right;
                private int _compareMode;
                private Machine _m;

                public exp_compare(Machine m, int line, Expression left, Expression right, int compareMode)
                {
                    _m = m;
                    _line = line;
                    _left = left;
                    _right = right;
                    _compareMode = compareMode;
                }

                public int getInstruction(Instruction[] buffer, int startPos)
                {
                    Instruction ins; int pos = 0;
                    pos += _left.getInstruction(buffer, startPos + pos);
                    pos += _right.getInstruction(buffer, startPos + pos);
                    ins.opCode = OpCode.CMP;
                    ins.pointer = _compareMode;
                    buffer[startPos + pos] = ins;
                    pos++;
                    _m.parser_setLineCode(_line, startPos);
                    return pos;
                }
            }

            class exp_not
                : Expression
            {
                private Machine _m;
                private int _line;
                private Expression _exp;

                public exp_not(Machine m, int line, Expression exp)
                {
                    _m = m;
                    _line = line;
                    _exp = exp;
                }

                public int getInstruction(Instruction[] buffer, int startPos)
                {
                    Instruction ins; int pos = 0;
                    pos += _exp.getInstruction(buffer, startPos + pos);
                    ins.opCode = OpCode.NOT;
                    ins.pointer = 0;
                    buffer[startPos + pos] = ins;
                    pos++;
                    _m.parser_setLineCode(_line, startPos);
                    return pos;
                }
            }

            class exp_and
                : Expression
            {
                private int _line;
                private Expression _left;
                private Expression _right;
                private Machine _m;

                public exp_and(Machine m, int line, Expression left, Expression right)
                {
                    _m = m;
                    _line = line;
                    _left = left;
                    _right = right;
                }

                public int getInstruction(Instruction[] buffer, int startPos)
                {
                    Instruction ins; int pos = 0;
                    pos += _left.getInstruction(buffer, startPos + pos);

                    ins.opCode = OpCode.JUMP;
                    ins.pointer = 3;
                    buffer[startPos + pos] = ins;
                    pos++;

                    ins.opCode = OpCode.PUSHNUM;
                    buffer[startPos + pos] = ins;
                    pos++;

                    ins.opCode = OpCode.JUMPA;
                    ins.pointer = _right.getInstruction(buffer, startPos + pos + 1) + 1;
                    buffer[startPos + pos] = ins;
                    pos++;

                    pos += (ins.pointer - 1);

                    _m.parser_setLineCode(_line, startPos);
                    return pos;
                }
            }

            class exp_or
                : Expression
            {
                private int _line;
                private Expression _left;
                private Expression _right;
                private Machine _m;

                public exp_or(Machine m, int line, Expression left, Expression right)
                {
                    _m = m;
                    _line = line;
                    _left = left;
                    _right = right;
                }

                public int getInstruction(Instruction[] buffer, int startPos)
                {
                    Expression exp = new exp_not(_m, _line,
                        new exp_and(_m, _line, new exp_not(_m, _line, _left), new exp_not(_m, _line, _right))
                    );
                    int pos = exp.getInstruction(buffer, startPos);
                    _m.parser_setLineCode(_line, startPos);
                    return pos;
                }
            }

            class exp_number
                : Expression
            {
                private Machine _m;
                private int _line;
                private double _num;

                public exp_number(Machine m, int line, double num)
                {
                    _m = m;
                    _line = line;
                    _num = num;
                }

                public int getInstruction(Instruction[] buffer, int startPos)
                {
                    Instruction ins; int pos = 0;
                    ins.opCode = OpCode.PUSHNUM;
                    ins.pointer = (int)_num;
                    buffer[startPos + pos] = ins;
                    pos++;
                    _m.parser_setLineCode(_line, startPos);
                    return pos;
                }
            }

            class exp_string
                : Expression
            {
                private Machine _m;
                private int _line;
                private int _str;

                public exp_string(Machine m, int line, int str)
                {
                    _m = m;
                    _line = line;
                    _str = str;
                }

                public int getInstruction(Instruction[] buffer, int startPos)
                {
                    Instruction ins; int pos = 0;
                    ins.opCode = OpCode.PUSHSTRING;
                    ins.pointer = _str;
                    buffer[startPos + pos] = ins;
                    pos++;
                    _m.parser_setLineCode(_line, startPos);
                    return pos;
                }
            }

            class exp_var_right
                : Expression
            {
                private Machine _m;
                private int _line;
                private int _var;

                public exp_var_right(Machine m, int line, int var)
                {
                    _m = m;
                    _line = line;
                    _var = var;
                }

                public int getInstruction(Instruction[] buffer, int startPos)
                {
                    Instruction ins; int pos = 0;
                    ins.opCode = OpCode.GETGLOBAL;
                    ins.pointer = _var;
                    buffer[startPos + pos] = ins;
                    pos++;
                    _m.parser_setLineCode(_line, startPos);
                    return pos;
                }
            }

            class exp_local_var_right
                : Expression
            {
                private Machine _m;
                private int _line;
                private int _var;

                public exp_local_var_right(Machine m, int line, int var)
                {
                    _m = m;
                    _line = line;
                    _var = var;
                }

                public int getInstruction(Instruction[] buffer, int startPos)
                {
                    Instruction ins; int pos = 0;
                    ins.opCode = OpCode.GETLOCAL;
                    ins.pointer = _var;
                    buffer[startPos + pos] = ins;
                    pos++;
                    _m.parser_setLineCode(_line, startPos);
                    return pos;
                }
            }

            class exp_arith
                : Expression
            {
                private Machine _m;
                private int _line;
                private Expression _left;
                private Expression _right;
                private int _arithType;

                public exp_arith(Machine m, int line, Expression left, Expression right, int arithType)
                {
                    _m = m;
                    _line = line;
                    _left = left;
                    _right = right;
                    _arithType = arithType;
                }

                public int getInstruction(Instruction[] buffer, int startPos)
                {
                    Instruction ins; int pos = 0;
                    pos += _left.getInstruction(buffer, startPos + pos);
                    pos += _right.getInstruction(buffer, startPos + pos);

                    ins.opCode = OpCode.ARITH;
                    ins.pointer = _arithType;
                    buffer[startPos + pos] = ins;
                    pos++;

                    _m.parser_setLineCode(_line, startPos);
                    return pos;
                }
            }

            class exp_functioncall
                : Expression
            {
                private Machine _m;
                private int _line;
                private List<Expression> _exprList;
                private int _funcName;
                private bool _local;

                public exp_functioncall(Machine m, int line, List<Expression> exprList, int funcName,bool local)
                {
                    _m = m;
                    _line = line;
                    _exprList = exprList;
                    _funcName = funcName;
                    _local = local;
                }

                public int getInstruction(Instruction[] buffer, int startPos)
                {
                    Instruction ins; int pos = 0;

                    for (int i = 0; i < _exprList.Count; ++i)
                    {
                        pos += _exprList[i].getInstruction(buffer, startPos + pos);
                    }


                    if (_local)
                        ins.opCode = OpCode.GETLOCAL;
                    else
                        ins.opCode = OpCode.GETGLOBAL;
                    ins.pointer = _funcName;
                    buffer[startPos + pos] = ins;
                    pos++;

                    ins.opCode = OpCode.CALL;
                    ins.pointer = _exprList.Count;
                    buffer[startPos + pos] = ins;
                    pos++;

                    _m.parser_setLineCode(_line, startPos);
                    return pos;
                }
            }

            class exp_in
                : Expression
            {
                private int _line;
                private Machine _m;
                private Expression _exp;

                public exp_in(Machine m, int line, Expression exp)
                {
                    _m = m;
                    _line = line;
                    _exp = exp;
                }

                public int getInstruction(Instruction[] buffer, int startPos)
                {
                    int pos = 0; Instruction ins;
                    pos += _exp.getInstruction(buffer, startPos + pos);

                    ins.pointer = 0;
                    ins.opCode = OpCode.IN;
                    buffer[startPos + pos] = ins;
                    pos++;
                    _m.parser_setLineCode(_line, startPos);
                    return pos;
                }
            }

            #endregion

            #region Statements

            class stat_nop
                : Statement
            {
                public int getInstruction(Instruction[] buffer, int startPos)
                {
                    return 0;
                }
            }

            class stat_function
                : Statement
            {
                private int _line;
                private Machine _m;
                private Statement _body;
                private int _numparam;
                private int _main;

                public stat_function(Machine m, int line, Statement body, int numparam,int main)
                {
                    _m = m;
                    _line = line;
                    _body = body;
                    _numparam = numparam;
                    _main = main;
                }

                public int getInstruction(Instruction[] buffer, int startPos)
                {
                    int pos = 0; Instruction ins;
                    ins.pointer = _numparam; 
                    ins.opCode = OpCode.ADJLOCAL;
                    buffer[startPos + pos] = ins;
                    pos++;

                    pos += _body.getInstruction(buffer, startPos + pos);

                    if (_main == 0)
                    {
                        ins.pointer = 0;
                        ins.opCode = OpCode.PUSHNUM;
                        buffer[startPos + pos] = ins;
                        pos++;

                        ins.pointer = 0;
                        ins.opCode = OpCode.RET;
                        buffer[startPos + pos] = ins;
                        pos++;
                    }
                    else
                    {
                        ins.pointer = 0;
                        ins.opCode = OpCode.JUMPA;
                        buffer[startPos + pos] = ins;
                        pos++;
                    }

                    _m.parser_setLineCode(_line, startPos);
                    return pos;
                }

            }

            class stat_exp
                : Statement
            {
                private int _line;
                private Machine _m;
                private Expression _exp;

                public stat_exp(Machine m, int line, Expression exp)
                {
                    _m = m;
                    _line = line;
                    _exp = exp;
                }

                public int getInstruction(Instruction[] buffer, int startPos)
                {
                    int pos = 0; Instruction ins;
                    pos += _exp.getInstruction(buffer, startPos + pos);

                    ins.pointer = 1; 
                    ins.opCode = OpCode.POP;
                    buffer[startPos + pos] = ins;
                    pos++;
                    _m.parser_setLineCode(_line, startPos);
                    return pos;
                }
            }

            class stat_seq
                : Statement
            {
                private int _line;
                private Machine _m;
                private List<Statement> _statList;

                public stat_seq(Machine m, int line, List<Statement> statList)
                {
                    _m = m;
                    _line = line;
                    _statList = statList;
                }

                public int getInstruction(Instruction[] buffer, int startPos)
                {
                    int pos = 0;
                    for (int i = 0; i < _statList.Count; ++i)
                    {
                        pos += _statList[i].getInstruction(buffer, startPos + pos);
                    }

                    _m.parser_setLineCode(_line, startPos);
                    return pos;
                }
            }

            class stat_return
                : Statement
            {
                private int _line;
                private Machine _m;
                private Expression _exp;

                public stat_return(Machine m, int line, Expression exp)
                {
                    _m = m;
                    _line = line;
                    _exp = exp;
                }

                public int getInstruction(Instruction[] buffer, int startPos)
                {
                    int pos = 0; Instruction ins;
                    pos += _exp.getInstruction(buffer, startPos + pos);

                    ins.pointer = 0; 
                    ins.opCode = OpCode.RET;
                    buffer[startPos + pos] = ins;
                    pos++;
                    _m.parser_setLineCode(_line, startPos);
                    return pos;
                }
            }

            class stat_out
                : Statement
            {
                private int _line;
                private Machine _m;
                private Expression _port;
                private Expression _data;

                public stat_out(Machine m, int line, Expression port, Expression data)
                {
                    _m = m;
                    _line = line;
                    _port = port;
                    _data = data;
                }

                public int getInstruction(Instruction[] buffer, int startPos)
                {
                    int pos = 0; Instruction ins;
                    pos += _port.getInstruction(buffer, startPos + pos);
                    pos += _data.getInstruction(buffer, startPos + pos);

                    ins.pointer = 0;
                    ins.opCode = OpCode.OUT;
                    buffer[startPos + pos] = ins;
                    pos++;
                    _m.parser_setLineCode(_line, startPos);
                    return pos;
                }
            }
                        
            class stat_assignment
                : Statement
            {
                private Machine _m;
                private int _line;
                private int _var;
                private Expression _exp;

                public stat_assignment(Machine m, int line, int var, Expression exp)
                {
                    _m = m;
                    _line = line;
                    _var = var;
                    _exp = exp;
                }

                public int getInstruction(Instruction[] buffer, int startPos)
                {
                    Instruction ins; int pos = 0;
                    pos += _exp.getInstruction(buffer, startPos + pos);
                    ins.opCode = OpCode.SETGLOBAL;
                    ins.pointer = _var;
                    
                    buffer[startPos + pos] = ins;
                    pos++;
                    _m.parser_setLineCode(_line, startPos);
                    return pos;
                }
            }

            class stat_assignment_local
                : Statement
            {
                private Machine _m;
                private int _line;
                private int _var;
                private Expression _exp;

                public stat_assignment_local(Machine m, int line, int var, Expression exp)
                {
                    _m = m;
                    _line = line;
                    _var = var;
                    _exp = exp;
                }

                public int getInstruction(Instruction[] buffer, int startPos)
                {
                    Instruction ins; int pos = 0;
                    pos += _exp.getInstruction(buffer, startPos + pos);
                    ins.opCode = OpCode.SETLOCAL;
                    ins.pointer = _var;
                    
                    buffer[startPos + pos] = ins;
                    pos++;
                    _m.parser_setLineCode(_line, startPos);
                    return pos;
                }
            }

            class stat_if
                : Statement
            {
                private Machine _m;
                private int _line;
                private Expression _exp;
                private Statement _doIfTrue;
                private Statement _doIfFalse;

                public stat_if(Machine m, int line, Expression exp, Statement doIfTrue, Statement doIfFalse)
                {
                    _m = m;
                    _line = line;
                    _exp = exp;
                    _doIfFalse = doIfFalse;
                    _doIfTrue = doIfTrue;
                }

                public int getInstruction(Instruction[] buffer, int startPos)
                {
                    Instruction ins; int pos = 0;
                    pos += _exp.getInstruction(buffer, startPos + pos);

                    int action_if_true_pos = pos + 1 + _doIfFalse.getInstruction(buffer, startPos + (pos + 1));
                    ins.opCode = OpCode.JUMP;
                    
                    ins.pointer = action_if_true_pos - pos + 1;
                    buffer[startPos + pos] = ins;
                    pos++;

                    pos = action_if_true_pos;
                    int end_pos = pos + 1 + _doIfTrue.getInstruction(buffer, startPos + (pos + 1));
                    ins.opCode = OpCode.JUMPA;
                    ins.pointer = end_pos - pos;
                    buffer[startPos + pos] = ins;
                    pos++;

                    pos = end_pos;

                    _m.parser_setLineCode(_line, startPos);
                    return pos;
                }
            }

            class stat_break
                : Statement
            {
                private Machine _m;
                private int _line;

                public stat_break(Machine m, int line)
                {
                    _m = m;
                    _line = line;
                }

                public int getInstruction(Instruction[] buffer, int startPos)
                {
                    int pos = 0; Instruction ins;

                    ins.pointer = 0; 
                    ins.opCode = OpCode.PSEUDO_BREAK;
                    buffer[startPos + pos] = ins;
                    pos++;

                    _m.parser_setLineCode(_line, startPos);
                    return pos;
                }
            }

            class stat_continue
                : Statement
            {
                private Machine _m;
                private int _line;

                public stat_continue(Machine m, int line)
                {
                    _m = m;
                    _line = line;
                }

                public int getInstruction(Instruction[] buffer, int startPos)
                {
                    int pos = 0; Instruction ins;

                    ins.pointer = 0; 
                    ins.opCode = OpCode.PSEUDO_CONTINUE;
                    buffer[startPos + pos] = ins;
                    pos++;

                    _m.parser_setLineCode(_line, startPos);
                    return pos;
                }
            }

            class stat_while
                : Statement
            {
                private Machine _m;
                private int _line;
                private Expression _exp;
                private Statement _body;

                public stat_while(Machine m, int line, Expression exp, Statement body)
                {
                    _m = m;
                    _line = line;
                    _exp = new exp_not(m, line, exp);
                    _body = body;
                }

                public int getInstruction(Instruction[] buffer, int startPos)
                {
                    int pos = 0; Instruction ins;
                    ins.pointer = 0; 

                    pos += _exp.getInstruction(buffer, startPos + pos);
                    int body_length = _body.getInstruction(buffer, startPos + (pos + 1));
                    //jump to end if false
                    ins.opCode = OpCode.JUMP;
                    ins.pointer = body_length + 2;
                    buffer[startPos + pos] = ins;
                    pos++;
                    pos += body_length;

                    ins.opCode = OpCode.JUMPA;
                    ins.pointer = -pos;
                    buffer[startPos + pos] = ins;
                    pos++;

                    //Scan any break and continue instructions
                    for (int i = 0; i < pos; ++i)
                    {
                        if (buffer[startPos + i].opCode == OpCode.PSEUDO_BREAK)
                        {
                            buffer[startPos + i].opCode = OpCode.JUMPA;
                            buffer[startPos + i].pointer = pos - i;
                        }
                        else if (buffer[startPos + i].opCode == OpCode.PSEUDO_CONTINUE)
                        {
                            buffer[startPos + i].opCode = OpCode.JUMPA;
                            buffer[startPos + i].pointer = -i;
                        }
                    }

                    _m.parser_setLineCode(_line, startPos);
                    return pos;
                }
            }

            #endregion
        }
    }
}