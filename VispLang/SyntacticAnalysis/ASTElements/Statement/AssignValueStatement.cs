using VispLang.SyntacticAnalysis.ASTElements.Expression;

namespace VispLang.SyntacticAnalysis.ASTElements.Statement
{
    public class AssignValueStatement : IStatement
    {
        public readonly string Variable;
        public readonly IExpression Value;
        public readonly List<IIndexator> Indexators;
        public readonly int Line, Column;

        public AssignValueStatement(string variable, IExpression value, List<IIndexator> indexators, int line, int column)
        {
            Variable = variable;
            Value = value;
            Indexators = indexators;
            Line = line;
            Column = column;
        }
    }
}
