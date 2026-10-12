namespace VispLang.SyntacticAnalysis.ASTElements.Expression
{
    public class DereferenceExpression : IExpression
    {
        public readonly IExpression Expression;
        public readonly int Line, Column;
        public DereferenceExpression(IExpression expression, int line, int column)
        {
            Expression = expression;
            Line = line;
            Column = column;
        }

        public int GetLine() => Line;
        public int GetColumn() => Column;
    }
}
