namespace VispLang.SyntacticAnalysis.ASTElements.Expression
{
    public class BinaryExpression : IExpression
    {
        public IExpression Left, Right;
        public readonly BinaryOperation Operation;
        public readonly int Line, Column;

        public BinaryExpression(IExpression left, IExpression right, BinaryOperation operation, int line, int column)
        {
            Left = left;
            Right = right;
            Operation = operation;
            Line = line;
            Column = column;
        }

        public int GetLine() => Line;
        public int GetColumn() => Column;
    }
}
