namespace VispLang.SyntacticAnalysis.ASTElements.Expression
{
    public class UnaryExpression : IExpression
    {
        public readonly IExpression Expression;
        public readonly UnaryOperation Operation;
        public readonly int Line, Column;

        public UnaryExpression(IExpression expression, UnaryOperation operation, int line, int column)
        {
            Expression = expression;
            Operation = operation;
            Line = line;
            Column = column;
        }

        public int GetLine() => Line;
        public int GetColumn() => Column;
    }
}
