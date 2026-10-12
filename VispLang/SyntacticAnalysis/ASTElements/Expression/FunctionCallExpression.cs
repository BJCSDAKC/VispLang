namespace VispLang.SyntacticAnalysis.ASTElements.Expression
{
    public class FunctionCallExpression : IExpression
    {
        public readonly List<IExpression> Arguments;
        public readonly IExpression Function;
        public readonly int Line, Column;

        public FunctionCallExpression(IExpression function, List<IExpression> arguments, int line, int column)
        {
            Arguments = arguments;
            Function = function;
            Line = line;
            Column = column;
        }

        public int GetLine() => Line;
        public int GetColumn() => Column;
    }
}
