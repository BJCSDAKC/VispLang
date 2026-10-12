namespace VispLang.SyntacticAnalysis.ASTElements.Expression
{
    public class AdressOfExpression : IExpression
    {
        public readonly IExpression Expression;
        public readonly int Line, Column;

        public AdressOfExpression(IExpression expression, int line, int column) 
        { 
            Expression = expression;
            Line = line;
            Column = column;
        }

        public int GetLine() => Line;
        public int GetColumn() => Column;
    }
}
