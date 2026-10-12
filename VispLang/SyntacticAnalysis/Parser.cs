using VispLang.CodeGeneration;
using VispLang.LexicalAnalysis;
using VispLang.SyntacticAnalysis.ASTElements.Definions;
using VispLang.SyntacticAnalysis.ASTElements.Expression;
using VispLang.SyntacticAnalysis.ASTElements.Statement;
using VispLang.SyntacticAnalysis.ASTElements.Type;

namespace VispLang.SyntacticAnalysis
{
    public class Parser
    {
        private List<Token> InputTokens = [];
        private int Position;
        

        public List<IDefinion> ParseFile(List<Token> inputTokens)
        {
            InputTokens = inputTokens;
            Position = 0;
            List<IDefinion> result = [];
            while (!Match(TokenType.EOF))
                result.Add(ParseDefinion());
            return result;
        }

        public IDefinion ParseDefinion()
        {
            if (Match(TokenType.ExternKW))
            {
                IType rettype = ParseType();
                Token name = Consume(TokenType.Identifier);
                List<IType> args = [];
                Consume(TokenType.LeftParentheses);
                if (Match(TokenType.RightParentheses)) 
                { 
                    Consume(TokenType.Semicolon); 
                    return new ExternFunctionDefinion(name.Value!, rettype, [], name.Line, name.Column); 
                }
                while (true)
                {
                    args.Add(ParseType());
                    if (Match(TokenType.Comma)) continue;
                    Consume(TokenType.RightParentheses);
                    Consume(TokenType.Semicolon);
                    break;
                }
                return new ExternFunctionDefinion(name.Value!, rettype, args, name.Line, name.Column);
            }
            if (Match(TokenType.FuncKW))
            {
                IType rettype = ParseType();
                Token name = Consume(TokenType.Identifier);
                List<(IType type, string name)> args = [];
                Consume(TokenType.LeftParentheses);
                if (Match(TokenType.RightParentheses)) return new FunctionDefinion(name.Value!, rettype, [], ParseStatementBlockOrStatement(), name.Line, name.Column);
                while (true)
                {
                    args.Add((ParseType(), Consume(TokenType.Identifier).Value!));
                    if (Match(TokenType.Comma)) continue;
                    Consume(TokenType.RightParentheses);
                    break;
                }
                return new FunctionDefinion(name.Value!, rettype, args, ParseStatementBlockOrStatement(), name.Line, name.Column);
            }
            if (Match(TokenType.LetKW))
            {
                IType type = ParseType();
                Token name = Consume(TokenType.Identifier);
                IExpression? value = null;
                if(Match(TokenType.Equals))
                    value = ParseExpression();
                Consume(TokenType.Semicolon);
                return new VariableDefinion(name.Value!, type, value, name.Line, name.Column);
            }
            throw new NotImplementedException();
        }

        public IStatement ParseStatementBlockOrStatement()
        {
            if (Match(TokenType.LeftBrace))
            {
                List<IStatement> statements = [];
                while (!Match(TokenType.RightBrace))
                    statements.Add(ParseStatement());
                return new StatementBlock(statements);
            }
            return ParseStatement();
        }
        public IStatement ParseStatement(bool eatSemicolon = true)
        {
            if (Match(TokenType.LetKW))
            {
                IType type = ParseType();
                Token name = Consume(TokenType.Identifier);
                IExpression? exp = null;
                if (Match(TokenType.Equals))
                    exp = ParseExpression();
                if(eatSemicolon) Consume(TokenType.Semicolon);
                return new VariableDefineStatement(type, name.Value!, exp);
            }
            if (Match(TokenType.IfKW))
            {
                Consume(TokenType.LeftParentheses);
                IExpression condition = ParseExpression();
                Consume(TokenType.RightParentheses);
                IStatement ifbody = ParseStatementBlockOrStatement();
                IStatement? elsebody = null;
                if (Match(TokenType.ElseKW))
                    elsebody = ParseStatementBlockOrStatement();
                return new IfElseStatement(condition, ifbody, elsebody);
            }
            if (Match(TokenType.WhileKW))
            {
                Consume(TokenType.LeftParentheses);
                IExpression condition = ParseExpression();
                Consume(TokenType.RightParentheses);
                IStatement body = ParseStatementBlockOrStatement();
                return new WhileStatement(condition, body);
            }
            if (Match(TokenType.ForKW))
            {
                Consume(TokenType.LeftParentheses);
                IStatement init = ParseStatement(false);
                Consume(TokenType.Semicolon);
                IExpression cond = ParseExpression();
                Consume(TokenType.Semicolon);
                IStatement iter = ParseStatement(false);
                Consume(TokenType.RightParentheses);
                IStatement body = ParseStatementBlockOrStatement();
                return new ForStatement(init, iter, body, cond);
            }
            if (Match(TokenType.BreakKW))
            {
                if (eatSemicolon) Consume(TokenType.Semicolon);
                return new BreakStatement();
            }
            if (Match(TokenType.ContinueKW))
            {
                if(eatSemicolon) Consume(TokenType.Semicolon);
                return new ContinueStatement();
            }
            if (Match(TokenType.ReturnKW))
            {
                IExpression? exp = null;
                if (Peek().Type != TokenType.Semicolon)
                    exp = ParseExpression();
                if(eatSemicolon) Consume(TokenType.Semicolon);
                return new ReturnStatement(exp);
            }
            if (Match(TokenType.Identifier))
            {
                Token t = Peek(-1);
                List<IIndexator> indexators = [];
                while (true)
                {
                    if (Match(TokenType.Star))
                    {
                        indexators.Add(new DereferenceIndexator()); 
                        continue;
                    }
                    if (Match(TokenType.LeftSquareBrace))
                    {
                        IExpression index = ParseExpression();
                        Consume(TokenType.RightSquareBrace);
                        indexators.Add(new ArrayIndexator(index));
                        continue;
                    }
                    break;
                }
                if (Match(TokenType.Equals))
                {
                    Token eq = Peek(-1);
                    IExpression exp = ParseExpression();
                    if (eatSemicolon) Consume(TokenType.Semicolon);
                    return new AssignValueStatement(t.Value!, exp, indexators, eq.Line, eq.Column);
                }
                if (Match(TokenType.PlusPlus))
                {
                    if (eatSemicolon) Consume(TokenType.Semicolon);
                    return new IncrementStatemnet(t.Value!, indexators);
                }
                if (Match(TokenType.MinusMinus))
                {
                    if (eatSemicolon) Consume(TokenType.Semicolon);
                    return new DecrementStatemnet(t.Value!, indexators);
                }
                if (Match(TokenType.LeftParentheses))
                {
                    List<IExpression> args = [];
                    if (Match(TokenType.RightParentheses)) 
                    {
                        if (eatSemicolon) Consume(TokenType.Semicolon);
                        return new FunctionCallStatement(t.Value!, [], indexators); 
                    }
                    while (true)
                    {
                        args.Add(ParseExpression());
                        if (Match(TokenType.Comma)) continue;
                        Consume(TokenType.RightParentheses);
                        if (eatSemicolon) Consume(TokenType.Semicolon);
                        break;
                    }
                    return new FunctionCallStatement(t.Value!, args, indexators);
                }
                throw new Exception();
            }
            
            throw new Exception();  
        }

        public IType ParseType()
        {
            Token t = Consume(TokenType.Identifier);
            IType type = new NameType(t.Value!);
            while (true)
            {
                if (Match(TokenType.Star))
                {
                    type = new PointerType(type);
                    continue;
                }
                if (Match(TokenType.LeftParentheses)) 
                {
                    List<IType> args = [];
                    if (Match(TokenType.RightParentheses)) 
                    { 
                        type = new FunctionPointerType(type, args);
                        continue; 
                    }
                    while (true)
                    {
                        args.Add(ParseType());
                        if(Match(TokenType.Comma)) continue;
                        Consume(TokenType.RightParentheses);
                        break;
                    }
                    type = new FunctionPointerType(type, args);
                    continue;
                }
                break;
            }
            return type;
        }

        public IExpression ParseExpression()
        {
            return BitAndLogical();
        }
        private IExpression BitAndLogical()
        {
            IExpression exp = Compare();
            while (true)
            {
                Token t = Peek(0);
                if (Match(TokenType.And))
                {
                    exp = new BinaryExpression(exp, Compare(), BinaryOperation.BitAnd, t.Line, t.Column);
                    continue;
                }
                if (Match(TokenType.AndAnd))
                {
                    exp = new BinaryExpression(exp, Compare(), BinaryOperation.LogicalAnd, t.Line, t.Column);
                    continue;
                }
                if (Match(TokenType.Or))
                {
                    exp = new BinaryExpression(exp, Compare(), BinaryOperation.BitOr, t.Line, t.Column);
                    continue;
                }
                if (Match(TokenType.OrOr))
                {
                    exp = new BinaryExpression(exp, Compare(), BinaryOperation.LogicalOr, t.Line, t.Column);
                    continue;
                }
                break;
            }
            return exp;
        }
        private IExpression Compare()
        {
            IExpression exp = Additive();
            while (true)
            {
                Token t = Peek(0);
                if (Match(TokenType.EqualsEquals))
                {
                    exp = new BinaryExpression(exp, Additive(), BinaryOperation.Equals, t.Line, t.Column);
                    continue;
                }
                if (Match(TokenType.NotEquals))
                {
                    exp = new BinaryExpression(exp, Additive(), BinaryOperation.NotEquals, t.Line, t.Column);
                    continue;
                }
                if (Match(TokenType.GreatThan))
                {
                    exp = new BinaryExpression(exp, Additive(), BinaryOperation.GreatThan, t.Line, t.Column);
                    continue;
                }
                if (Match(TokenType.GreatThanEquals))
                {
                    exp = new BinaryExpression(exp, Additive(), BinaryOperation.GreatThanOrEqual, t.Line, t.Column);
                    continue;
                }
                if (Match(TokenType.LessThan))
                {
                    exp = new BinaryExpression(exp, Additive(), BinaryOperation.LessThan, t.Line, t.Column);
                    continue;
                }
                if (Match(TokenType.LessThanEquals))
                {
                    exp = new BinaryExpression(exp, Additive(), BinaryOperation.LessThanOrEqual, t.Line, t.Column);
                    continue;
                }
                break;
            }
            return exp;
        }
        private IExpression Additive()
        {
            IExpression exp = Multiplicative();
            while (true)
            {
                Token t = Peek();
                if (Match(TokenType.Plus))  
                {
                    exp = new BinaryExpression(exp, Multiplicative(), BinaryOperation.Add, t.Line, t.Column);
                    continue;
                }
                if (Match(TokenType.Minus))
                {
                    exp = new BinaryExpression(exp, Multiplicative(), BinaryOperation.Sub, t.Line, t.Column);
                    continue;
                }
                break;
            }
            return exp;
        }
        private IExpression Multiplicative()
        {
            IExpression exp = Unary();
            while (true)
            {
                Token t = Peek();
                if (Match(TokenType.Star))
                {
                    exp = new BinaryExpression(exp, Unary(), BinaryOperation.Mul, t.Line, t.Column);
                    continue;
                }
                if (Match(TokenType.Slash))
                {
                    exp = new BinaryExpression(exp, Unary(), BinaryOperation.Div, t.Line, t.Column);
                    continue;
                }
                if (Match(TokenType.Percent))
                {
                    exp = new BinaryExpression(exp, Unary(), BinaryOperation.Rem, t.Line, t.Column);
                    continue;
                }
                break;
            }
            return exp;
        }
        private IExpression Unary() 
        {
            Token t = Peek();
            if (Match(TokenType.Plus)) 
                return Unary();
            if (Match(TokenType.Minus))
                return new UnaryExpression(Unary(), UnaryOperation.Neg, t.Line, t.Column);
            if (Match(TokenType.Star))
                return new DereferenceExpression(Unary(), t.Line, t.Column);
            if (Match(TokenType.SizeOfKW))
                return new SizeOfExpression(ParseType(), t.Line, t.Column);
            if (Match(TokenType.And))
                return new AdressOfExpression(PostUnary(), t.Line, t.Column);
            if (Match(TokenType.Tilta)) 
                return new UnaryExpression(Unary(), UnaryOperation.BitNot, t.Line, t.Column);
            if (Match(TokenType.ExclamationMark))
                return new UnaryExpression(Unary(), UnaryOperation.LogicNot, t.Line, t.Column);
            if (Match(TokenType.PlusPlus))
                return new UnaryExpression(Unary(), UnaryOperation.PrefixIncrement, t.Line, t.Column); 
            if (Match(TokenType.MinusMinus))
                return new UnaryExpression(Unary(), UnaryOperation.PrefixDecrement, t.Line, t.Column);
            return PostUnary();
        }
        private IExpression PostUnary() 
        {
            IExpression exp = Primary();
            while (true)
            {
                Token t = Peek();
                if (Match(TokenType.LeftParentheses))
                {
                    List<IExpression> args = [];
                    if (Match(TokenType.RightParentheses)) { exp = new FunctionCallExpression(exp, [], t.Line, t.Column); continue; }
                    while(true)
                    {
                        args.Add(ParseExpression());
                        if (Match(TokenType.Comma)) continue;
                        Consume(TokenType.RightParentheses);
                        break;
                    }
                    exp = new FunctionCallExpression(exp, args, t.Line, t.Column);
                    continue;
                }
                if (Match(TokenType.AsKW))
                {
                    IType type = ParseType();
                    exp = new CastExpression(exp, type, t.Line, t.Column);
                    continue;
                }
                if (Match(TokenType.LeftSquareBrace))
                {
                    IExpression index = ParseExpression();
                    Consume(TokenType.RightSquareBrace);
                    exp = new IndexExpression(exp, index, t.Line, t.Column);
                    continue;
                }
                break;
            }
            return exp;
        }
        private IExpression Primary() 
        {
            Token t = Peek();
            if (Match(TokenType.Number))
                return new NumberExpression(t.Value!, t.Line, t.Column);
            if (Match(TokenType.Identifier))
                return new VariableExpression(t.Value!, t.Line, t.Column);
            if (Match(TokenType.Character))
            {
                if (t.Value!.Length == 1)
                    return new NumberExpression(((int)t.Value![0]).ToString(), t.Line, t.Column);
                return t.Value! switch
                {
                    "\\0" => new NumberExpression("0", t.Line, t.Column),
                    "\\t" => new NumberExpression("9", t.Line, t.Column),
                    "\\n" => new NumberExpression("10", t.Line, t.Column),
                    "\\r" => new NumberExpression("13", t.Line, t.Column),
                    "\\\\" => new NumberExpression("92", t.Line, t.Column),
                    _ => throw new CompilationException("Нераспознанная escape-последовательность", t.Line, t.Column),
                };
            }
            if (Match(TokenType.String))
                return new StringExpression(t.Value!, t.Line, t.Column);

            
            if (Match(TokenType.LeftParentheses))
            {
                IExpression exp = ParseExpression();
                Consume(TokenType.RightParentheses);
                return exp;
            }
            throw new CompilationException("Неизвестное выражение.", t.Line, t.Column);
        }


        private bool Match(TokenType type)
        {
            if (Peek().Type != type) return false;
            Position++;
            return true;
        }
        private Token Consume(TokenType type) 
        {
            if (Match(type))
                return Peek(-1);
            throw new Exception("Waiting for somethink");

        }
        private Token Peek(int offset = 0)
        {
            int index = Position + offset;
            return index < InputTokens.Count ? InputTokens[index] : new Token(TokenType.EOF, null, -1, -1);
        }
    }
}
