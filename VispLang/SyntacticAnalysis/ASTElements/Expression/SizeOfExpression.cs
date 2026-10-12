using VispLang.SyntacticAnalysis.ASTElements.Type;

namespace VispLang.SyntacticAnalysis.ASTElements.Expression
{
    public class SizeOfExpression : IExpression
    {
        public readonly IType Type;
        public readonly int Line, Column;

        public SizeOfExpression(IType type, int line, int column)
        {
            Type = type;
            Line = line;
            Column = column;
        }

        public int GetLine() => Line;
        public int GetColumn() => Column;
    }
}
