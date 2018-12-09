using System.Collections.Generic;
using Interpreter.Inner.CodeGenerator;

namespace Interpreter
{
    namespace Inner
    {
        class Parser
        {
            public Parser(Machine _m, Lexer _l)
            {
                m = _m; l = _l;
            }

            public void parse()
            {
                int pos = 1; int size;
                accept();
                do
                {
                    if (lookAhead.TokenType == TokenType.TOKEN_EOF) break;

                    int line = l.currentLine;
                    Statement body = functionDef();
                    Statement func = new stat_function(m, line, body, m.parser_getLocalCount(), m.parser_getCurrentFunction() == "main" ? 1 : 0);

                    size = func.getInstruction(m.code, pos);
                    m.parser_register_function(l.currentLine, m.parser_getCurrentFunction());
                    m.SetGlobal(m.parser_getCurrentFunction(), new Varible(pos));

                    pos += size;

                } while (!l.eof());
                m.CodeLength = pos;
            }

            private void paramList(bool locals)
            {
                char eod = ')';
                if (locals)
                    eod = ';';
                while (lookAhead.TokenType != eod)
                {
                    Token id = match(TokenType.TOKEN_ID);
                    m.parser_addLocal(id.strToken);
                    if (lookAhead.TokenType != eod)
                        match(',');
                }
            }

            private List<Expression> args()
            {
                List<Expression> expList = new List<Expression>();
                while (lookAhead.TokenType != ')')
                {
                    expList.Add(expr());
                    if (lookAhead.TokenType != ')')
                        match(',');
                }
                return expList;
            }

            private Expression factor()
            {
                Expression retVal;
                int local;
                Token id;
                switch (lookAhead.TokenType)
                {
                    case '(':
                        accept(); retVal = expr(); match(')');
                        break;
                    case TokenType.TOKEN_ID:
                        id = match(TokenType.TOKEN_ID);
                        local = m.GetLocalIDByName(id.strToken);
                        if (lookAhead.TokenType == '(') //function call
                        {
                            int line = l.currentLine;
                            accept(); List<Expression> argss = args(); match(')');
                            if (local != -1)
                                retVal = new exp_functioncall(m, line, argss, local,true);
                            else
                                retVal = new exp_functioncall(m, line, argss, m.parser_checkStringMap(id.strToken, true),false);
                        }
                        else
                        {
                            if (local != -1)
                                retVal = new exp_local_var_right(m, l.currentLine, local);
                            else
                                retVal = new exp_var_right(m, l.currentLine, m.parser_checkStringMap(id.strToken, true));
                        }
                        break;
                    case TokenType.TOKEN_IN:
                        accept();match('(');
                        retVal = new exp_in(m, l.currentLine, expr());
                        match(')');
                        break;
                    case TokenType.TOKEN_NUMBER:
                        id = match(TokenType.TOKEN_NUMBER);
                        return new exp_number(m, l.currentLine, id.numToken);
                    case '!':
                        accept();
                        return new exp_not(m, l.currentLine, factor());
                    case '-':
                        accept();
                        return new exp_arith(m, l.currentLine, new exp_number(m, l.currentLine, 0.0), factor(), OpCode.ARITH_SUB);
                    case TokenType.TOKEN_STRING:
                        id = match(TokenType.TOKEN_STRING);
                        return new exp_string(m, l.currentLine, m.parser_checkStringMap(id.strToken, true));
                    default:
                        throw new CompileException(l.currentLine, "Unexpected Token " + TokenType.getTokenTypeString(lookAhead.TokenType));
                }
                return retVal;
            }

            private Expression expr1(int priority)
            {
                Expression retVal;
                Expression right;
                int compareMode;
                int line = l.currentLine;
                if (priority >= 0)
                    retVal = expr1(priority - 1);
                else
                    return factor();
                while (inPriority(lookAhead.TokenType, priority))
                {
                    switch (lookAhead.TokenType)
                    {
                        case '+':
                            accept();
                            right = expr1(priority - 1);
                            retVal = new exp_arith(m, line, retVal, right, OpCode.ARITH_ADD);
                            break;
                        case '-':
                            accept();
                            right = expr1(priority - 1);
                            retVal = new exp_arith(m, line, retVal, right, OpCode.ARITH_SUB);
                            break;
                        case '*':
                            accept();
                            right = expr1(priority - 1);
                            retVal = new exp_arith(m, line, retVal, right, OpCode.ARITH_MUL);
                            break;
                        case '/':
                            accept();
                            right = expr1(priority - 1);
                            retVal = new exp_arith(m, line, retVal, right, OpCode.ARITH_DIV);
                            break;
                        case '<':
                        case '>':
                        case TokenType.TOKEN_GEQ:
                        case TokenType.TOKEN_EQUAL:
                        case TokenType.TOKEN_LEQ:
                        case TokenType.TOKEN_NEQU:
                            compareMode = getCompareMode();
                            accept();
                            right = expr1(priority - 1);
                            retVal = new exp_compare(m, line, retVal, right, compareMode);
                            break;
                        case TokenType.TOKEN_LOGICAL_AND:
                            accept();
                            right = expr1(priority - 1);
                            retVal = new exp_and(m, line, retVal, right);
                            break;
                        case TokenType.TOKEN_LOGICAL_OR:
                            accept();
                            right = expr1(priority - 1);
                            retVal = new exp_or(m, line, retVal, right);
                            break;
                        default:
                            return retVal;
                    }

                }
                return retVal;
            }

            private Expression expr()
            {
                return expr1(4);
            }

            private Statement stmts()
            {
                List<Statement> statList = new List<Statement>();
                int line = l.currentLine;
                while (lookAhead.TokenType != '}')
                {
                    statList.Add(stmt());
                }
                return new stat_seq(m, line, statList);
            }

            private Statement stmt()
            {
                Statement retVal;
                Statement do_if_true;
                Statement do_if_false;
                Expression cmp;
                int local;
                Token id;
                int line = l.currentLine;
                switch (lookAhead.TokenType)
                {
                    case '{':
                        match('{');
                        retVal = stmts();
                        match('}');
                        break;
                    case TokenType.TOKEN_IF:
                        accept(); match('(');
                        cmp = expr();
                        match(')');
                        do_if_true = stmt();
                        if (lookAhead.TokenType == TokenType.TOKEN_ELSE)
                        {
                            accept();
                            do_if_false = stmt();
                        }
                        else
                            do_if_false = new stat_nop();
                        retVal = new stat_if(m, line, cmp, do_if_true, do_if_false);
                        break;
                    case TokenType.TOKEN_WHILE:
                        accept(); match('(');
                        cmp = expr();
                        match(')');
                        retVal = new stat_while(m, line, cmp, stmt());
                        break;
                    case TokenType.TOKEN_ID:
                        id = match(TokenType.TOKEN_ID);
                        local = m.GetLocalIDByName(id.strToken);
                        if (lookAhead.TokenType == '(') // function call
                        {
                            accept(); List<Expression> argss = args(); match(')');
                            if (local != -1)
                                retVal = new stat_exp(m, line, new exp_functioncall(m, line, argss, local,true));
                            else
                                retVal = new stat_exp(m, line, new exp_functioncall(m, line, argss, m.parser_checkStringMap(id.strToken, true),false));
                        }
                        else
                        {
                            match('=');
                            if (local != -1)
                                retVal = new stat_assignment_local(m, line, local, expr());
                            else
                                retVal = new stat_assignment(m, line, m.parser_checkStringMap(id.strToken, true), expr());
                        }
                        match(';');
                        break;
                    case TokenType.TOKEN_RETURN:
                        accept();
                        retVal = new stat_return(m, line, expr());
                        match(';');
                        break;
                    case TokenType.TOKEN_OUT:
                        accept();
                        match('(');
                        retVal = new stat_out(m, line, expr(),match(',').TokenType!=0?expr():null);
                        match(')');
                        match(';');
                        break;
                    case TokenType.TOKEN_LOCAL:
                        accept();
                        paramList(true);
                        match(';');
                        retVal = new stat_nop();
                        break;
                    case TokenType.TOKEN_BREAK:
                        accept();
                        retVal = new stat_break(m, line);
                        match(';');
                        break;
                    case TokenType.TOKEN_CONTINUE:
                        accept();
                        retVal = new stat_continue(m, line);
                        match(';');
                        break;
                    default:
                        retVal = new stat_exp(m, line, expr());
                        match(';');
                        break;
                }
                return retVal;
            }

            private Statement functionDef()
            {
                match(TokenType.TOKEN_FUNCTION);
                Token id = match(TokenType.TOKEN_ID);
                m.parser_setCurrentFunction(id.strToken);
                match('('); paramList(false); match(')');
                return stmt();
            }

            private void accept()
            {
                lookAhead = l.next();
            }

            private Token match(int Token_type)
            {
                Token retVal = lookAhead;
                if (lookAhead.TokenType == Token_type)
                    accept();
                else
                    throw new CompileException(l.currentLine, "Except " + TokenType.getTokenTypeString(Token_type)
                        + " near " + TokenType.getTokenTypeString(lookAhead.TokenType));
                return retVal;
            }

            private int getCompareMode()
            {
                switch (lookAhead.TokenType)
                {
                    case '<':
                        return CompareMode.COMPARE_LESS;
                    case '>':
                        return CompareMode.COMPARE_GREATER;
                    case TokenType.TOKEN_GEQ:
                        return CompareMode.COMPARE_GREATER | CompareMode.COMPARE_EQUAL;
                    case TokenType.TOKEN_EQUAL:
                        return CompareMode.COMPARE_EQUAL;
                    case TokenType.TOKEN_LEQ:
                        return CompareMode.COMPARE_LESS | CompareMode.COMPARE_EQUAL;
                    case TokenType.TOKEN_NEQU:
                        return CompareMode.COMPARE_GREATER | CompareMode.COMPARE_LESS;
                };
                return 0;
            }

            private bool inPriority(int TokenType, int priority)
            {
                int i = 0;
                while (priorities[priority, i] != -1)
                {
                    if (priorities[priority, i] == TokenType) return true;
                    ++i;
                }
                return false;
            }

            private static int[,] priorities = 
            {
	            {'*','/',-1,-1,-1,-1,-1},
	            {'+','-',-1,-1,-1,-1,-1},
	            {'>','<',TokenType.TOKEN_EQUAL,TokenType.TOKEN_GEQ,TokenType.TOKEN_LEQ,TokenType.TOKEN_NEQU,-1},
	            {TokenType.TOKEN_LOGICAL_AND,-1,-1,-1,-1,-1,-1 },
	            {TokenType.TOKEN_LOGICAL_OR,-1,-1,-1,-1,-1,-1},
            };

            private Machine m;
            private Lexer l;
            private Token lookAhead;
        }
    }
}
