namespace VispLang.LexicalAnalysis
{
    public enum TokenType
    {
        Number,
        Identifier,

        Equals, EqualsEquals,
        NotEquals,
        GreatThan, GreatThanEquals,
        LessThan, LessThanEquals,

        Plus,
        Minus,
        PlusPlus, MinusMinus,
        Star,
        Slash,
        Percent,           // %

        And, AndAnd,
        Or, OrOr,
        Xor,
        Tilta,
        ExclamationMark,

        Semicolon,         // ;
        Comma,
        Dot,

        String,
        Character,

        LeftParentheses,   // ( 
        RightParentheses,  // )
        LeftBrace,         // {
        RightBrace,        // }
        LeftSquareBrace,   // [
        RightSquareBrace,  // ]

        LetKW,
        FuncKW,
        IfKW,
        ElseKW,
        WhileKW,
        ForKW,
        SizeOfKW,
        ContinueKW,
        BreakKW,
        ReturnKW,
        AsKW,
        ExternKW,

        EOF
    }
}
