namespace VispLang.SyntacticAnalysis.ASTElements.Expression
{
    public class IndexExpression : IExpression
    {
        public readonly IExpression Array, Index;
        public readonly int Line, Column;

        public IndexExpression(IExpression array, IExpression index, int line, int column)
        {
            Array = array;
            Index = index;
            Line = line;
            Column = column;
        }

        public int GetLine() => Line;
        public int GetColumn() => Column;
    }
}
