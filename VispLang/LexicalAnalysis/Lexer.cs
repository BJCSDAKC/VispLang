using System.Text;
using VispLang.CodeGeneration;

namespace VispLang.LexicalAnalysis
{
    public class Lexer
    {
        private string InputText;
        private int Position;
        private List<Token> OutputTokens;

        private int Line, Column, ActualLine, ActualColumn;

        private readonly Dictionary<string, TokenType> OperatorsAndBracets = new() 
        { 
            { "+", TokenType.Plus }, 
            { "++", TokenType.PlusPlus }, 
            { "-", TokenType.Minus }, 
            { "--", TokenType.MinusMinus }, 
            { "*", TokenType.Star }, 
            { "/", TokenType.Slash },
            { "%", TokenType.Percent },
            { "&", TokenType.And },
            { "&&", TokenType.AndAnd },
            { "^", TokenType.Xor },
            { "|", TokenType.Or },
            { "||", TokenType.OrOr },
            { "(", TokenType.LeftParentheses },
            { ")", TokenType.RightParentheses },
            { "=", TokenType.Equals },
            { "==", TokenType.EqualsEquals },
            { "!=", TokenType.NotEquals },
            { ">", TokenType.GreatThan },
            { ">=", TokenType.GreatThanEquals },
            { "<", TokenType.LessThan },
            { "<=", TokenType.LessThanEquals },
            { "{", TokenType.LeftBrace },
            { "}", TokenType.RightBrace },
            { "[", TokenType.LeftSquareBrace },
            { "]", TokenType.RightSquareBrace },
            { ";", TokenType.Semicolon },
            { ",", TokenType.Comma },
            { ".", TokenType.Dot },
        };

        private readonly Dictionary<string, TokenType> KeyWords = new()
        {
            { "let", TokenType.LetKW },
            { "func", TokenType.FuncKW },
            { "if", TokenType.IfKW },
            { "else", TokenType.ElseKW },
            { "sizeof", TokenType.SizeOfKW },
            { "while", TokenType.WhileKW },
            { "for", TokenType.ForKW },
            { "continue", TokenType.ContinueKW },
            { "break", TokenType.BreakKW },
            { "return", TokenType.ReturnKW },
            { "as", TokenType.AsKW },
            { "extern", TokenType.ExternKW }
        };

        public Lexer() 
        {
            OutputTokens = [];
        }

        public List<Token> Tokenize(string inputText) 
        {
            InputText = inputText;
            Position = 0;
            OutputTokens.Clear();
            ActualLine = ActualColumn = 1;

            while (Position < InputText.Length) 
            {
                Line = ActualLine;
                Column = ActualColumn;
                char current = Peek();
                if (char.IsDigit(current)) TokenizeNumber();
                else if (char.IsWhiteSpace(current) || current == '\0') Next();
                else if (current == '/' && Peek(1) == '/') SkipComment();
                else if (current == '\'') TokenizeCharacter();
                else if (current == '\"') TokenizeString();
                else if (OperatorsAndBracets.Keys.Any(t => t.StartsWith(current))) TokenizeOperator();
                else if (char.IsLetter(current) || current == '_') TokenizeIdentifierOrKeyWord();
                else throw new CompilationException("Неизвестный символ.", Line, Column);
            }
            AddToken(TokenType.EOF);
            return OutputTokens;
        }

        private void TokenizeIdentifierOrKeyWord()
        {
            StringBuilder buffer = new();
            char current = Peek();
            while (char.IsLetterOrDigit(current) || current == '_')
            {
                buffer.Append(current);
                current = Next();
            }
            if (KeyWords.TryGetValue(buffer.ToString(), out var v))
                AddToken(v, buffer.ToString());
            else
                AddToken(TokenType.Identifier, buffer.ToString());
        }
        private void SkipComment()
        {
            char c = Next();
            while (c != '\n') { c = Next(); }
            Next();
        }
        private void TokenizeNumber()
        {
            StringBuilder buffer = new();
            char current = Peek();
            while (char.IsDigit(current))
            {
                buffer.Append(current);
                current = Next();
            }
            AddToken(TokenType.Number, buffer.ToString());
        }
        private void TokenizeOperator()
        {
            StringBuilder buffer = new();
            char current = Peek();
            while (OperatorsAndBracets.Keys.Any(t => t.StartsWith(buffer.ToString() + current)))
            {
                buffer.Append(current);
                current = Next();
            }
            AddToken(OperatorsAndBracets[buffer.ToString()]);
        }
        private void TokenizeCharacter()
        {
            if(Next() == '\\')
            {
                char current = Next();
                AddToken(TokenType.Character, "\\" + current);
                if (Next() != '\'') throw new CompilationException("Ожидается '.", ActualLine, ActualColumn);
            }
            else
            {
                AddToken(TokenType.Character, Peek().ToString());
                if(Next() != '\'') throw new CompilationException("Ожидается '.", ActualLine, ActualColumn);
            }
            Next();
        }
        private void TokenizeString()
        {
            StringBuilder buffer = new();
            char current = Next();
            while (current != '"') 
            {
                if (current == '\n' || current == '\0') throw new CompilationException("Ожидается \".", ActualLine, ActualColumn);
                buffer.Append(current);
                current = Next();
            }
            Next();
            AddToken(TokenType.String, buffer.ToString());
        }

        private char Next()
        {
            Position++;
            char c = Peek();
            if (c == '\n')
            {
                ActualLine++;
                ActualColumn = 0;
            }
            else 
                ActualColumn++;
            return c;
        } 
        private char Peek(int offset = 0)
        {
            int index = Position + offset;
            return index < InputText.Length ? InputText[index] : ' ';
        }
        private void AddToken(TokenType type, string? value = null)
        {
            OutputTokens.Add(new Token(type, value, Line, Column));
        }

    }
}
