using VispLang.SyntacticAnalysis.ASTElements.Type;

namespace VispLang.SyntacticAnalysis.ASTElements.Expression
{
    public class CastExpression : IExpression
    {
        public readonly IExpression Expression;
        public readonly IType Type;
        public readonly int Line, Column;
        public CastExpression(IExpression expression, IType type, int line, int column)
        {
            Expression = expression;
            Type = type;
            Line = line;
            Column = column;
        }

        public int GetLine() => Line;
        public int GetColumn() => Column;
    }
}
