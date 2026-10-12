namespace VispLang.SyntacticAnalysis.ASTElements.Expression
{
    public class VariableExpression : IExpression
    {
        public readonly string Value;
        public readonly int Line, Column;

        public VariableExpression(string value, int line, int column)
        {
            Value = value;
            Line = line;
            Column = column;
        }

        public int GetLine() => Line;
        public int GetColumn() => Column;
    }
}
