using VispLang.SyntacticAnalysis.ASTElements.Expression;

namespace VispLang.SyntacticAnalysis.ASTElements.Statement
{
    public class ReturnStatement : IStatement
    {
        public readonly IExpression? Expression;

        public ReturnStatement(IExpression? expression)
        {
            Expression = expression;
        }
    }
}
