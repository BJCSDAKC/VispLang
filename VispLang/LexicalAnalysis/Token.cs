using System.Runtime.CompilerServices;

namespace VispLang.LexicalAnalysis
{
    public struct Token
    {
        public TokenType Type;
        public string? Value;
        public int Line, Column; 

        public Token(TokenType type, string? value, int line, int column)
        {
            Type = type;
            Value = value;
            Line = line;
            Column = column;
        }

        public override string ToString()
        {
            return Value;
        }
    }
}
